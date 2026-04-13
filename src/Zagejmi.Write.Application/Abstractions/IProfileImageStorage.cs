using System.Threading;
using System.Threading.Tasks;

namespace Zagejmi.Write.Application.Abstractions;

public interface IProfileImageStorage
{
    Task<string> SaveProfileImageAsync(
        byte[] imageBytes,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<(byte[] Content, string ContentType)?> ReadProfileImageAsync(
        string blobKey,
        CancellationToken cancellationToken = default);
}