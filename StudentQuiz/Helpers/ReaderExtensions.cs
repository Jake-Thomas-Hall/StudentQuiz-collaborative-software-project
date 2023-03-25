using System;
using System.Data.Common;

namespace StudentQuiz.Helpers
{
    public static class ReaderExtensions
    {
        public static DateTime? GetNullableDateTime(this DbDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ? null : reader.GetDateTime(col);
        }

        public static int? GetNullableInt(this DbDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ? null : reader.GetInt32(col);
        }

        public static string GetNullableString(this DbDataReader reader, string name)
        {
            var col = reader.GetOrdinal(name);
            return reader.IsDBNull(col) ? null : reader.GetString(col);
        }
    }
}
