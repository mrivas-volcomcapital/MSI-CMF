# Copilot Instructions — MSI-CMF

Estas instrucciones definen los estándares de arquitectura, nomenclatura y patrones que deben seguirse en todos los desarrollos nuevos de este proyecto.

---

## 1. Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Base de datos | SQL Server (ADO.NET + Stored Procedures) |
| ORM / acceso a datos | ADO.NET directo (no Entity Framework para consultas) |
| Autenticación | Cookie Authentication (UI) + JWT Bearer (API) |
| Frontend | Bootstrap 5, DataTables 1.13.8, Select2, jQuery, Font Awesome 6 |
| Exportación | ClosedXML (Excel), iTextSharp (PDF) |
| Background Jobs | `BackgroundService` / `IHostedService` |
| Caché | `IMemoryCache` |

---

## 2. Estructura de carpetas

```
AppCreditos/
├── Controllers/        # Controladores MVC. Un archivo por controlador.
├── Datos/              # Capa de acceso a datos. Un archivo por entidad/dominio.
├── Models/             # Modelos de vista y DTOs. Un archivo por modelo.
├── Jobs/               # Workers y colas de background jobs.
├── Views/              # Vistas Razor, organizadas por nombre de controlador.
│   └── {Controlador}/  # Una subcarpeta por cada controlador.
└── wwwroot/            # Archivos estáticos (CSS, JS, imágenes).
```

### Reglas de estructura

- Cada nuevo módulo **debe** tener su carpeta en `Views/{NombreControlador}/`.
- Las clases de acceso a datos van en `Datos/`, nunca dentro de los controladores.
- Los modelos de dominio y DTOs van en `Models/`.
- Los workers de fondo van en `Jobs/`.
- No crear subcarpetas adicionales dentro de `Controllers/` ni de `Datos/`.

---

## 3. Nomenclatura

### C# — General

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, métodos públicos | `PascalCase` | `RecaudacionController`, `ObtenerMorosidad` |
| Variables locales, parámetros | `camelCase` | `connectionString`, `vehiculoInversion` |
| Campos privados readonly | `_camelCase` con prefijo `_` | `_configuration`, `_recaudacionDatos` |
| Constantes | `PascalCase` o `UPPER_SNAKE_CASE` | `CachePrefix`, `MaxRequestBodySize` |
| Propiedades de modelos | `snake_case` (para alinear con columnas de BD) | `numero_operacion_cmf`, `fecha_carga` |

### Controladores

- Nombre: `{Entidad}Controller` → `CreditController`, `RecaudacionController`.
- Namespace: `AppCreditos.Controllers`.

### Capa de datos (`Datos/`)

- Nombre: corresponde a la entidad de dominio → `Creditos`, `Recaudacion`, `ReporteMora`.
- Namespace: `AppCreditos.Datos`.

### Modelos (`Models/`)

- Modelos de vista: `{Entidad}Model` o `{Entidad}Models` → `RecaudacionModels`, `CuadroControlCreditosModel`.
- DTOs de request: `{Accion}{Entidad}Request` → `BuscarObservacionesRequest`.
- Namespace: `AppCreditos.Models`.

### Vistas

- Nombre del archivo Razor: `PascalCase` → `Index.cshtml`, `AddCredit.cshtml`.
- Deben ubicarse en `Views/{NombreControlador}/`.

---

## 4. Controladores MVC

### Patrón base obligatorio

```csharp
namespace AppCreditos.Controllers
{
    public class NuevoModuloController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly NuevoModuloDatos _datos = new NuevoModuloDatos();

        public NuevoModuloController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private string GetConnectionString()
        {
            var cs = _configuration.GetConnectionString("CreditContext");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Cadena de conexión 'CreditContext' no configurada.");
            return cs;
        }
    }
}
```

### Reglas de los controladores

