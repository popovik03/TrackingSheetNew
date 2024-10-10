using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace TrackingSheet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramMessagesController : ControllerBase
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramMessagesController(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { status = "Сообщение не может быть пустым." });
            }

            try
            {
                await _botClient.SendTextMessageAsync(
                    chatId: "-1001795494804", // Замените на ID вашей группы
                    text: request.Text
                );

                return Ok(new { status = "Сообщение отправлено успешно." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = $"Ошибка при отправке сообщения: {ex.Message}" });
            }
        }
    }

    public class SendMessageRequest
    {
        public string Text { get; set; }
    }
}
