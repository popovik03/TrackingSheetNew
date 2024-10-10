using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TrackingSheet.Data;
using TrackingSheet.Models.Telegram;
using Telegram.Bot.Polling;

namespace TrackingSheet.Services.TelegramService
{
    public class TelegramLongPollingService : BackgroundService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramLongPollingService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TelegramLongPollingService(
            ITelegramBotClient botClient,
            ILogger<TelegramLongPollingService> logger,
            IServiceProvider serviceProvider)
        {
            _botClient = botClient;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _logger.LogInformation("TelegramLongPollingService initialized.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExecuteAsync started.");
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Получать все обновления
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Telegram Bot started receiving messages.");

            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            _logger.LogInformation("HandleUpdateAsync called.");

            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {
                var message = update.Message;
                _logger.LogInformation($"Received message from {message.From.Username}: {message.Text}");

               
                // Сохранение сообщения в базу данных
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<MVCDbContext>();
                        var telegramMessage = new TelegramMessage
                        {
                            ChatId = message.Chat.Id,
                            Username = message.From.Username,
                            Text = message.Text,
                            Date = message.Date
                        };
                        dbContext.TelegramMessages.Add(telegramMessage);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        _logger.LogInformation("Message saved to database.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении сообщения в базу данных.");
                }
            }
        }


        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Telegram Bot encountered an error.");
            return Task.CompletedTask;
        }
    }
}
