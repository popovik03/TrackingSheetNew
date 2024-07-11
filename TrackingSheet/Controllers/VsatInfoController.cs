using Microsoft.AspNetCore.Mvc;
using TrackingSheet.Services;

namespace TrackingSheet.Controllers
{
    public class VsatInfoController : Controller
    {
        public async Task <ActionResult> VsatInfo()
        {
            
            return View();
        }
    }
}