- **Siempre** validar que la cadena de conexión no sea nula/vacía antes de usarla. Retornar `BadRequest(...)` si lo es.
- **No** poner lógica de negocio ni consultas SQL dentro del controlador. Toda interacción con la BD debe delegarse a la clase correspondiente en `Datos/`.
- La inyección de dependencias se hace **sólo por constructor**.
- Los campos de datos instanciados directamente (p. ej. `new Recaudacion()`) se declaran como `private readonly`.
- Usar `IConfiguration` para leer la cadena de conexión; nunca hardcodearla.
- **Un solo tipo público por archivo** — nunca incluir múltiples clases o enums en el mismo `.cs`.
- Los métodos de acción deben mantener **≤ 40 líneas**; si superan ese límite, extraer la lógica a un método privado o a `Datos/`.

### Métodos async en controladores

Cuando una acción invoca operaciones de I/O (BD, archivos), debe ser `async` y aceptar `CancellationToken`:

```csharp
[HttpPost]
public async Task<IActionResult> Procesar(
    [FromBody] SomeRequest request,
    CancellationToken cancellationToken = default)
{
    var cs = GetConnectionString();
    var resultado = await _datos.EjecutarAsync(cs, request, cancellationToken);
    return Ok(resultado);
}
```

### Endpoints AJAX para DataTables

Todos los módulos con tablas de datos deben exponer un endpoint `JsonResult` separado para la carga AJAX:

```csharp
/// <summary>
/// Endpoint AJAX para DataTables. Retorna datos en formato { data: [...] }
/// </summary>
[HttpGet]
public JsonResult GetNuevoModuloJson(string? filtro = null)
{
    try
    {
        var cs = GetConnectionString();
        var items = _datos.ObtenerItems(cs, filtro);

        var data = items.Select(i => new
        {
            campo_uno = i.campo_uno ?? "",
            fecha_ejemplo = i.fecha_ejemplo.ToString("dd-MM-yyyy"),
            monto_ejemplo = i.monto_ejemplo
        });

        return Json(new { data });
    }
    catch (Exception ex)
    {
        return Json(new { error = ex.Message });
    }
}
```

- La acción `Index` **no** debe pasar la lista de registros al modelo de la vista cuando se use AJAX. Retornar `View(new List<T>())`.
- Los filtros de dropdown/select de la vista se pasan siempre mediante `ViewBag`.

---

## 5. Capa de acceso a datos (`Datos/`)

### Patrón base obligatorio

```csharp
using System.Data;
using System.Data.SqlClient;
using AppCreditos.Models;

namespace AppCreditos.Datos
{
    public class NuevoModuloDatos
    {
        public List<NuevoModuloModel> ObtenerItems(string connectionString, string? filtro = null)
        {
            var lista = new List<NuevoModuloModel>();

            using var cn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("dbo.usp_NombreStoredProcedure", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Filtro", SqlDbType.VarChar, 100)
               .Value = string.IsNullOrWhiteSpace(filtro) ? (object)DBNull.Value : filtro.Trim();

            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new NuevoModuloModel
                {
                    campo_uno = rd.GetStringOrDefault("campo_uno"),
                    fecha_ejemplo = rd.GetDateTimeOrNull("fecha_ejemplo") ?? DateTime.MinValue
                });
            }

            return lista;
        }
    }
}
```

### Reglas de la capa de datos

- **Siempre** usar Stored Procedures para operaciones de lectura compleja, inserciones y actualizaciones. No embeber SQL en strings largos directamente en el código.
- Para SQL simple (SELECT de catálogos pequeños), se permite SQL inline siempre que sea parametrizado.
- **Nunca** concatenar valores de usuario en cadenas SQL. Usar siempre `SqlParameter`.
- Usar los métodos de extensión de `SqlDataReaderExtensions` (`GetStringOrDefault`, `GetStringOrNull`, `GetInt32OrDefault`, `GetDecimalOrDefault`, `GetDateTimeOrNull`, etc.) para una lectura segura del `SqlDataReader`.
- Los valores nulos de BD deben manejarse explícitamente: usar `(object)DBNull.Value` al asignar parámetros opcionales.
- Abrir y cerrar la conexión siempre dentro de un bloque `using`.
- **No materializar antes de filtrar/paginar**: nunca traer todos los registros a memoria solo para luego aplicar `.Where()` o `.Skip()/.Take()` en C#. El filtrado y la paginación deben realizarse en el SQL (SP o query parametrizado) antes de retornar los datos.
- Para exportaciones o consultas de volumen alto, leer en **lotes** usando `OFFSET … FETCH` en el SP para no saturar memoria ni la BD.
- Documentar los métodos públicos con `/// <summary>` indicando qué SP invocan y qué parámetros reciben.

