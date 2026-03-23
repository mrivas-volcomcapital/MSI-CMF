namespace MSI_CMF.Datos
{
    public static class ErrorMessages
    {
        public const string CadenaConexionNoConfigurada = "Cadena de conexión 'CreditContext' no configurada.";
        public const string ParametrosInvalidos         = "Parámetros de entrada inválidos.";
        public const string FechaRequerida              = "Fecha de consulta requerida.";
        public const string PeriodoRequerido            = "Período de consulta requerido (formato AAAAMM).";
        public const string SinResultados               = "No se encontraron resultados para los parámetros indicados.";
        public const string ErrorBaseDatos              = "Error al consultar la base de datos.";
        public const string ArchivoInvalido             = "El archivo no es válido. Se requiere un archivo .xlsx.";
        public const string EstructuraExcelInvalida     = "La estructura del archivo Excel no es válida.";
    }
}
