using System.Data;

namespace Piipan.Metrics.Core.Extensions
{
    public static class DbCommandExtensions
    {
        public static void AddParameter(this IDbCommand dbCommand, DbType type, string name, object value)
        {
            var p = dbCommand.CreateParameter();
            p.DbType = type;
            p.ParameterName = name;
            p.Value = value;
            dbCommand.Parameters.Add(p);
        }
    }
}