---

## 6. Modelos

```csharp
using System.ComponentModel.DataAnnotations;

namespace AppCreditos.Models
{
    public class NuevoModuloModel
    {
        // snake_case para alinear con columnas de base de datos
        public string campo_texto { get; set; }
        public decimal monto_uf { get; set; }
        public DateTime fecha_proceso { get; set; }
        public string? campo_opcional { get; set; }
    }
}
```

### Reglas de modelos

- Las propiedades que mapean directamente a columnas de BD usan `snake_case`.
- Usar tipos `nullable` (`string?`, `decimal?`, `DateTime?`) para columnas que admiten `NULL` en BD.
- Aplicar `[Required]`, `[Display]` y `[Key]` cuando el modelo se use en formularios o como entidad de EF Core.
- No mezclar lógica de negocio dentro de los modelos.

---

## 7. Vistas Razor

### Estructura mínima de una vista con DataTables

```cshtml
@model List<AppCreditos.Models.NuevoModuloModel>
@{
    ViewData["Title"] = "Nombre Módulo";
}

@section Styles {
    {{-- Estilos específicos de la vista, si aplica --}}
}

<div class="container-fluid px-4 py-3">
    <h2 class="mb-3">Nombre Módulo</h2>

    <!-- Filtros -->
    <form id="formFiltros" class="row g-2 mb-3">
        ...
    </form>

    <!-- Tabla -->
    <div class="table-responsive">
        <table id="tablaNuevoModulo" class="table table-striped table-bordered table-hover w-100">
            <thead class="table-dark">
                <tr>
                    <th>Columna 1</th>
                    ...
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>
</div>

@section Scripts {
    <script>
        // Inicialización DataTables con AJAX
        const tabla = $('#tablaNuevoModulo').DataTable({
            ajax: {
                url: '/NuevoModulo/GetNuevoModuloJson',
                dataSrc: 'data'
            },
            columns: [
                { data: 'campo_texto' },
                ...
            ],
            language: { url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-CL.json' },
            pageLength: 50,
            order: [[0, 'asc']]
        });
    </script>
}
```

### Reglas de vistas

- **Siempre** usar el layout `_Layout.cshtml` a través de `_ViewStart.cshtml`. No sobreescribir `Layout` salvo excepciones justificadas.
- Los datos de tabla se cargan siempre por AJAX; la vista inicial retorna lista vacía.
- El idioma de DataTables debe ser `es-CL` (español Chile).
- Las fechas se formatean como `dd-MM-yyyy`.
- Los montos se formatean con separador de miles (`.`) y decimales (`,`) conforme al formato chileno.
- Usar `ViewBag` para poblar dropdowns de filtros desde el controlador.
- No escribir lógica de negocio en las vistas. El código C# en Razor se limita a renderizado condicional simple.
- Colocar el JavaScript de inicialización en `@section Scripts { }`.
- Los estilos propios de la vista van en `@section Styles { }`.

---

## 8. Background Jobs

Para procesos largos que no pueden completarse en el ciclo de un request HTTP, usar el patrón establecido:

```
Jobs/
├── {Modulo}JobModels.cs   # Modelos de entrada/salida del job
├── {Modulo}JobQueue.cs    # Interfaz + implementación de la cola (Channel<T>)
└── {Modulo}Worker.cs      # BackgroundService que consume la cola
```

### Reglas de jobs

