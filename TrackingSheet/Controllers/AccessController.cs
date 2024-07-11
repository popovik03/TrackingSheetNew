using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TrackingSheet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace TrackingSheet.Controllers
{
    public class AccessController : Controller
    {
        
        public IActionResult Login()
        {
            TempData["Login"] = "";
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
                 
                return RedirectToAction("Index", "Home");

            return View();
        }

        

        [HttpPost]
        public async Task<IActionResult> Login(VMLogin modelLogin)
        {
            var users = new Dictionary<string, string>
            {
                {"admin", "admin"},
                {"popovik01", "popovik01"},
                {"khokrom","khokrom" },
                {"utebtim", "utebtim"},
                {"mostvic","mostvic" },
                {"polomak","polomak" },
                {"kravale","kravale" },
                {"konoili","konoili" },
                {"yagudam","yagudam" },
                {"pushvla","pushvla" },
                {"bulyrom","bulyrom" }
            };

            if (users.TryGetValue(modelLogin.Login431, out var expectedPassword) && expectedPassword == modelLogin.PassWord)
            {
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, modelLogin.Login431),
                    new Claim("OtherProperties", "Example Role")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLogged
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                TempData["Login"] = modelLogin.Login431;

                //HttpContext.Session.SetString("Logged", modelLogin.Login431); 
                //var logged = HttpContext.Session.GetString("Logged");
                //вернусь когда разберусь с сессиями

                return RedirectToAction("Index", "Home");
                
            }

            
            ViewData["ValidateMessage"] = "Пользователь не найден";
            
            return View();
        }



        

    }
}
