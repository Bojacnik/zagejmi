using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Zagejmi.Application.Images.Abstractions;
using Zagejmi.Domain.Images;

namespace Zagejmi.Api.Controllers;

/// <summary>
///     Serves stored images (profile pictures, banners, service images). Images are public and immutable: a new upload
///     always gets a new ID, so browsers may cache them forever.
/// </summary>
[ApiController]
[Route("images")]
[AllowAnonymous]
public class ImagesController : ControllerBase
{
    private readonly IImageBlobRepository imageBlobRepository;
    private readonly IImageStorage imageStorage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImagesController" /> class.
    /// </summary>
    /// <param name="imageBlobRepository">Image metadata repository.</param>
    /// <param name="imageStorage">Image file storage.</param>
    public ImagesController(IImageBlobRepository imageBlobRepository, IImageStorage imageStorage)
    {
        this.imageBlobRepository = imageBlobRepository;
        this.imageStorage = imageStorage;
    }

    /// <summary>
    ///     Returns an image by ID.
    /// </summary>
    /// <param name="id">The image ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The image file, or 404.</returns>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 31536000, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        ImageBlob? blob = await this.imageBlobRepository.GetByIdAsync(id, cancellationToken);
        if (blob is null)
        {
            return this.NotFound();
        }

        Stream content = await this.imageStorage.OpenReadAsync(blob.BlobKey, cancellationToken);
        return this.File(content, blob.ContentType);
    }
}
