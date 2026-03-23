using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSI_CMF.Datos;
using MSI_CMF.Models;

namespace MSI_CMF.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ReportesFondosDatos _datos = new ReportesFondosDatos();

        public ReportesController(IConfiguration configuration)
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

        // ── FONDOS 03 ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Fondos03() => View(new Fondos03ViewModel());

        [HttpGet]
        public async Task<JsonResult> GetFondos03Json(
            DateTime? fecha,
            CancellationToken cancellationToken = default)
        {
            if (fecha is null)
                return Json(new { error = ErrorMessages.FechaRequerida });
            try
            {
                var cs = GetConnectionString();
                var vm = await _datos.ObtenerFondos03Async(cs, fecha.Value, cancellationToken);
                return Json(new
                {
                    registro1 = vm.Registro1,
                    registro2 = vm.Registro2,
                    registro3 = vm.Registro3
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ── FONDOS 05 ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Fondos05() => View(new Fondos05ViewModel());

        [HttpGet]
        public async Task<JsonResult> GetFondos05Json(
            DateTime? fecha,
            CancellationToken cancellationToken = default)
        {
            if (fecha is null)
                return Json(new { error = ErrorMessages.FechaRequerida });
            try
            {
                var cs = GetConnectionString();
                var vm = await _datos.ObtenerFondos05Async(cs, fecha.Value, cancellationToken);
                return Json(new
                {
                    registro1 = vm.Registro1,
                    registro2 = vm.Registro2,
                    registro3 = vm.Registro3
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ── FONDOS 06 ──────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Fondos06() => View(new Fondos06ViewModel());

        [HttpGet]
        public async Task<JsonResult> GetFondos06Json(
            DateTime? fecha,
            CancellationToken cancellationToken = default)
        {
            if (fecha is null)
                return Json(new { error = ErrorMessages.FechaRequerida });
            try
            {
                var cs = GetConnectionString();
                var vm = await _datos.ObtenerFondos06Async(cs, fecha.Value, cancellationToken);
                return Json(new
                {
                    registro1 = vm.Registro1,
                    registro2 = vm.Registro2
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ── EXPORTAR EXCEL 03 ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ExportarExcel03(
            DateTime? fecha,
            CancellationToken cancellationToken = default)
        {
            if (fecha is null) return BadRequest(ErrorMessages.FechaRequerida);
            var cs = GetConnectionString();
            var vm = await _datos.ObtenerFondos03Async(cs, fecha.Value, cancellationToken);
            var tempFile = Path.GetTempFileName();
            using (var wb = new XLWorkbook())
            {
                BuildSheetF03R1(wb, vm.Registro1);
                BuildSheetF03R2(wb, vm.Registro2);
                BuildSheetF03R3(wb, vm.Registro3);
                wb.SaveAs(tempFile);
            }
            var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read,
                FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, ReportesExportConstants.MimeExcel, $"FONDOS03_{fecha.Value:yyyyMMdd}.xlsx");
        }

        private static void BuildSheetF03R1(XLWorkbook wb, List<Fondos03R1Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_01");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF03R1);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.fecha_informacion;
                ws.Cell(r, 3).Value = x.run_fondo;
                ws.Cell(r, 4).Value = x.codigo_serie;
                ws.Cell(r, 5).Value = x.monto_remuneracion_fija;
                ws.Cell(r, 6).Value = x.monto_remuneracion_variable;
                ws.Cell(r, 7).Value = x.monto_costo_inversion_otros_vehiculos;
                ws.Cell(r, 8).Value = x.horizonte_tiempo ?? "";
                ws.Cell(r, 9).Value = x.monto_remuneracion_devuelto;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void BuildSheetF03R2(XLWorkbook wb, List<Fondos03R2Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_02");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF03R2);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.fecha_informacion;
                ws.Cell(r, 3).Value = x.run_fondo;
                ws.Cell(r, 4).Value = x.codigo_RDFondo;
                ws.Cell(r, 5).Value = x.monto_gastos_financieros_afectos;
                ws.Cell(r, 6).Value = x.monto_gastos_financieros_no_afectos;
                ws.Cell(r, 7).Value = x.monto_gastos_operacionales_afectos;
                ws.Cell(r, 8).Value = x.monto_gastos_operacionales_no_afectos;
                ws.Cell(r, 9).Value = x.monto_otros_gastos_afectos;
                ws.Cell(r, 10).Value = x.monto_otros_gastos_no_afectos;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void BuildSheetF03R3(XLWorkbook wb, List<Fondos03R3Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_03");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF03R3);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.fecha_informacion;
                ws.Cell(r, 3).Value = x.run_fondo;
                ws.Cell(r, 4).Value = x.codigo_RDFondo;
                ws.Cell(r, 5).Value = x.tasa_anual_costos;
                ws.Cell(r, 6).Value = x.costo_total_periodo;
                ws.Cell(r, 7).Value = x.promedio_movil;
                ws.Cell(r, 8).Value = x.dias_con_datos;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        // ── EXPORTAR EXCEL 05 ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ExportarExcel05(
            DateTime? fecha,
            CancellationToken cancellationToken = default)
        {
            if (fecha is null) return BadRequest(ErrorMessages.FechaRequerida);
            var cs = GetConnectionString();
            var vm = await _datos.ObtenerFondos05Async(cs, fecha.Value, cancellationToken);
            var tempFile = Path.GetTempFileName();
            using (var wb = new XLWorkbook())
            {
                BuildSheetF05R1(wb, vm.Registro1);
                BuildSheetF05R2(wb, vm.Registro2);
                BuildSheetF05R3(wb, vm.Registro3);
                wb.SaveAs(tempFile);
            }
            var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read,
                FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, ReportesExportConstants.MimeExcel, $"FONDOS05_{fecha.Value:yyyyMMdd}.xlsx");
        }

        private static void BuildSheetF05R1(XLWorkbook wb, List<Fondos05R1Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_01");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF05R1);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.fecha_informacion;
                ws.Cell(r, 3).Value = x.run_fondo;
                ws.Cell(r, 4).Value = x.total_activos;
                ws.Cell(r, 5).Value = x.total_pasivos;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void BuildSheetF05R2(XLWorkbook wb, List<Fondos05R2Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_02");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF05R2);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.fecha_informacion;
                ws.Cell(r, 3).Value = x.run_fondo;
                ws.Cell(r, 4).Value = x.codigo_serie;
                ws.Cell(r, 5).Value = x.patrimonio;
                ws.Cell(r, 6).Value = x.cuotas_circulacion;
                ws.Cell(r, 7).Value = x.valor_cuota;
                ws.Cell(r, 8).Value = x.aportes;
                ws.Cell(r, 9).Value = x.monto_aportes;
                ws.Cell(r, 10).Value = x.rescates;
                ws.Cell(r, 11).Value = x.monto_rescates;
                ws.Cell(r, 12).Value = x.emision_cuotas_vigente ?? "";
                ws.Cell(r, 13).Value = x.tipo_vigencia ?? "";
                ws.Cell(r, 14).Value = x.fondo_liquidacion ?? "";
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void BuildSheetF05R3(XLWorkbook wb, List<Fondos05R3Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_03");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF05R3);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.fecha;
                ws.Cell(r, 3).Value = x.run_fondo;
                ws.Cell(r, 4).Value = x.codigo_serie;
                ws.Cell(r, 5).Value = x.tipo_participe;
                ws.Cell(r, 6).Value = x.numero_participes;
                ws.Cell(r, 7).Value = x.proporcion_patrimonio;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        // ── EXPORTAR EXCEL 06 ──────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ExportarExcel06(
            DateTime? fecha,
            CancellationToken cancellationToken = default)
        {
            if (fecha is null) return BadRequest(ErrorMessages.FechaRequerida);
            var cs = GetConnectionString();
            var vm = await _datos.ObtenerFondos06Async(cs, fecha.Value, cancellationToken);
            var tempFile = Path.GetTempFileName();
            using (var wb = new XLWorkbook())
            {
                BuildSheetF06R1(wb, vm.Registro1);
                BuildSheetF06R2(wb, vm.Registro2);
                wb.SaveAs(tempFile);
            }
            var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read,
                FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, ReportesExportConstants.MimeExcel, $"FONDOS06_{fecha.Value:yyyyMMdd}.xlsx");
        }

        private static void BuildSheetF06R1(XLWorkbook wb, List<Fondos06R1Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_01_Sexo");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF06R1);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.run_fondo;
                ws.Cell(r, 3).Value = x.codigo_serie;
                ws.Cell(r, 4).Value = x.sexo;
                ws.Cell(r, 5).Value = x.numero_participes;
                ws.Cell(r, 6).Value = x.proporcion_patrimonio_serie;
                ws.Cell(r, 7).Value = x.valor_cuota;
                ws.Cell(r, 8).Value = x.patrimonio_serie;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void BuildSheetF06R2(XLWorkbook wb, List<Fondos06R2Model> rows)
        {
            var ws = wb.Worksheets.Add("Registro_02_Ubicacion");
            WriteHeaders(ws, ReportesExportConstants.ColumnasF06R2);
            int r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.tipo_registro;
                ws.Cell(r, 2).Value = x.run_fondo;
                ws.Cell(r, 3).Value = x.codigo_serie;
                ws.Cell(r, 4).Value = x.comuna;
                ws.Cell(r, 5).Value = x.region;
                ws.Cell(r, 6).Value = x.numero_participes;
                ws.Cell(r, 7).Value = x.proporcion_patrimonio_serie;
                ws.Cell(r, 8).Value = x.valor_cuota;
                ws.Cell(r, 9).Value = x.patrimonio_serie;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void WriteHeaders(IXLWorksheet ws, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(ReportesExportConstants.ColorEncabezadoHex);
                cell.Style.Font.FontColor = XLColor.White;
            }
        }
    }
}
