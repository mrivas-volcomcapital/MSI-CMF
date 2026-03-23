using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using MSI_CMF.Models;

namespace MSI_CMF.Datos
{
    public class UsuarioDatos
    {
        /// <summary>
        /// Valida credenciales contra la tabla dbo.Usuario.
        /// Retorna el UsuarioModel si es válido, null si no.
        /// </summary>
        public async Task<UsuarioModel?> ValidarCredencialesAsync(
            string connectionString,
            string usuario,
            string password,
            CancellationToken cancellationToken = default)
        {
            var hash = HashPassword(password);
            UsuarioModel? result = null;

            using var cn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(
                "SELECT id_usuario, usuario, nombre, email, rol, activo " +
                "FROM dbo.Usuario " +
                "WHERE usuario = @Usuario AND password_hash = @PasswordHash AND activo = 1", cn);

            cmd.Parameters.Add("@Usuario",      SqlDbType.VarChar, 100).Value = usuario.Trim();
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 256).Value = hash;

            await cn.OpenAsync(cancellationToken);
            using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await rd.ReadAsync(cancellationToken))
            {
                result = new UsuarioModel
                {
                    id_usuario = rd.GetInt32OrDefault("id_usuario"),
                    usuario    = rd.GetStringOrDefault("usuario"),
                    nombre     = rd.GetStringOrDefault("nombre"),
                    email      = rd.GetStringOrDefault("email"),
                    rol        = rd.GetStringOrDefault("rol"),
                    activo     = rd.GetBoolOrDefault("activo")
                };
            }

            return result;
        }

        /// <summary>
        /// Crea un nuevo usuario en dbo.Usuario.
        /// </summary>
        public async Task CrearUsuarioAsync(
            string connectionString,
            CrearUsuarioRequest request,
            CancellationToken cancellationToken = default)
        {
            var hash = HashPassword(request.password);

            using var cn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(
                "INSERT INTO dbo.Usuario (usuario, nombre, email, password_hash, rol, activo, fecha_creacion) " +
                "VALUES (@Usuario, @Nombre, @Email, @PasswordHash, @Rol, 1, GETDATE())", cn);

            cmd.Parameters.Add("@Usuario",      SqlDbType.VarChar, 100).Value = request.usuario.Trim();
            cmd.Parameters.Add("@Nombre",       SqlDbType.VarChar, 200).Value = request.nombre.Trim();
            cmd.Parameters.Add("@Email",        SqlDbType.VarChar, 200).Value = request.email.Trim();
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarChar, 256).Value = hash;
            cmd.Parameters.Add("@Rol",          SqlDbType.VarChar, 50).Value  = request.rol;

            await cn.OpenAsync(cancellationToken);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Obtiene todos los usuarios activos.
        /// </summary>
        public List<UsuarioModel> ObtenerUsuarios(string connectionString)
        {
            var lista = new List<UsuarioModel>();

            using var cn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(
                "SELECT id_usuario, usuario, nombre, email, rol, activo FROM dbo.Usuario ORDER BY nombre", cn);

            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new UsuarioModel
                {
                    id_usuario = rd.GetInt32OrDefault("id_usuario"),
                    usuario    = rd.GetStringOrDefault("usuario"),
                    nombre     = rd.GetStringOrDefault("nombre"),
                    email      = rd.GetStringOrDefault("email"),
                    rol        = rd.GetStringOrDefault("rol"),
                    activo     = rd.GetBoolOrDefault("activo")
                });
            }

            return lista;
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
