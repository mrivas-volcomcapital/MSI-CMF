using System.ComponentModel.DataAnnotations;

namespace MSI_CMF.Models
{
    public class CostoInversionModel
    {
        public string run_fondo { get; set; } = string.Empty;
        public string? dv_fondo { get; set; }
        public string nombre_fondo { get; set; } = string.Empty;
        public int periodo { get; set; }
        public int anio { get; set; }
        public string? costo_inversion_master_fund { get; set; }
        public string? costo_inversion_fondos_mutuo { get; set; }
        public string? horizonte_tiempo_codigo { get; set; }
    }

    public class GuardarCostoInversionRequest
    {
        [Required(ErrorMessage = "El RUN del fondo es obligatorio.")]
        public string run_fondo { get; set; } = string.Empty;

        public string? dv_fondo { get; set; }

        [Required(ErrorMessage = "El nombre del fondo es obligatorio.")]
        public string nombre_fondo { get; set; } = string.Empty;

        [Range(1, 12, ErrorMessage = "El período debe ser entre 1 y 12.")]
        public int periodo { get; set; }

        [Range(2000, 2100, ErrorMessage = "El año no es válido.")]
        public int anio { get; set; }

        public string? costo_inversion_master_fund { get; set; }
        public string? costo_inversion_fondos_mutuo { get; set; }
        public string? horizonte_tiempo_codigo { get; set; }
    }
}
