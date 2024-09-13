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
                    Solution = incidents.Solution,
                    File = incidents.File
                   

                };
                return await Task.Run(() => View("View", ViewModel));

            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> View(UpdateIncidentViewModel model, IFormFile uploadedFile)
        {
            var incident = await mvcDbContext.IncidentList.FindAsync(model.ID);
            if (incident != null)
            {
                // Обновление свойств инцидента
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
                incident.File = model.File;

                // Обработка загруженного файла
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    try
                    {
                        var fileName = Path.GetFileName(uploadedFile.FileName);

                        // Путь для сохранения файла
                        var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incident.ID.ToString());
                        if (!Directory.Exists(incidentFolder))
                        {
                            Directory.CreateDirectory(incidentFolder);
                        }

                        // Проверка, существует ли файл с таким же именем
                        var filePath = Path.Combine(incidentFolder, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            // Добавляем суффикс, чтобы избежать перезаписи
                            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                            filePath = Path.Combine(incidentFolder, fileName);
                        }

                        // Сохранение файла
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(stream);
                        }

                        // Обновление значения поля File в базе данных
                        incident.File = 1;
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки и возвращение сообщения об ошибке пользователю
                        // Например, можно передать ошибку в TempData и перенаправить пользователя обратно на страницу редактирования
                        TempData["ErrorMessage"] = "Произошла ошибка при загрузке файла. Попробуйте еще раз.";
                        return RedirectToAction("View", new { id = model.ID });
                    }
                }

                await mvcDbContext.SaveChangesAsync();

                // Возврат к виду View для данного инцидента
                return RedirectToAction("View", new { id = model.ID });
            }

            return RedirectToAction("Index");
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult DeleteFile(string fileName, Guid incidentId)
        {
            try
            {
                // Путь к папке с файлами инцидента
                var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incidentId.ToString());

                // Полный путь к файлу
                var filePath = Path.Combine(incidentFolder, fileName);

                // Удаляем файл, если он существует
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Файл не найден." });
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки (если необходимо)
                return Json(new { success = false, message = ex.Message });
            }
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