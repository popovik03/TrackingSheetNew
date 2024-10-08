using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class EwsService
{
    private readonly ExchangeService _exchangeService;
    private readonly ILogger<EwsService> _logger;

    public EwsService(IConfiguration configuration, ILogger<EwsService> logger)
    {
        var ewsConfig = configuration.GetSection("Ews");
        string serviceUrl = ewsConfig["ServiceUrl"];
        string username = ewsConfig["Username"];
        string password = ewsConfig["Password"];
        string domain = ewsConfig["Domain"];

        _exchangeService = new ExchangeService(ExchangeVersion.Exchange2013_SP1)
        {
            Credentials = new WebCredentials(username, password, domain),
            Url = new Uri(serviceUrl)
        };

        _logger = logger;
    }

    // Получение событий календаря
    //public async Task<List<Appointment>> GetCalendarEventsAsync(DateTime startDate, DateTime endDate)
    //{
    //    try
    //    {
    //        FindItemsResults<Item> findResults = await _exchangeService.FindAppointments(
    //            WellKnownFolderName.Calendar,
    //            new CalendarView(startDate, endDate)
    //        ).ConfigureAwait(false);

    //        return findResults.Items.OfType<Appointment>().ToList();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Ошибка при получении событий календаря.");
    //        throw;
    //    }
    //}

    // Добавление события в календарь
    //public async Task CreateCalendarEventAsync(string subject, DateTime start, DateTime end, string body = "")
    //{
    //    try
    //    {
    //        Appointment appointment = new Appointment(_exchangeService)
    //        {
    //            Subject = subject,
    //            Body = new MessageBody(body),
    //            Start = start,
    //            End = end,
    //            Location = "Офис",
    //            IsAllDayEvent = false
    //        };

    //        await appointment.Save(WellKnownFolderName.Calendar).ConfigureAwait(false);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Ошибка при создании события календаря.");
    //        throw;
    //    }
    //}

    // Дополнительные методы для обновления и удаления событий могут быть добавлены здесь
}
