using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System.Data;
using MSI_CMF.Models;

namespace MSI_CMF.Datos
{
    public class CostoInversionDatos
    {
        private static readonly string[] ColumnasExcelEsperadas =
            { "run_fondo", "dv_fondo", "nombre_fondo", "periodo", "anio",
              "costo_inversion_master_fund", "costo_inversion_fondos_mutuo", "horizonte_tiempo_codigo" };

        /// <summary>
        /// Obtiene todos los registros de dbo.CostoInversionOtrosVehiculos.
        /// Filtro opcional por año.
        /// </summary>
        public List<CostoInversionModel> ObtenerTodos(string connectionString, int? anio = null)
        {
            var lista = new List<CostoInversionModel>();

            using var cn = new SqlConnection(connectionString);
            var sql = "SELECT run_fondo, dv_fondo, nombre_fondo, periodo, anio, " +
                      "costo_inversion_master_fund, costo_inversion_fondos_mutuo, horizonte_tiempo_codigo " +
                      "FROM dbo.CostoInversionOtrosVehiculos " +
                      (anio.HasValue ? "WHERE anio = @Anio " : "") +
                      "ORDER BY anio DESC, periodo DESC, run_fondo";

            using var cmd = new SqlCommand(sql, cn);
            if (anio.HasValue)
                cmd.Parameters.Add("@Anio", SqlDbType.Int).Value = anio.Value;

            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(MapearFila(rd));
            }

            return lista;
        }

