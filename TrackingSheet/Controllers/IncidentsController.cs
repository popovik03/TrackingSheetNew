using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.VSATdata;
using TrackingSheet.Data;
using TrackingSheet.Models;
using TrackingSheet.Models.Domain;
using TrackingSheet.Services;
using System.Text.Json;
using TrackingSheet.Models.DTO;

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
        public async Task<IActionResult> View(Guid id)
        {
            var incidents = await mvcDbContext.IncidentList
                .Include(i => i.Updates)
                .FirstOrDefaultAsync(x => x.ID == id);

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
                    File = incidents.File,
                    DateEnd = incidents.DateEnd,
                    Updates = incidents.Updates?.ToList() // Передаем обновления в представление
                };
                ViewData["CurrentPage"] = "journal";
                return View("View", ViewModel);
            }

            return RedirectToAction("Index");
        }


        
        //Передача данных из базы в DataTable в формате json
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

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> UpdateIncidents([FromBody] List<UpdateIncidentsDTO> updatedIncidents)
        {
            if (updatedIncidents == null || !updatedIncidents.Any())
            {
                _logger.LogWarning("UpdateIncidents: Нет данных для обновления.");
                return BadRequest(new { message = "Нет данных для обновления." });
            }

            try
            {
                foreach (var updatedIncident in updatedIncidents)
                {
                    if (updatedIncident.ID == Guid.Empty)
                    {
                        _logger.LogWarning("UpdateIncidents: Пропуск записи с пустым ID.");
                        continue; // Пропускаем некорректные записи
                    }

                    var incident = await mvcDbContext.IncidentList
                        .FirstOrDefaultAsync(i => i.ID == updatedIncident.ID);

                    if (incident == null)
                    {
                        _logger.LogWarning($"UpdateIncidents: Инцидент с ID {updatedIncident.ID} не найден.");
                        continue; // Пропускаем отсутствующие записи
                    }

                    // Обновляем только изменённые поля
                    incident.Date = updatedIncident.Date;
                    incident.Shift = updatedIncident.Shift;
                    incident.Reporter = updatedIncident.Reporter;
                    incident.VSAT = updatedIncident.VSAT;
                    incident.Well = updatedIncident.Well;
                    incident.Run = updatedIncident.Run;
                    incident.SavedNPT = updatedIncident.SavedNPT;
                    incident.ProblemType = updatedIncident.ProblemType;
                    incident.HighLight = updatedIncident.HighLight;
                    incident.Status = updatedIncident.Status;
                    incident.Solution = updatedIncident.Solution;
                    incident.File = updatedIncident.File;
                    incident.DateEnd = updatedIncident.DateEnd;
                    // Обновите другие поля по необходимости
                }

                await mvcDbContext.SaveChangesAsync();
                _logger.LogInformation("UpdateIncidents: Данные успешно обновлены.");
                return Ok(new { message = "Данные успешно обновлены." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateIncidents: Ошибка при массовом обновлении инцидентов.");
                return StatusCode(500, new { message = "Произошла ошибка при сохранении данных." });
            }
        }



        //_________________________Методы для добавления, редактирования и удаления инцидентов_______________________________//


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(AddIncidentViewModel addIncidentRequest)
        {
            addIncidentRequest.VSAT = addIncidentRequest.IpPart; // Assign IpPart to VSAT

            var incident = new Incidents()
            {
                ID = addIncidentRequest.ID, // Use the ID from the model
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

            // Save the incident to the database
            await mvcDbContext.IncidentList.AddAsync(incident);
            await mvcDbContext.SaveChangesAsync();

            // Handle uploaded files
            if (addIncidentRequest.UploadedFiles != null && addIncidentRequest.UploadedFiles.Count > 0)
            {
                var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incident.ID.ToString());
                if (!Directory.Exists(incidentFolder))
                {
                    Directory.CreateDirectory(incidentFolder);
                }

                foreach (var file in addIncidentRequest.UploadedFiles)
                {
                    if (file != null && file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(incidentFolder, fileName);

                        // Ensure unique file names
                        if (System.IO.File.Exists(filePath))
                        {
                            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                            filePath = Path.Combine(incidentFolder, fileName);
                        }

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }

                // Update the incident to indicate that files are attached
                incident.File = 1;
                await mvcDbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Incidents");
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateIncidentViewModel model, IEnumerable<IFormFile> UploadedFiles)
        {
            if (model == null)
            {
                _logger.LogWarning("Edit: Model is null.");
                return BadRequest(new { success = false, message = "Invalid data." });
            }

            var incident = await mvcDbContext.IncidentList.FindAsync(model.ID);
            if (incident == null)
            {
                _logger.LogWarning($"Edit: Incident with ID {model.ID} not found.");
                return NotFound(new { success = false, message = "Incident not found." });
            }

            try
            {
                // Обновление свойств инцидента
                incident.Date = model.Date;
                incident.DateEnd = model.DateEnd;
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
                // Не присваиваем incident.File здесь; оно будет обновлено после загрузки файлов

                _logger.LogInformation($"Edit: Updating incident {model.ID}.");

                // Обработка загруженных файлов
                if (UploadedFiles != null && UploadedFiles.Any())
                {
                    var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incident.ID.ToString());
                    if (!Directory.Exists(incidentFolder))
                    {
                        Directory.CreateDirectory(incidentFolder);
                        _logger.LogInformation($"Edit: Created folder {incidentFolder}.");
                    }

                    foreach (var uploadedFile in UploadedFiles)
                    {
                        if (uploadedFile != null && uploadedFile.Length > 0)
                        {
                            var fileName = Path.GetFileName(uploadedFile.FileName);
                            var filePath = Path.Combine(incidentFolder, fileName);

                            // Проверка на существование файла и добавление уникального идентификатора
                            if (System.IO.File.Exists(filePath))
                            {
                                fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                                filePath = Path.Combine(incidentFolder, fileName);
                                _logger.LogWarning($"Edit: File {fileName} already exists. Renaming to {fileName}.");
                            }

                            // Сохранение файла
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await uploadedFile.CopyToAsync(stream);
                                _logger.LogInformation($"Edit: Saved file {filePath}.");
                            }
                        }
                    }

                    // Обновление поля File, если файлы загружены
                    incident.File = 1;
                    _logger.LogInformation($"Edit: Updated File field for incident {model.ID}.");
                }

                await mvcDbContext.SaveChangesAsync();
                _logger.LogInformation($"Edit: Incident {model.ID} successfully updated.");

                TempData["SuccessMessage"] = "Инцидент успешно обновлен.";
                return RedirectToAction("View", new { id = model.ID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Edit: Error updating incident {model.ID}.");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении инцидента.");
                return View(model);
            }
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Attempted to delete a record with an empty ID.");
                TempData["AlertMessage"] = "Invalid identifier.";
                return RedirectToAction("Index");
            }

            var incident = await mvcDbContext.IncidentList.FindAsync(id);

            if (incident != null)
            {
                mvcDbContext.IncidentList.Remove(incident);
                await mvcDbContext.SaveChangesAsync();
                TempData["AlertMessage"] = "Incident deleted.";
                _logger.LogInformation("Incident successfully deleted.");
            }
            else
            {
                _logger.LogWarning($"Incident with ID {id} not found for deletion.");
                TempData["AlertMessage"] = "Incident not found.";
            }

            return RedirectToAction("Index");
        }




        //___________________________________Метод для получения информации из базы данных____________________________________________//
        [HttpPost]
        public async Task<IActionResult> SetIpAddressAndGetLatestVsatInfo(int ipPart)
        {
            string connectionStringTemplate = _configuration.GetConnectionString("RemoteDatabase");
            string connectionString = connectionStringTemplate.Replace("${IPAddress}", ipPart.ToString());

            // Save the connection string in session
            HttpContext.Session.SetString("RemoteDbConnectionString", connectionString);
            TempData["ipPart"] = ipPart;

            try
            {
                // Retrieve the connection string from session
                string connectionStringNew = HttpContext.Session.GetString("RemoteDbConnectionString");
                _remoteDataService.SetConnectionString(connectionStringNew);

                // Get VSAT data
                VsatInfo vsatInfo = await _remoteDataService.GetLatestVsatInfoAsync();

                if (vsatInfo == null)
                {
                    return NotFound();
                }

                // Create the model and pass data
                var model = new AddIncidentViewModel
                {
                    
                    VSAT = ipPart,
                    Well = vsatInfo.WELL_NAME,
                    Run = vsatInfo.MWRU_NUMBER,
                    Date = DateTime.Now,
                    Shift = DateTime.Now.Hour >= 20 || DateTime.Now.Hour < 8 ? "Night" : "Day",
                    Reporter = TempData.Peek("Login") as string ?? string.Empty,
                    IpPart = ipPart,
                    // Initialize other properties as needed
                };

                // Pass VSAT data through ViewBag
                ViewBag.CurrentPage = "new_incident";
                ViewBag.VsatInfo = vsatInfo;

                return View("Add", model); // Pass the model to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting latest VSAT info.");
                string status_message = $"Error retrieving data from {HttpContext.Session.GetString("RemoteDbConnectionString")}";
                return View("Add");
                
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            ViewData["CurrentPage"] = "new_incident";

            // Generate a new ID for the incident
            var newIncidentId = Guid.NewGuid();

            // Retrieve VSAT data from TempData
            var vsatInfoJson = TempData["VsatInfo"] as string;
            VsatInfo vsatInfo = string.IsNullOrEmpty(vsatInfoJson) ? null : JsonSerializer.Deserialize<VsatInfo>(vsatInfoJson);

            // Create the model and pass the generated ID
            var model = new AddIncidentViewModel
            {
                ID = newIncidentId,
                Well = vsatInfo?.WELL_NAME ?? "Test",
                Run = vsatInfo?.MWRU_NUMBER ?? 100,
                Date = DateTime.Now,
                Shift = DateTime.Now.Hour >= 20 || DateTime.Now.Hour < 8 ? "Night" : "Day",
                Reporter = TempData.Peek("Login") as string ?? string.Empty,
                //VSAT = vsatInfo?.IP_PART ?? 0, // Use the correct property from VsatInfo
                //IpPart = vsatInfo?.IP_PART ?? 0,
                // Initialize other properties as needed
            };

            // Pass VSAT data through ViewBag
            ViewBag.VsatInfo = vsatInfo;

            return View(model);
        }



        // Method to delete temporary files when creating an incident
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult DeleteTempFile(string fileName, Guid incidentId)
        {
            try
            {
                var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incidentId.ToString());
                var filePath = Path.Combine(incidentFolder, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "File not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult DeleteFile(string fileName, Guid incidentId)
        {
            try
            {
                // Path to the incident's file folder
                var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incidentId.ToString());

                // Full path to the file
                var filePath = Path.Combine(incidentFolder, fileName);

                // Delete the file if it exists
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "File not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the error if necessary
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUpdate([FromBody] UpdateIncidentByNewCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var incidentUpdate = new IncidentUpdate
                {
                    ID = Guid.NewGuid(),
                    IncidentID = model.IncidentID,
                    Date = DateTime.Now,
                    UpdateReporter = model.UpdateReporter,
                    UpdateSolution = model.UpdateSolution,
                    Run = model.Run
                };

                await mvcDbContext.IncidentUpdates.AddAsync(incidentUpdate);
                await mvcDbContext.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Ошибка валидации данных" });
        }

    }


}

