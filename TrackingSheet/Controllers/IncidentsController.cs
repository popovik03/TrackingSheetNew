using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.VSATdata;
using TrackingSheet.Data;
using TrackingSheet.Models;
using TrackingSheet.Models.Domain;
using TrackingSheet.Services;
using System.Text.Json;
using NuGet.Packaging.Signing;



namespace TrackingSheet.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly MVCDbContext mvcDbContext;
        private readonly RemoteDataService _remoteDataService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public IncidentsController(MVCDbContext mvcDbContext, RemoteDataService remoteDataService, IConfiguration configuration, ILogger<IncidentsController> logger)
        {
            this.mvcDbContext = mvcDbContext;
            _remoteDataService = remoteDataService;
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var incidents = await mvcDbContext.IncidentList.OrderByDescending(p => p.Date).ToListAsync();
            ViewData["CurrentPage"] = "journal";
            return View(incidents);
        }






        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            ViewData["CurrentPage"] = "new_incident";

            // Получение данных VSAT из TempData
            var vsatInfoJson = TempData["VsatInfo"] as string;
            VsatInfo vsatInfo = string.IsNullOrEmpty(vsatInfoJson) ? null : JsonSerializer.Deserialize<VsatInfo>(vsatInfoJson);

            // Если данные VSAT получены, заполнить модель
            var model = new AddIncidentViewModel
            {
                Well = vsatInfo?.WELL_NAME ?? "Test",
                Run = vsatInfo?.MWRU_NUMBER ?? 100
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SetIpAddressAndGetLatestVsatInfo(int ipPart)
        {
            string connectionStringTemplate = _configuration.GetConnectionString("RemoteDatabase");
            string connectionString = connectionStringTemplate.Replace("${IPAddress}", ipPart.ToString());

            // Сохранение строки подключения в сессии
            HttpContext.Session.SetString("RemoteDbConnectionString", connectionString);
            TempData["ipPart"] = ipPart;

            try
            {
                // Получение строки подключения из сессии
                string connectionStringNew = HttpContext.Session.GetString("RemoteDbConnectionString");
                _remoteDataService.SetConnectionString(connectionStringNew);

                // Получение данных VSAT
                VsatInfo vsatInfo = await _remoteDataService.GetLatestVsatInfoAsync();

                if (vsatInfo == null)
                {
                    return NotFound();
                }

                ViewBag.CurrentPage = "new_incident";
                ViewBag.VsatInfo = vsatInfo;  // Передача данных в ViewBag
                return View("Add");  // Отображение представления
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting latest VSAT info.");
                string status_message = string.Format("Получение данных по адресу {0}", HttpContext.Session.GetString("RemoteDbConnectionString"));
                ViewBag.ErrorMessage = status_message;  // Передача сообщения об ошибке
                return View("ErrorView");  // Отображение представления ошибки
            }

        }



        [Authorize]
        [HttpPost]

        public async Task<IActionResult> Add(AddIncidentViewModel addIncidentRequest)
        {

            addIncidentRequest.VSAT = addIncidentRequest.IpPart; //Присваиваем значение ip-part и номер VSAT

            var incident = new Incidents()
            {
                ID = Guid.NewGuid(),
                Date = addIncidentRequest.Date,
                Shift = addIncidentRequest.Shift,
                Reporter = addIncidentRequest.Reporter,
                VSAT = addIncidentRequest.VSAT,
                Well = addIncidentRequest.Well,
                Run = addIncidentRequest.Run,
                SavedNPT = addIncidentRequest.SavedNPT,
                ProblemType = addIncidentRequest.ProblemType,
                HighLight = addIncidentRequest.HighLight,
                Status = addIncidentRequest.Status,
                Solution = addIncidentRequest.Solution,

            };

            await mvcDbContext.IncidentList.AddAsync(incident);
            await mvcDbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var incidents = await mvcDbContext.IncidentList.FirstOrDefaultAsync(x => x.ID == id);

            if (incidents != null)
            {
                var ViewModel = new UpdateIncidentViewModel()
                {
                    ID = incidents.ID,
                    Date = incidents.Date,
                    Shift = incidents.Shift,
                    Reporter = incidents.Reporter,
                    VSAT = incidents.VSAT,
                    Well = incidents.Well,
                    Run = incidents.Run,
                    SavedNPT = incidents.SavedNPT,
                    ProblemType = incidents.ProblemType,
                    HighLight = incidents.HighLight,
                    Status = incidents.Status,
                    Solution = incidents.Solution
                };
                return await Task.Run(() => View("View", ViewModel));

            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> View(UpdateIncidentViewModel model)
        {
            var incident = await mvcDbContext.IncidentList.FindAsync(model.ID);
            if (incident != null)
            {
                incident.Date = model.Date;
                incident.Shift = model.Shift;
                incident.Reporter = model.Reporter;
                incident.VSAT = model.VSAT;
                incident.Well = model.Well;
                incident.Run = model.Run;
                incident.SavedNPT = model.SavedNPT;
                incident.ProblemType = model.ProblemType;
                incident.HighLight = model.HighLight;
                incident.Status = model.Status;
                incident.Solution = model.Solution;
                await mvcDbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Удаление записи с пустым идентификатором.");
                TempData["AlertMessage"] = "Некорректный идентификатор.";
                return RedirectToAction("Index");
            }

            var incident = await mvcDbContext.IncidentList.FindAsync(id);

            if (incident != null)
            {
                mvcDbContext.IncidentList.Remove(incident);
                await mvcDbContext.SaveChangesAsync();
                TempData["AlertMessage"] = "Инцидент удален";
                _logger.LogInformation("Инцидент успешно удален.");
            }
            else
            {
                _logger.LogWarning($"Инцидент с ID {id} не найден для удаления.");
                TempData["AlertMessage"] = "Инцидент не найден";
            }

            return RedirectToAction("Index");
        }


        [Authorize]
        [HttpGet("api/incidents/all")]
        public async Task<IActionResult> GetAllIncidents()
        {
            var data = await mvcDbContext.IncidentList.ToListAsync();
            var totalRecords = data.Count;

            return Json(new
            {
                draw = 1,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }





    }

}