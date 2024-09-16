using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackingSheet.Models.VSATdata;
using TrackingSheet.Data;
using TrackingSheet.Models;
using TrackingSheet.Models.Domain;
using TrackingSheet.Services;
using System.Text.Json;

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
                    File = incidents.File,
                    DateEnd = incidents.DateEnd
                };
                ViewData["CurrentPage"] = "journal";
                return View("View", ViewModel);
            }

            return RedirectToAction("Index");
        }



        [HttpPost]
        public async Task<IActionResult> View(UpdateIncidentViewModel model, IFormFile uploadedFile)
        {
            var incident = await mvcDbContext.IncidentList.FindAsync(model.ID);
            if (incident != null)
            {
                // Update incident properties
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
                // Do not assign incident.File here; it will be updated after file upload

                // Handle uploaded file
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    try
                    {
                        var fileName = Path.GetFileName(uploadedFile.FileName);

                        // Path to save the file
                        var incidentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", incident.ID.ToString());
                        if (!Directory.Exists(incidentFolder))
                        {
                            Directory.CreateDirectory(incidentFolder);
                        }

                        // Check if a file with the same name exists
                        var filePath = Path.Combine(incidentFolder, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            // Add a suffix to avoid overwriting
                            fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
                            filePath = Path.Combine(incidentFolder, fileName);
                        }

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadedFile.CopyToAsync(stream);
                        }

                        // Update the File field in the database
                        incident.File = 1;
                    }
                    catch (Exception ex)
                    {
                        // Log the error and return an error message to the user
                        TempData["ErrorMessage"] = "An error occurred while uploading the file. Please try again.";
                        return RedirectToAction("View", new { id = model.ID });
                    }
                }

                await mvcDbContext.SaveChangesAsync();

                // Return to the View for this incident
                ViewData["CurrentPage"] = "journal";
                return RedirectToAction("View", new { id = model.ID });
            }
            ViewData["CurrentPage"] = "journal";
            return RedirectToAction("Index");
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
