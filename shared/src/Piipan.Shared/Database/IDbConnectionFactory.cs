using System.Data;
using System.Threading.Tasks;

namespace Piipan.Shared.Database
{
    public interface IDbConnectionFactory<T>
    {
        Task<IDbConnection> Build(string database = null);
        Task<IDbConnection> BuildConnectionWithNodaTime(string database = null);
    }
}
