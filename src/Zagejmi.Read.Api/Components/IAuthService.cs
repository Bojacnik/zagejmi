using System.Threading.Tasks;

namespace Zagejmi.Read.Api.Components;

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
}