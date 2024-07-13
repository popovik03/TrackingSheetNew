using Microsoft.AspNetCore.Mvc;
using TrackingSheet.Services;
using TrackingSheet.Models.VSATdata;
using System.Threading.Tasks;

namespace TrackingSheet.Controllers
{
    public class VsatInfoController : Controller
    {
        //внедрение зависимости DI
        private readonly RemoteDataService _remoteDataService; 
        public VsatInfoController(RemoteDataService remoteDataService)
        {
            _remoteDataService = remoteDataService;
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
