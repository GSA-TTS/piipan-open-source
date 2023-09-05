using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Piipan.Shared.Database
{
    public interface IDatabaseManager<DbType>
    {
        Task<T> PerformQuery<T>(Func<NpgsqlConnection, Task<T>> query, string database = null, CancellationToken cancellationToken = default);
    }
}
