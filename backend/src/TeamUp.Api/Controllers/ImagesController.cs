// Date: 2026-11-25
// Purpose: Database-stored image upload and public serving. POST returns a stable /api/v1/images/{id} URL usable
// in any existing image field (organization logo/cover, event cover, project image); GET serves bytes publicly
// with long caching since content is immutable.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Services;

namespace TeamUp.Api.Controllers;

[Route("api/v1/images")]
public sealed class ImagesController(IImageService imageService) : BaseApiController
{
    private const long MaxMultipartRequestBytes = 3 * 1024 * 1024;

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("upload-policy")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxMultipartRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxMultipartRequestBytes)]
    [ProducesOk<StoredImageDto>]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length <= 0)
            throw new BadRequestException("A non-empty JPEG, PNG, or WebP image file is required.");

        await using var stream = file.OpenReadStream();
        var result = await imageService.SaveImageAsync(CurrentStudentId, stream, file.Length, ct);
        return OkResponse(result, "Image uploaded successfully.");
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    [ProducesNotFound]
    public async Task<IActionResult> GetImage([FromRoute] Guid id, CancellationToken ct)
    {
        var image = await imageService.GetImageAsync(id, ct);
        if (image is null)
            return NotFound();

        Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        return File(image.Content, image.ContentType);
    }
}
