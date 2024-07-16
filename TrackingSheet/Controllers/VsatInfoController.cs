using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NuGet.Packaging.Signing;
using System;
using System.Threading.Tasks;
using TrackingSheet.Models.VSATdata;
using TrackingSheet.Services;

namespace TrackingSheet.Controllers
{
    public class VsatInfoController : Controller
    {
        

        private readonly RemoteDataService _remoteDataService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VsatInfoController> _logger;

        public VsatInfoController(RemoteDataService remoteDataService, IConfiguration configuration, ILogger<VsatInfoController> logger)
        {
            _remoteDataService = remoteDataService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SelectIPAddress()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetIpAddress(int ipPart)
        {
            string connectionStringTemplate = _configuration.GetConnectionString("RemoteDatabase");
            string connectionString = connectionStringTemplate.Replace("${IPAddress}", ipPart.ToString());

            // Сохранение строки подключения в сессии
            HttpContext.Session.SetString("RemoteDbConnectionString", connectionString);
            TempData["ipPart"] = ipPart;

            return RedirectToAction(nameof(GetLatestVsatInfo));
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestVsatInfo()
        {
            try
            {
                // Получение строки подключения из сессии
                string connectionString = HttpContext.Session.GetString("RemoteDbConnectionString");
                _remoteDataService.SetConnectionString(connectionString);

                // Получение данных VSAT
                VsatInfo vsatInfo = await _remoteDataService.GetLatestVsatInfoAsync();

                if (vsatInfo == null)
                {
                    return NotFound();
                }

                return View(vsatInfo);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                _logger.LogError(ex, "Exception occurred while getting latest VSAT info.");
                string status_message = string.Format("Получение данных по адресу {0}", HttpContext.Session.GetString("RemoteDbConnectionString"));
                return StatusCode(500, status_message);
            }
        }
    }
}
