namespace MSI_CMF.Models
{
    /// <summary>
    /// Constantes de exportación compartidas para los reportes de fondos.
    /// </summary>
    public static class ReportesExportConstants
    {
        public const string MimeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        // Encabezado de hojas
        public const string ColorEncabezadoHex = "#1E5591";

        // ── FONDOS 03 ───────────────────────────────────────────────────────────
        public static readonly string[] ColumnasF03R1 =
        {
            "Tipo Registro", "Fecha Información", "Run Fondo", "Código Serie",
            "Monto Rem. Fija", "Monto Rem. Variable", "Monto Costo Inv. Otros Veh.",
            "Horizonte Tiempo", "Monto Rem. Devuelto"
        };

        public static readonly string[] ColumnasF03R2 =
        {
            "Tipo Registro", "Fecha Información", "Run Fondo", "Código RD Fondo",
            "Gastos Fin. Afectos", "Gastos Fin. No Afectos",
            "Gastos Op. Afectos", "Gastos Op. No Afectos",
            "Otros Gastos Afectos", "Otros Gastos No Afectos"
        };

        public static readonly string[] ColumnasF03R3 =
        {
            "Tipo Registro", "Fecha Información", "Run Fondo", "Código RD Fondo",
            "Tasa Anual Costos", "Costo Total Período", "Promedio Móvil", "Días con Datos"
        };

        // ── FONDOS 05 ───────────────────────────────────────────────────────────
        public static readonly string[] ColumnasF05R1 =
        {
            "Tipo Registro", "Fecha Información", "Run Fondo",
            "Total Activos", "Total Pasivos"
        };

        public static readonly string[] ColumnasF05R2 =
        {
            "Tipo Registro", "Fecha Información", "Run Fondo", "Código Serie",
            "Patrimonio", "Cuotas en Circulación", "Valor Cuota",
            "Aportes", "Monto Aportes", "Rescates", "Monto Rescates",
            "Emisión Cuotas Vigente", "Tipo Vigencia", "Fondo Liquidación"
        };

        public static readonly string[] ColumnasF05R3 =
        {
            "Tipo Registro", "Fecha", "Run Fondo", "Código Serie",
            "Tipo Partícipe", "N° Partícipes", "Proporción Patrimonio"
        };

        // ── FONDOS 06 ───────────────────────────────────────────────────────────
        public static readonly string[] ColumnasF06R1 =
        {
            "Tipo Registro", "Run Fondo", "Código Serie", "Sexo",
            "N° Partícipes", "Proporción Patrimonio Serie",
            "Valor Cuota", "Patrimonio Serie"
        };

        public static readonly string[] ColumnasF06R2 =
        {
            "Tipo Registro", "Run Fondo", "Código Serie",
            "Comuna", "Región", "N° Partícipes",
            "Proporción Patrimonio Serie", "Valor Cuota", "Patrimonio Serie"
        };
    }
}
