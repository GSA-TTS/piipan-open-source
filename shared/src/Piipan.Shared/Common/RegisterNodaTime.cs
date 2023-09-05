using Dapper.NodaTime;
using Dapper;
using Npgsql;
using Piipan.Shared.Database;
using System;

namespace Piipan.Shared.Common
{
	public class NodaTimeSetup
    {
		/// <summary>
		/// Parse any stringified date to return last day of month
		/// </summary>
	    public static NpgsqlDataSource Setup(NpgsqlDataSourceBuilder builder)
        {
            SqlMapper.AddTypeHandler(new DateRangeListHandler());
            DapperNodaTimeSetup.Register();

            var ds = builder.Build();
            return ds;
        }
    }
}