- Implementar `BackgroundService` y sobreescribir `ExecuteAsync`.
- Registrar la cola como `Singleton` y el worker como `HostedService` en `Program.cs`.
- Loguear inicio, error y fin de cada job usando `ILogger<T>`.
- Capturar excepciones dentro del worker y marcar el job como fallido; nunca dejar caer el worker completo.

---

## 9. Autenticación y autorización

- La autenticación por defecto es **Cookie** (flujo web MVC).
- Los endpoints que consumen APIs externas o que se exponen como API REST usan **JWT Bearer**.
- Proteger todas las acciones sensibles con `[Authorize]`.
- La sesión de cookie expira en **8 horas** con `SlidingExpiration = true`. No cambiar sin justificación.
- Las rutas de login, logout y acceso denegado se definen en `Program.cs`; no duplicarlas.

---

## 10. Configuración (`appsettings.json`)

- La cadena de conexión principal es `"CreditContext"`. Siempre acceder con `_configuration.GetConnectionString("CreditContext")`.
- Los secretos (JWT key, passwords) **nunca** se hardcodean. Usar `appsettings.json` o variables de entorno, y validar su presencia al arrancar la aplicación.
- La configuración específica de ambiente va en `appsettings.Development.json`; la de producción en variables de entorno o secretos administrados.

---

## 11. Manejo de errores

- En controladores, capturar `SqlException` y excepciones generales de forma separada en operaciones críticas.
- Los endpoints JSON deben retornar `{ error: "mensaje" }` ante fallos, nunca lanzar excepciones no controladas al cliente.
- Las vistas usan `ErrorViewModel` y la vista `Views/Shared/Error.cshtml` para errores HTTP.
- Usar `ILogger<T>` para registrar errores en workers y servicios; en controladores sólo para errores críticos.

---

## 12. Exportación de datos

- **Excel**: usar `ClosedXML` (`XLWorkbook`). Retornar `File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "nombre.xlsx")`.
- **PDF**: usar `iTextSharp`. Retornar `File(bytes, "application/pdf", "nombre.pdf")`.
- Los métodos de exportación van en el mismo controlador del módulo, con sufijo en el nombre de la acción (`ExportarExcel`, `ExportarPdf`).

### Patrón de exportación por streaming (obligatorio para conjuntos grandes)

Para evitar errores de memoria (`OutOfMemoryException`) en archivos grandes, **no usar `MemoryStream` ni `byte[]`**. Usar un archivo temporal en disco:

```csharp
public IActionResult ExportarExcel(string? filtro = null)
{
    var cs = GetConnectionString();

    // Leer en lotes desde el SP para no saturar memoria
    var items = _datos.ObtenerItemsParaExport(cs, filtro);

    var tempFile = Path.GetTempFileName();
    using (var wb = new XLWorkbook())
    {
        var ws = wb.Worksheets.Add("Datos");
        // ... rellenar hoja
        wb.SaveAs(tempFile);
    }

    // FileOptions.DeleteOnClose → el SO borra el archivo al cerrar el stream
    var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read,
        FileShare.None, 4096, FileOptions.DeleteOnClose);

    return File(stream,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"reporte_{DateTime.Now:yyyyMMdd}.xlsx");
}
```

### Constantes de exportación

Los colores, MIME types y encabezados de columnas **no se hardcodean** en el método. Centralizar en una clase estática por módulo:

```csharp
// Models/NuevoModuloExportConstants.cs
public static class NuevoModuloExportConstants
{
    // MIME types
    public const string MimeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const string MimePdf   = "application/pdf";

    // Colores iTextSharp (PDF)
    public static readonly BaseColor ColorEncabezado = new BaseColor(30, 85, 145);  // #1E5591
    public static readonly BaseColor ColorTextoBlanco = BaseColor.WHITE;

    // Encabezados columnas Excel
    public static readonly string[] ColumnasExcel =
        { "Campo 1", "Campo 2", "Fecha", "Monto UF" };
}
```

---

## 13. Buenas prácticas generales

