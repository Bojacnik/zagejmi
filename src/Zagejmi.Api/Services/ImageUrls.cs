using System;

namespace Zagejmi.Api.Services;

/// <summary>
///     Builds browser URLs for stored images (served by <see cref="Controllers.ImagesController" />).
/// </summary>
public static class ImageUrls
{
    /// <summary>
    ///     Returns the URL of an image, or <see langword="null" /> when there is no image.
    /// </summary>
    /// <param name="imageId">The image ID.</param>
    /// <returns>A relative URL such as <c>/images/{id}</c>.</returns>
    public static string? For(Guid? imageId)
    {
        return imageId is { } id ? $"/images/{id}" : null;
    }
}
