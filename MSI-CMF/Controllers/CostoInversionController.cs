using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSI_CMF.Datos;
using MSI_CMF.Models;

namespace MSI_CMF.Controllers
{
    [Authorize]
    public class CostoInversionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly CostoInversionDatos _datos = new CostoInversionDatos();

        public CostoInversionController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private string GetConnectionString()
        {
            var cs = _configuration.GetConnectionString("CreditContext");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(ErrorMessages.CadenaConexionNoConfigurada);
            return cs;
        }

        // GET /CostoInversion
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.AnioActual = DateTime.Now.Year;
            return View(new List<CostoInversionModel>());
        }

        // GET /CostoInversion/GetJson
        [HttpGet]
        public JsonResult GetJson(int? anio = null)
        {
            try
            {
                var cs   = GetConnectionString();
                var data = _datos.ObtenerTodos(cs, anio);
                return Json(new { data });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST /CostoInversion/Guardar
        [HttpPost]
        [Authorize(Roles = "Admin,Mantenedor")]
        public async Task<JsonResult> Guardar(
            [FromBody] GuardarCostoInversionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return Json(new { error = ErrorMessages.ParametrosInvalidos });
            try
            {
                var cs = GetConnectionString();
                await _datos.GuardarAsync(cs, request, cancellationToken);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST /CostoInversion/Eliminar
        [HttpPost]
        [Authorize(Roles = "Admin,Mantenedor")]
        public async Task<JsonResult> Eliminar(
            string runFondo, int periodo, int anio,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(runFondo))
                return Json(new { error = ErrorMessages.ParametrosInvalidos });
            try
            {
                var cs = GetConnectionString();
                await _datos.EliminarAsync(cs, runFondo, periodo, anio, cancellationToken);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST /CostoInversion/ImportarExcel
        [HttpPost]
        [Authorize(Roles = "Admin,Mantenedor")]
        public async Task<JsonResult> ImportarExcel(
            IFormFile archivo,
            CancellationToken cancellationToken = default)
        {
            if (archivo is null || !archivo.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return Json(new { error = ErrorMessages.ArchivoInvalido });
            try
            {
                var cs = GetConnectionString();
                using var stream = archivo.OpenReadStream();
                var (procesados, errores) = await _datos.ImportarExcelAsync(cs, stream, cancellationToken);

                if (errores.Any())
                    return Json(new { error = ErrorMessages.EstructuraExcelInvalida, detalle = errores });

                return Json(new { success = true, procesados });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