### DateTime

- Usar `DateTime.Now` por defecto para toda fecha/hora generada en código (el sistema opera en zona horaria local).
- Evitar `DateTime.UtcNow` o `DateTimeOffset.UtcNow` salvo que haya una especificación explícita que lo requiera.

### Mensajes de error en respuestas JSON

Evitar strings literales repetidos en las respuestas. Centralizar en constantes por módulo o en una clase global:

```csharp
// Datos/ErrorMessages.cs  (clase compartida del proyecto)
public static class ErrorMessages
{
    public const string CadenaConexionNoConfigurada = "Cadena de conexión 'CreditContext' no configurada.";
    public const string ParametrosInvalidos         = "Parámetros de entrada inválidos.";
}

// Uso en controlador:
return Json(new { error = ErrorMessages.CadenaConexionNoConfigurada });
```

### Seguridad en logging

- **Nunca** registrar en logs credenciales, tokens JWT ni cadenas de conexión completas.
- Al loguear errores de BD, registrar solo el mensaje de excepción (no el stack completo en entornos de producción).
- El logging detallado va exclusivamente en `Jobs/` (workers) y en puntos de diagnóstico muy específicos de `Datos/`; en controllers solo para errores críticos no controlados.

### Rendimiento

- No traer todos los registros de BD a memoria para luego filtrar con LINQ en C#. El filtrado va en el SP.
- Para obtener totales de paginación sin doble consulta, usar `COUNT(*) OVER()` en el SP.
- No abrir instancias de `SqlConnection` fuera de un bloque `using`; nunca reutilizar conexiones entre requests.

### Clarificación antes de generar código

Si falta un dato esencial (nombre real de SP, columna de BD, parámetro exacto del SP), **no inventarlo**. Responder:

```
🚨 CLARIFICATION NEEDED:
Artifacto faltante: <describir>
No se puede continuar sin: <dato requerido>
Por favor confirmar o proveer la especificación.
```

---

## 14. Checklist para nuevos módulos

Al crear un nuevo módulo, verificar que se cumplan todos estos puntos:

- [ ] Clase de acceso a datos creada en `Datos/` con su método usando SP o SQL parametrizado.
- [ ] Modelo(s) creado(s) en `Models/` con propiedades `snake_case`.
- [ ] Controlador creado en `Controllers/` con inyección de `IConfiguration` por constructor.
- [ ] La cadena de conexión se valida antes de usarla.
- [ ] Endpoint `GetXxxJson` creado para la carga AJAX de DataTables (si hay tabla).
- [ ] Vista `Index.cshtml` creada en `Views/{NombreControlador}/` con DataTables inicializado en español Chile.
- [ ] La vista inicial retorna `View(new List<T>())` (sin datos pre-cargados).
- [ ] Dropdowns y filtros se pasan via `ViewBag` desde el controlador.
- [ ] Fechas formateadas como `dd-MM-yyyy`.
- [ ] Montos formateados en formato chileno (`.` miles, `,` decimales).
- [ ] No hay SQL sin parametrizar ni cadenas de conexión hardcodeadas.
- [ ] Los métodos de exportación de Excel/PDF siguen el patrón `ClosedXML`/`iTextSharp`.
- [ ] Si requiere proceso largo: worker implementado en `Jobs/` y registrado en `Program.cs`.
- [ ] No hay strings literales duplicados en respuestas JSON de error; se usan constantes.
- [ ] Exportaciones masivas (> 5.000 filas) usan FileStream temporal en lugar de MemoryStream.
- [ ] Colores y MIME types de exportación centralizados en clase `*ExportConstants`.
- [ ] Métodos de acción `async` cuando invocan operaciones de I/O (BD, archivos).
- [ ] Ningún método supera las 40 líneas; la lógica extra se extrae a método privado o a `Datos/`.
- [ ] Se usa `DateTime.Now` (no `DateTime.UtcNow`) salvo especificación explícita.
- [ ] Un solo tipo público por archivo `.cs`.
