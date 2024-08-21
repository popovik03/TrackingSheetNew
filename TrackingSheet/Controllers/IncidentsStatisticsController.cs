using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TrackingSheet.Services;
using TrackingSheet.Models;

namespace TrackingSheet.Controllers
{

    public class IncidentsStatisticsController : Controller
    {
        private readonly QuarterYearStatisticsService _quarterYearStatisticsService;

        public IncidentsStatisticsController(QuarterYearStatisticsService quarterYearStatisticsService)
        {
            _quarterYearStatisticsService = quarterYearStatisticsService;
        }

        [Authorize]
        [HttpGet]
        public IActionResult SelectPeriod()
        {
            ViewData["CurrentPage"] = "statistics";
            return View();
            
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> IncidentsStatistics(int year, int quarter)
        {
            var incidentsStatistics = await _quarterYearStatisticsService.GetIncidentStatisticsASync(year, quarter);
            ViewData["CurrentPage"] = "statistics";
            return View(incidentsStatistics);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> IncidentsStatisticsJson(int year, int quarter)
        {
            var incidentsStatisticsJson = await _quarterYearStatisticsService.GetIncidentStatisticsASync(year, quarter);
            return Json(incidentsStatisticsJson);
        }

    }
}
