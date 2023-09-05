using System.Threading.Tasks;

namespace Piipan.Shared.Authentication
{
    public interface ITokenProvider<T>
    {
        Task<string> RetrieveAsync();
    }
}