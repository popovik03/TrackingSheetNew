using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TrackingSheet.Data;
using TrackingSheet.Models;
using TrackingSheet.Models.Domain;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrackingSheet.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly MVCDbContext mvcDbContext;

        public IncidentsController(MVCDbContext mvcDbContext) 
        {
            this.mvcDbContext = mvcDbContext;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var incidents = await mvcDbContext.IncidentList.OrderByDescending(p=> p.Date).ToListAsync();

            return View(incidents);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(AddIncidentViewModel addIncidentRequest)
        {
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
            //return RedirectToAction("Add");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var incidents = await mvcDbContext.IncidentList.FirstOrDefaultAsync(x => x.ID == id);

            if (incidents !=null)
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
                return await Task.Run(() =>View("View", ViewModel));

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
        [HttpPost]
        public async Task<IActionResult> Delete(UpdateIncidentViewModel model)
        {
            var incident = await mvcDbContext.IncidentList.FindAsync(model.ID);
            
            if (incident != null)
            {
               
                mvcDbContext.IncidentList.Remove(incident);
                await mvcDbContext.SaveChangesAsync();
                TempData["AlertMessage"] = "Инцидент удален";
                return RedirectToAction("Index");

            }

            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> GetIncidents(int page = 1, int pageSize = 50)
        {
            var incidents = await mvcDbContext.IncidentList
                .OrderByDescending(p => p.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Json(incidents);
        }



    }

}
