using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TrackingSheet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;

namespace TrackingSheet.Controllers
{
    public class AccessController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public AccessController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Login()
        {
            // Проверка, аутентифицирован ли пользователь
            if (User.Identity.IsAuthenticated)
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
                {"khokromn","khokromn" },
                {"utebtim", "utebtim"},
                {"mostvic","mostvic" },
                {"polomak","polomak" },
                {"kravale","kravale" },
                {"konoili","konoili" },
                {"yagudam","yagudam" },
                {"pushvla","pushvla" },
                {"bulyrom","bulyrom" },
                {"shulgdmi","shulgdmi" }
            };

            if (users.TryGetValue(modelLogin.Login431, out var expectedPassword) && expectedPassword == modelLogin.PassWord)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, modelLogin.Login431),
                    new Claim(ClaimTypes.NameIdentifier, modelLogin.Login431),
                    // Добавьте другие клеймы, если необходимо
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLogged,
                    // Можно добавить другие свойства, например, время истечения
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ValidateMessage"] = "Пользователь не найден";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Access");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (avatar != null && avatar.Length > 0)
            {
                // Проверка типа файла (разрешить только изображения)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(avatar.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ValidateMessage"] = "Недопустимый тип файла. Разрешены только изображения.";
                    return RedirectToAction("Index", "Home");
                }

                // Получение имени пользователя
                var loggedUser = User.Identity.Name;

                // Путь для сохранения файла
                var avatarsPath = Path.Combine(_environment.WebRootPath, "avatars");
                if (!Directory.Exists(avatarsPath))
                {
                    Directory.CreateDirectory(avatarsPath);
                }

                var filePath = Path.Combine(avatarsPath, $"{loggedUser}{extension}");

                // Сохранение файла, перезаписывая существующий
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                // Удаление старых файлов с другими расширениями
                var existingFiles = Directory.GetFiles(avatarsPath, $"{loggedUser}.*")
                                             .Where(f => f != filePath);

                foreach (var existingFile in existingFiles)
                {
                    System.IO.File.Delete(existingFile);
                }

                TempData["ValidateMessage"] = "Фото профиля успешно обновлено.";
            }
            else
            {
                TempData["ValidateMessage"] = "Файл не выбран.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
