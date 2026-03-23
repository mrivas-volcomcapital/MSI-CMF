using Microsoft.Data.SqlClient;

namespace MSI_CMF.Datos
{
    public static class SqlDataReaderExtensions
    {
        public static string GetStringOrDefault(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return rd.IsDBNull(ord) ? string.Empty : rd.GetString(ord);
        }

        public static string? GetStringOrNull(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return rd.IsDBNull(ord) ? null : rd.GetString(ord);
        }

        public static int GetInt32OrDefault(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return rd.IsDBNull(ord) ? 0 : rd.GetInt32(ord);
        }

        public static decimal GetDecimalOrDefault(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return rd.IsDBNull(ord) ? 0m : rd.GetDecimal(ord);
        }

        public static decimal? GetDecimalOrNull(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return rd.IsDBNull(ord) ? null : rd.GetDecimal(ord);
        }

        public static DateTime? GetDateTimeOrNull(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return rd.IsDBNull(ord) ? null : rd.GetDateTime(ord);
        }

        public static bool GetBoolOrDefault(this SqlDataReader rd, string column)
        {
            int ord = rd.GetOrdinal(column);
            return !rd.IsDBNull(ord) && rd.GetBoolean(ord);
        }
    }
}
