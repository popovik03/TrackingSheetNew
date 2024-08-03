using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrackingSheet.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using TrackingSheet.Data;
using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.Domain;
using System.Collections.Generic;
using System.Linq;




namespace TrackingSheet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        private readonly MVCDbContext _context;

        public HomeController(ILogger<HomeController> logger, MVCDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        /// &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        ///  &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&>

        public async Task <IActionResult> Index()
        {

            /// Сводка за сутки
            DateTime last36Hours = DateTime.Now.AddHours(-36);
            var recentData = await _context.IncidentList
                .Where(d => d.Date >= last36Hours)
                .ToListAsync();
            var allData = await _context.IncidentList.ToListAsync();





            /// Статистика по кварталам
            /// Q1
            DateTime firstQuarterStartDate = new DateTime(2024, 1, 1); // Начало первого квартала 2024 года
            DateTime firstQuarterEndDate = new DateTime(2024, 3, 31); // Конец первого квартала 2024 года

            
            var firstQuarterIncidents = await _context.IncidentList
                .Where(i => i.Date >= firstQuarterStartDate && i.Date <= firstQuarterEndDate)
                .GroupBy(i => i.ProblemType)
                .Select(g => new ProblemTypeStatisticsViewModel
                {
                    ProblemType = g.Key,
                    Count = g.Count(),
                    SuccessCount = g.Count(i => i.Status  == "Success"),
                    FailCount = g.Count(i => i.Status  == "Fail"),
                    TotalSuccessFailCount = g.Count(i => i.Status == "Success") + g.Count(i => i.Status == "Fail"),
                    SavedNPTCount = (int)g.Sum(i => i.SavedNPT) //в базе long - предложил такой вариант, без этого не хотел
                })
                .ToListAsync();

            int firstQuartertotalProblemTypes = firstQuarterIncidents.Sum(i=>i.Count);
            int firstQuartertotalClosedCount = firstQuarterIncidents.Sum(i => i.SuccessCount) + firstQuarterIncidents.Sum(i => i.FailCount);
            int firstQuartertotalSavedNPT = firstQuarterIncidents.Sum(i => i.SavedNPTCount);

            ///Q2

            DateTime secondQuarterStartDate = new DateTime(2024, 4, 1); // Начало первого квартала 2024 года
            DateTime secondQuarterEndDate = new DateTime(2024, 6, 30); // Конец первого квартала 2024 года

            var secondQuarterIncidents = await _context.IncidentList
                .Where(i => i.Date >= secondQuarterStartDate && i.Date <= secondQuarterEndDate)
                .GroupBy(i => i.ProblemType)
                .Select(g => new ProblemTypeStatisticsViewModel
                {
                    ProblemType = g.Key,
                    Count = g.Count(),
                    SuccessCount = g.Count(i => i.Status == "Success"),
                    FailCount = g.Count(i => i.Status == "Fail"),
                    TotalSuccessFailCount = g.Count(i => i.Status == "Success") + g.Count(i => i.Status == "Fail"),
                    SavedNPTCount = (int)g.Sum(i => i.SavedNPT) //в базе long - предложил такой вариант, без этого не хотел
                })
                .ToListAsync();

            int secondQuartertotalProblemTypes = secondQuarterIncidents.Sum(i => i.Count);
            int secondQuartertotalClosedCount = secondQuarterIncidents.Sum(i => i.SuccessCount) + secondQuarterIncidents.Sum(i => i.FailCount);
            int secondQuartertotalSavedNPT = secondQuarterIncidents.Sum(i => i.SavedNPTCount);

            /// Модель
            var model = new CombinedDataViewModel
            {
                RecentIncidents = recentData,
                AllIncidents = allData,
                FirstQuarterStatistics = firstQuarterIncidents,
                FirstQuarterSTotalProblemTypes = firstQuartertotalProblemTypes,
                FirstQuarterSTotalClosedCount = firstQuartertotalClosedCount,
                FirstQuarterSTotalSavedNPT = firstQuartertotalSavedNPT,
                SecondQuarterStatistics = secondQuarterIncidents,
                SecondQuarterSTotalProblemTypes = secondQuartertotalProblemTypes,
                SecondQuarterSTotalClosedCount = secondQuartertotalClosedCount,
                SecondQuarterSTotalSavedNPT = secondQuartertotalSavedNPT

            };

            
            return View(model);
        }

        /// &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        /// &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        ///   ПЕРЕДАЧА ДАННЫХ в JSON

        [HttpGet]
        public async Task<ActionResult> GetFirstQuarterStatistics()
        {
            DateTime startDate = new DateTime(2024, 1, 1); // Начало первого квартала 2024 года
            DateTime endDate = new DateTime(2024, 3, 31); // Конец первого квартала 2024 года

            var firstQuarterIncidents = await _context.IncidentList
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .GroupBy(i => i.ProblemType)
                .Select(g => new
                {
                    ProblemType = g.Key,
                    Count = g.Count(),
                    SuccessCount = g.Count(i => i.Status == "Success"),
                    FailCount = g.Count(i => i.Status == "Fail"),
                    TotalSuccessFailCount = g.Count(i => i.Status == "Success") + g.Count(i => i.Status == "Fail"),
                    SavedNPTCount = (int)g.Sum(i => i.SavedNPT) //в базе long - предложил такой вариант, без этого не хотел
                })
                .ToListAsync();

            int totalProblemTypes = firstQuarterIncidents.Sum(i => i.Count);
            int totalClosedCount = firstQuarterIncidents.Sum(i => i.SuccessCount) + firstQuarterIncidents.Sum(i => i.FailCount);
            int totalSavedNPT = firstQuarterIncidents.Sum(i => i.SavedNPTCount);

            return Json(firstQuarterIncidents);
        }

        /// &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        /// &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
        ///  &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&>



        public IActionResult Privacy()
        {
            return View();
        }

        public async Task <IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
