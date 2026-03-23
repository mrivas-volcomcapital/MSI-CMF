namespace MSI_CMF.Models
{
    // ── FONDOS 03 ────────────────────────────────────────────────────────────────

    /// <summary>Registro 01 de FONDOS03: Remuneraciones por fondo/serie.</summary>
    public class Fondos03R1Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string fecha_informacion { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_serie { get; set; } = string.Empty;
        public decimal monto_remuneracion_fija { get; set; }
        public decimal monto_remuneracion_variable { get; set; }
        public decimal monto_costo_inversion_otros_vehiculos { get; set; }
        public string? horizonte_tiempo { get; set; }
        public decimal monto_remuneracion_devuelto { get; set; }
    }

    /// <summary>Registro 02 de FONDOS03: Gastos operacionales por fondo.</summary>
    public class Fondos03R2Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string fecha_informacion { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_RDFondo { get; set; } = string.Empty;
        public decimal monto_gastos_financieros_afectos { get; set; }
        public decimal monto_gastos_financieros_no_afectos { get; set; }
        public decimal monto_gastos_operacionales_afectos { get; set; }
        public decimal monto_gastos_operacionales_no_afectos { get; set; }
        public decimal monto_otros_gastos_afectos { get; set; }
        public decimal monto_otros_gastos_no_afectos { get; set; }
    }

    /// <summary>Registro 03 de FONDOS03: Tasa anual de costos.</summary>
    public class Fondos03R3Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string fecha_informacion { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_RDFondo { get; set; } = string.Empty;
        public decimal tasa_anual_costos { get; set; }
        public decimal costo_total_periodo { get; set; }
        public decimal promedio_movil { get; set; }
        public int dias_con_datos { get; set; }
    }

    public class Fondos03ViewModel
    {
        public List<Fondos03R1Model> Registro1 { get; set; } = new();
        public List<Fondos03R2Model> Registro2 { get; set; } = new();
        public List<Fondos03R3Model> Registro3 { get; set; } = new();
    }

    // ── FONDOS 05 ────────────────────────────────────────────────────────────────

    /// <summary>Registro 01 de FONDOS05: Activo y pasivo por fondo.</summary>
    public class Fondos05R1Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string fecha_informacion { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public decimal total_activos { get; set; }
        public decimal total_pasivos { get; set; }
    }

    /// <summary>Registro 02 de FONDOS05: Datos por serie.</summary>
    public class Fondos05R2Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string fecha_informacion { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_serie { get; set; } = string.Empty;
        public decimal patrimonio { get; set; }
        public decimal cuotas_circulacion { get; set; }
        public decimal valor_cuota { get; set; }
        public decimal aportes { get; set; }
        public decimal monto_aportes { get; set; }
        public decimal rescates { get; set; }
        public decimal monto_rescates { get; set; }
        public string? emision_cuotas_vigente { get; set; }
        public string? tipo_vigencia { get; set; }
        public string? fondo_liquidacion { get; set; }
    }

    /// <summary>Registro 03 de FONDOS05: Partícipes por tipo.</summary>
    public class Fondos05R3Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_serie { get; set; } = string.Empty;
        public string tipo_participe { get; set; } = string.Empty;
        public long numero_participes { get; set; }
        public decimal proporcion_patrimonio { get; set; }
    }

    public class Fondos05ViewModel
    {
        public List<Fondos05R1Model> Registro1 { get; set; } = new();
        public List<Fondos05R2Model> Registro2 { get; set; } = new();
        public List<Fondos05R3Model> Registro3 { get; set; } = new();
    }

    // ── FONDOS 06 ────────────────────────────────────────────────────────────────

    /// <summary>Registro 01 de FONDOS06: Partícipes por sexo.</summary>
    public class Fondos06R1Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_serie { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public int numero_participes { get; set; }
        public decimal proporcion_patrimonio_serie { get; set; }
        public decimal valor_cuota { get; set; }
        public decimal patrimonio_serie { get; set; }
    }

    /// <summary>Registro 02 de FONDOS06: Partícipes por ubicación (comuna/región).</summary>
    public class Fondos06R2Model
    {
        public string tipo_registro { get; set; } = string.Empty;
        public string run_fondo { get; set; } = string.Empty;
        public string codigo_serie { get; set; } = string.Empty;
        public int comuna { get; set; }
        public int region { get; set; }
        public int numero_participes { get; set; }
        public decimal proporcion_patrimonio_serie { get; set; }
        public decimal valor_cuota { get; set; }
        public decimal patrimonio_serie { get; set; }
    }

    public class Fondos06ViewModel
    {
        public List<Fondos06R1Model> Registro1 { get; set; } = new();
        public List<Fondos06R2Model> Registro2 { get; set; } = new();
    }
}