        /// <summary>
        /// Inserta o actualiza un registro (MERGE por run_fondo + periodo + anio).
        /// </summary>
        public async Task GuardarAsync(
            string connectionString,
            GuardarCostoInversionRequest request,
            CancellationToken cancellationToken = default)
        {
            using var cn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(
                "MERGE dbo.CostoInversionOtrosVehiculos AS tgt " +
                "USING (SELECT @RunFondo AS run_fondo, @Periodo AS periodo, @Anio AS anio) AS src " +
                "  ON tgt.run_fondo = src.run_fondo AND tgt.periodo = src.periodo AND tgt.anio = src.anio " +
                "WHEN MATCHED THEN UPDATE SET " +
                "  dv_fondo = @DvFondo, nombre_fondo = @NombreFondo, " +
                "  costo_inversion_master_fund = @CostoMaster, " +
                "  costo_inversion_fondos_mutuo = @CostoFondos, " +
                "  horizonte_tiempo_codigo = @Horizonte " +
                "WHEN NOT MATCHED THEN INSERT " +
                "  (run_fondo, dv_fondo, nombre_fondo, periodo, anio, " +
                "   costo_inversion_master_fund, costo_inversion_fondos_mutuo, horizonte_tiempo_codigo) " +
                "VALUES (@RunFondo, @DvFondo, @NombreFondo, @Periodo, @Anio, " +
                "        @CostoMaster, @CostoFondos, @Horizonte);", cn);

            AgregarParametros(cmd, request);

            await cn.OpenAsync(cancellationToken);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Elimina un registro por run_fondo + periodo + anio.
        /// </summary>
        public async Task EliminarAsync(
            string connectionString,
            string runFondo, int periodo, int anio,
            CancellationToken cancellationToken = default)
        {
            using var cn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(
                "DELETE FROM dbo.CostoInversionOtrosVehiculos " +
                "WHERE run_fondo = @RunFondo AND periodo = @Periodo AND anio = @Anio", cn);

            cmd.Parameters.Add("@RunFondo", SqlDbType.VarChar, 10).Value = runFondo.Trim();
            cmd.Parameters.Add("@Periodo",  SqlDbType.Int).Value         = periodo;
            cmd.Parameters.Add("@Anio",     SqlDbType.Int).Value         = anio;

            await cn.OpenAsync(cancellationToken);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Importa masivamente desde un stream de archivo .xlsx.
        /// Retorna lista de errores (vacía = éxito).
        /// </summary>
        public async Task<(int procesados, List<string> errores)> ImportarExcelAsync(
            string connectionString,
            Stream excelStream,
            CancellationToken cancellationToken = default)
        {
            var errores   = new List<string>();
            var registros = new List<GuardarCostoInversionRequest>();

            using var wb = new XLWorkbook(excelStream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws is null)
            {
                errores.Add("El archivo Excel no contiene hojas.");
                return (0, errores);
            }

            // Validar cabeceras (fila 1)
            var headers = ws.Row(1).CellsUsed()
                            .Select(c => c.Value.ToString().Trim().ToLower())
                            .ToList();

            var faltantes = ColumnasExcelEsperadas
                .Where(c => !headers.Contains(c))
                .ToList();

            if (faltantes.Any())
            {
                errores.Add($"Columnas faltantes en el Excel: {string.Join(", ", faltantes)}");
                return (0, errores);
            }

            // Parsear filas
            int totalFilas = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int i = 2; i <= totalFilas; i++)
            {
                var fila = ws.Row(i);
                var runFondo     = fila.Cell(headers.IndexOf("run_fondo") + 1).GetString().Trim();
                var dvFondo      = fila.Cell(headers.IndexOf("dv_fondo") + 1).GetString().Trim();
                var nombreFondo  = fila.Cell(headers.IndexOf("nombre_fondo") + 1).GetString().Trim();
                var periodoStr   = fila.Cell(headers.IndexOf("periodo") + 1).GetString().Trim();
                var anioStr      = fila.Cell(headers.IndexOf("anio") + 1).GetString().Trim();
                var costoMaster  = fila.Cell(headers.IndexOf("costo_inversion_master_fund") + 1).GetString().Trim();
                var costoFondos  = fila.Cell(headers.IndexOf("costo_inversion_fondos_mutuo") + 1).GetString().Trim();
                var horizonte    = fila.Cell(headers.IndexOf("horizonte_tiempo_codigo") + 1).GetString().Trim();

                if (string.IsNullOrWhiteSpace(runFondo)) continue;

                if (!int.TryParse(periodoStr, out int periodo) || periodo < 1 || periodo > 12)
                    { errores.Add($"Fila {i}: período inválido '{periodoStr}'."); continue; }

                if (!int.TryParse(anioStr, out int anio) || anio < 2000)
                    { errores.Add($"Fila {i}: año inválido '{anioStr}'."); continue; }

                registros.Add(new GuardarCostoInversionRequest
                {
                    run_fondo    = runFondo,
                    dv_fondo     = string.IsNullOrWhiteSpace(dvFondo)     ? null : dvFondo,
                    nombre_fondo = nombreFondo,
                    periodo      = periodo,
                    anio         = anio,
                    costo_inversion_master_fund  = string.IsNullOrWhiteSpace(costoMaster) ? null : costoMaster,
                    costo_inversion_fondos_mutuo = string.IsNullOrWhiteSpace(costoFondos) ? null : costoFondos,
                    horizonte_tiempo_codigo      = string.IsNullOrWhiteSpace(horizonte)   ? null : horizonte
                });
            }

            if (errores.Any()) return (0, errores);

            // Persistir en transacción
            using var cn = new SqlConnection(connectionString);
            await cn.OpenAsync(cancellationToken);
            using var tx = cn.BeginTransaction();
            try
            {
                foreach (var reg in registros)
                {
                    using var cmd = new SqlCommand(
                        "MERGE dbo.CostoInversionOtrosVehiculos AS tgt " +
                        "USING (SELECT @RunFondo AS run_fondo, @Periodo AS periodo, @Anio AS anio) AS src " +
                        "  ON tgt.run_fondo = src.run_fondo AND tgt.periodo = src.periodo AND tgt.anio = src.anio " +
                        "WHEN MATCHED THEN UPDATE SET " +
                        "  dv_fondo = @DvFondo, nombre_fondo = @NombreFondo, " +
                        "  costo_inversion_master_fund = @CostoMaster, " +
                        "  costo_inversion_fondos_mutuo = @CostoFondos, " +
                        "  horizonte_tiempo_codigo = @Horizonte " +
                        "WHEN NOT MATCHED THEN INSERT " +
                        "  (run_fondo, dv_fondo, nombre_fondo, periodo, anio, " +
                        "   costo_inversion_master_fund, costo_inversion_fondos_mutuo, horizonte_tiempo_codigo) " +
                        "VALUES (@RunFondo, @DvFondo, @NombreFondo, @Periodo, @Anio, " +
                        "        @CostoMaster, @CostoFondos, @Horizonte);", cn, tx);

                    AgregarParametros(cmd, reg);
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            return (registros.Count, errores);
        }

        private static void AgregarParametros(SqlCommand cmd, GuardarCostoInversionRequest r)
        {
            cmd.Parameters.Add("@RunFondo",    SqlDbType.VarChar, 10).Value  = r.run_fondo.Trim();
            cmd.Parameters.Add("@DvFondo",     SqlDbType.Char,    1).Value   = (object?)r.dv_fondo ?? DBNull.Value;
            cmd.Parameters.Add("@NombreFondo", SqlDbType.VarChar, 200).Value = r.nombre_fondo.Trim();
            cmd.Parameters.Add("@Periodo",     SqlDbType.Int).Value           = r.periodo;
            cmd.Parameters.Add("@Anio",        SqlDbType.Int).Value           = r.anio;
            cmd.Parameters.Add("@CostoMaster", SqlDbType.VarChar, 20).Value  = (object?)r.costo_inversion_master_fund  ?? DBNull.Value;
            cmd.Parameters.Add("@CostoFondos", SqlDbType.VarChar, 20).Value  = (object?)r.costo_inversion_fondos_mutuo ?? DBNull.Value;
            cmd.Parameters.Add("@Horizonte",   SqlDbType.VarChar, 2).Value   = (object?)r.horizonte_tiempo_codigo      ?? DBNull.Value;
        }

        private static CostoInversionModel MapearFila(SqlDataReader rd) => new()
        {
            run_fondo    = rd.GetStringOrDefault("run_fondo"),
            dv_fondo     = rd.GetStringOrNull("dv_fondo"),
            nombre_fondo = rd.GetStringOrDefault("nombre_fondo"),
            periodo      = rd.GetInt32OrDefault("periodo"),
            anio         = rd.GetInt32OrDefault("anio"),
            costo_inversion_master_fund  = rd.GetStringOrNull("costo_inversion_master_fund"),
            costo_inversion_fondos_mutuo = rd.GetStringOrNull("costo_inversion_fondos_mutuo"),
            horizonte_tiempo_codigo      = rd.GetStringOrNull("horizonte_tiempo_codigo")
        };
    }
}
