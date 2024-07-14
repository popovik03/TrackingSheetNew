using Microsoft.AspNetCore.Mvc;
using TrackingSheet.Services;
using TrackingSheet.Models.VSATdata;
using System.Threading.Tasks;

namespace TrackingSheet.Controllers
{
    public class VsatInfoController : Controller
    {
        private readonly RemoteDataService _remoteDataService;
        private readonly IConfiguration _configuration;

        public VsatInfoController(RemoteDataService remoteDataService, IConfiguration configuration)
        {
            _remoteDataService = remoteDataService;
            _configuration = configuration;
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

            // Сохранение строки подключения в сессии в HttpContext.Seesion, до этого передавал напрямую через SetConnectionString, но кажется она не сохраняла ее между вызовами
            HttpContext.Session.SetString("RemoteDbConnectionString", connectionString);

            return RedirectToAction(nameof(GetLatestVsatInfo));
        }

        [HttpGet]
        public async Task<IActionResult> GetLatestVsatInfo()
        {
            try
            {
                // Получение строки подключения из сессии которую создал выше
                string connectionString = HttpContext.Session.GetString("RemoteDbConnectionString");
                _remoteDataService.SetConnectionString(connectionString);

                VsatInfo vsatInfo = await _remoteDataService.GetLatestVsatInfoAsync();
                if (vsatInfo == null)
                {
                    return NotFound();
                }
                return Ok(vsatInfo);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Exception occurred: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }
}
