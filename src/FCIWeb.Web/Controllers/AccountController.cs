using System.Threading.Tasks;
using FCIWeb.Web.Models;
using FCIWeb.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCIWeb.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly DemoUserStore _users;
        private readonly LoginAbuseGuard _loginAbuseGuard;

        public AccountController(DemoUserStore users, LoginAbuseGuard loginAbuseGuard)
        {
            _users = users;
            _loginAbuseGuard = loginAbuseGuard;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var remoteAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (_loginAbuseGuard.IsLocked(model.Username, remoteAddress))
            {
                ModelState.AddModelError(string.Empty, "Accesso temporaneamente bloccato. Attendere prima di riprovare.");
                return View(model);
            }

            var principal = _users.Authenticate(model.Username, model.Password);
            if (principal == null)
            {
                _loginAbuseGuard.RecordFailure(model.Username, remoteAddress);
                ModelState.AddModelError(string.Empty, "Credenziali non valide.");
                return View(model);
            }

            _loginAbuseGuard.RecordSuccess(model.Username, remoteAddress);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Richieste");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Denied() => View();
    }
}
