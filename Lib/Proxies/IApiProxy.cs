using System.Threading.Tasks;

namespace Lib.Proxies
{
    public interface IApiProxy
    {
        Task<string> GetRandomDataAsync();
    }
}