using Microsoft.AspNetCore.Mvc;
using TrackingSheet.Services;
using TrackingSheet.Models.VSATdata;
using System.Threading.Tasks;
using System.Net;

namespace TrackingSheet.Controllers
{
    public class VsatInfoController : Controller
    {



        //внедрение зависимости DI
        private readonly RemoteDataService _remoteDataService;
        private readonly IConfiguration _configuration;

        public VsatInfoController(RemoteDataService remoteDataService, IConfiguration configuration)
        {
            _remoteDataService = remoteDataService;
            _configuration = configuration;
            
        }
        //Выбор номера VSAT
        [HttpGet]
        public IActionResult SelectIPAddress()
        {
            return View();
        }
        //Применение номера VSAT в качестве IP-адреса
        [HttpPost]
        public async Task<IActionResult> SetIpAddress(int ipPart)
        {
            string connectionStringTemplate = _configuration.GetConnectionString("RemoteDatabase");
            string connectionString = connectionStringTemplate.Replace("${IPAddress}", ipPart.ToString());

            _remoteDataService.SetConnectionString(connectionString);
            return RedirectToAction(nameof(GetLatestVsatInfo));
        }

        [HttpGet]
        public async Task<ActionResult<VsatInfo>> GetLatestVsatInfo()
        {
            try
            {
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
