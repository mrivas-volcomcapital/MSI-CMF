using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSI_CMF.Datos;
using MSI_CMF.Models;
using System.Security.Claims;

namespace MSI_CMF.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly UsuarioDatos _datos = new UsuarioDatos();

        public AccountController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        private string GetConnectionString()
        {
            var cs = _configuration.GetConnectionString("CreditContext");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(ErrorMessages.CadenaConexionNoConfigurada);
            return cs;
        }

        // GET /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.IsDevelopment = _env.IsDevelopment();
            return View();
        }

        // GET /Account/LoginDemo  — Solo disponible en Development
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginDemo(CancellationToken cancellationToken = default)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,      "demo"),
                new Claim(ClaimTypes.GivenName, "Usuario Demo"),
                new Claim(ClaimTypes.Email,     "demo@local.dev"),
                new Claim(ClaimTypes.Role,      "Admin"),
                new Claim("id_usuario",         "0")
            };

            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            return RedirectToAction("Index", "Home");
        }

        // POST /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginModel model,
            string? returnUrl = null,
            CancellationToken cancellationToken = default)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            try
            {
                var cs      = GetConnectionString();
                var usuario = await _datos.ValidarCredencialesAsync(cs, model.usuario, model.password, cancellationToken);

                if (usuario is null)
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,       usuario.usuario),
                    new Claim(ClaimTypes.GivenName,  usuario.nombre),
                    new Claim(ClaimTypes.Email,      usuario.email),
                    new Claim(ClaimTypes.Role,       usuario.rol),
                    new Claim("id_usuario",          usuario.id_usuario.ToString())
                };

                var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = false });

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", ErrorMessages.ErrorBaseDatos);
                return View(model);
            }
        }

        // POST /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET /Account/AccessDenied
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        // GET /Account/CrearUsuario
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CrearUsuario() => View();

        // POST /Account/CrearUsuario
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(
            CrearUsuarioRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return View(request);

            try
            {
                var cs = GetConnectionString();
                await _datos.CrearUsuarioAsync(cs, request, cancellationToken);
                TempData["Mensaje"] = $"Usuario '{request.usuario}' creado correctamente.";
                return RedirectToAction("CrearUsuario");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", ErrorMessages.ErrorBaseDatos);
                return View(request);
            }
        }
    }
}
