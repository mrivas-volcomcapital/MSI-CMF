using Microsoft.Data.SqlClient;
using System.Data;
using MSI_CMF.Models;

namespace MSI_CMF.Datos
{
    public class ReportesFondosDatos
    {
        // ── FONDOS 03 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene el ViewModel completo de FONDOS03 invocando:
        ///   dbo.usp_genera_archivo_fondo3_registro1 @periodo_consulta DATE
        ///   dbo.usp_genera_archivo_fondo3_registro2 @periodo_consulta DATE
        ///   dbo.usp_genera_archivo_fondo3_registro3 @periodo_consulta DATE
        /// </summary>
        public async Task<Fondos03ViewModel> ObtenerFondos03Async(
            string connectionString,
            DateTime fecha,
            CancellationToken cancellationToken = default)
        {
            var vm = new Fondos03ViewModel();

            using var cn = new SqlConnection(connectionString);
            await cn.OpenAsync(cancellationToken);

            // Registro 1
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo3_registro1", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro1.Add(new Fondos03R1Model
                    {
                        tipo_registro    = rd.GetStringOrDefault("Tipo_Registro"),
                        fecha_informacion = rd.GetStringOrDefault("Fecha_Informacion"),
                        run_fondo        = rd.GetStringOrDefault("Run_Fondo"),
                        codigo_serie     = rd.GetStringOrDefault("CodigoSerie"),
                        monto_remuneracion_fija = rd.GetDecimalOrDefault("Monto_Remuneracion_Fija"),
                        monto_remuneracion_variable = 0,
                        monto_costo_inversion_otros_vehiculos = rd.GetDecimalOrDefault("Monto_Costo_Inversion_Otros_Vehiculos_calculo"),
                        horizonte_tiempo = rd.GetStringOrNull("Horizonte_Tiempo"),
                        monto_remuneracion_devuelto = 0
                    });
                }
            }

            // Registro 2
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo3_registro2", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro2.Add(new Fondos03R2Model
                    {
                        tipo_registro    = rd.GetStringOrDefault("Tipo_Registro"),
                        fecha_informacion = rd.GetStringOrDefault("Fecha_Informacion"),
                        run_fondo        = rd.GetStringOrDefault("n_RuFondo"),
                        codigo_RDFondo   = rd.GetStringOrDefault("codigo_RDFondo"),
                        monto_gastos_financieros_afectos     = 0,
                        monto_gastos_financieros_no_afectos  = 0,
                        monto_gastos_operacionales_afectos   = rd.GetDecimalOrDefault("Monto_gastos_operacionales_afectos"),
                        monto_gastos_operacionales_no_afectos = rd.GetDecimalOrDefault("Monto_gastos_operacionales_no_afectos"),
                        monto_otros_gastos_afectos    = 0,
                        monto_otros_gastos_no_afectos = 0
                    });
                }
            }

            // Registro 3
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo3_registro3", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro3.Add(new Fondos03R3Model
                    {
                        tipo_registro    = rd.GetStringOrDefault("Tipo_Registro"),
                        fecha_informacion = fecha.ToString("yyyyMMdd"),
                        run_fondo        = rd.GetStringOrDefault("n_RunFondo"),
                        codigo_RDFondo   = rd.GetStringOrDefault("codigo_RDFondo"),
                        tasa_anual_costos  = rd.GetDecimalOrDefault("Tasa_anual_costos"),
                        costo_total_periodo = rd.GetDecimalOrDefault("Costo_total_periodo"),
                        promedio_movil     = rd.GetDecimalOrDefault("PromedioMovil"),
                        dias_con_datos     = rd.GetInt32OrDefault("Dias_con_datos")
                    });
                }
            }

            return vm;
        }

        // ── FONDOS 05 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene el ViewModel completo de FONDOS05 invocando:
        ///   dbo.usp_genera_archivo_fondo5_registro1 @periodo_consulta DATE
        ///   dbo.usp_genera_archivo_fondo5_registro2 @periodo_consulta DATE
        ///   dbo.usp_genera_archivo_fondo5_registro3 @periodo_consulta DATE
        /// </summary>
        public async Task<Fondos05ViewModel> ObtenerFondos05Async(
            string connectionString,
            DateTime fecha,
            CancellationToken cancellationToken = default)
        {
            var vm = new Fondos05ViewModel();

            using var cn = new SqlConnection(connectionString);
            await cn.OpenAsync(cancellationToken);

            // Registro 1
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo5_registro1", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro1.Add(new Fondos05R1Model
                    {
                        tipo_registro    = rd.GetStringOrDefault("Tipo_Registro"),
                        fecha_informacion = rd.GetStringOrDefault("Fecha_Informacion"),
                        run_fondo        = rd.GetStringOrDefault("Run_Fondo"),
                        total_activos    = rd.GetDecimalOrDefault("total_activos"),
                        total_pasivos    = rd.GetDecimalOrDefault("total_pasivos")
                    });
                }
            }

            // Registro 2
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo5_registro2", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro2.Add(new Fondos05R2Model
                    {
                        tipo_registro    = rd.GetStringOrDefault("Tipo_Registro"),
                        fecha_informacion = rd.GetStringOrDefault("Fecha_Informacion"),
                        run_fondo        = rd.GetStringOrDefault("Run_Fondo"),
                        codigo_serie     = rd.GetStringOrDefault("CodigoRdSerie"),
                        patrimonio       = rd.GetDecimalOrDefault("Patrimonio"),
                        cuotas_circulacion = rd.GetDecimalOrDefault("Cuotas_Circulacion"),
                        valor_cuota      = rd.GetDecimalOrDefault("valor_cuota"),
                        aportes          = rd.GetDecimalOrDefault("aportes"),
                        monto_aportes    = rd.GetDecimalOrDefault("monto_aportes"),
                        rescates         = rd.GetDecimalOrDefault("rescates"),
                        monto_rescates   = rd.GetDecimalOrDefault("monto_rescates"),
                        emision_cuotas_vigente = rd.GetStringOrNull("Emision_cuotas_vigentes"),
                        tipo_vigencia    = rd.GetStringOrNull("tipo_vigencia"),
                        fondo_liquidacion = rd.GetStringOrNull("fondo_liquidacion")
                    });
                }
            }

            // Registro 3
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo5_registro3", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro3.Add(new Fondos05R3Model
                    {
                        tipo_registro   = rd.GetStringOrDefault("tipo_registro"),
                        fecha           = rd.GetStringOrDefault("fecha"),
                        run_fondo       = rd.GetStringOrDefault("run_fondo"),
                        codigo_serie    = rd.GetStringOrDefault("codigo_serie"),
                        tipo_participe  = rd.GetStringOrDefault("tipo_participe"),
                        numero_participes = rd.GetInt32OrDefault("numero_participes"),
                        proporcion_patrimonio = rd.GetDecimalOrDefault("proporcion_patrimonio")
                    });
                }
            }

            return vm;
        }

        // ── FONDOS 06 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene el ViewModel de FONDOS06 invocando:
        ///   dbo.usp_genera_archivo_fondo6_registro1 @periodo_consulta DATE
        ///   dbo.usp_genera_archivo_fondo6_registro2 @periodo_consulta DATE
        /// </summary>
        public async Task<Fondos06ViewModel> ObtenerFondos06Async(
            string connectionString,
            DateTime fecha,
            CancellationToken cancellationToken = default)
        {
            var vm = new Fondos06ViewModel();

            using var cn = new SqlConnection(connectionString);
            await cn.OpenAsync(cancellationToken);

            // Registro 1: por sexo
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo6_registro1", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro1.Add(new Fondos06R1Model
                    {
                        tipo_registro   = rd.GetStringOrDefault("Tipo_Registro"),
                        run_fondo       = rd.GetStringOrDefault("Run_Fondo"),
                        codigo_serie    = rd.GetStringOrDefault("codigo_serie"),
                        sexo            = rd.GetStringOrDefault("sexo"),
                        numero_participes = rd.GetInt32OrDefault("numero_participes"),
                        proporcion_patrimonio_serie = rd.GetDecimalOrDefault("proporcion_patrimonio_serie"),
                        valor_cuota     = rd.GetDecimalOrDefault("valor_cuota"),
                        patrimonio_serie = rd.GetDecimalOrDefault("patrimonio_serie")
                    });
                }
            }

            // Registro 2: por ubicación
            using (var cmd = new SqlCommand("dbo.usp_genera_archivo_fondo6_registro2", cn)
                   { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.Add("@periodo_consulta", SqlDbType.Date).Value = fecha.Date;
                using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken))
                {
                    vm.Registro2.Add(new Fondos06R2Model
                    {
                        tipo_registro   = rd.GetStringOrDefault("Tipo_Registro"),
                        run_fondo       = rd.GetStringOrDefault("Run_Fondo"),
                        codigo_serie    = rd.GetStringOrDefault("codigo_serie"),
                        comuna          = rd.GetInt32OrDefault("comuna"),
                        region          = rd.GetInt32OrDefault("region"),
                        numero_participes = rd.GetInt32OrDefault("numero_participes"),
                        proporcion_patrimonio_serie = rd.GetDecimalOrDefault("proporcion_patrimonio_serie"),
                        valor_cuota     = rd.GetDecimalOrDefault("valor_cuota"),
                        patrimonio_serie = rd.GetDecimalOrDefault("patrimonio_serie")
                    });
                }
            }

            return vm;
        }
    }
}
