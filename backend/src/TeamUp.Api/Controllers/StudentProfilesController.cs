using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Students;

namespace TeamUp.Api.Controllers;

/// <summary>
/// Single Responsibility: Extended portfolio, biography, and external profiles.
/// </summary>
[Route("api/v1/students")]
[Authorize]
public sealed class StudentProfilesController(
    IStudentProfileService profileService,
    IProfilePhotoStorage photoStorage) : BaseApiController
{
    [HttpGet("{id:int}/profile")]
    [ProducesOk<StudentProfileDto>]
    public async Task<IActionResult> GetProfile([FromRoute] int id, CancellationToken ct)
    {
        var result = await profileService.GetExtendedProfileAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPut("me/profile")]
    [ProducesOk<StudentProfileDto>]
    public async Task<IActionResult> UpsertMyProfile([FromBody] UpsertProfileRequest request, CancellationToken ct)
    {
        var result = await profileService.UpsertExtendedProfileAsync(CurrentStudentId, request, ct);
        return OkResponse(result, "Profile updated successfully.");
    }

    [HttpPost("me/profile/photo")]
    [EnableRateLimiting("upload-policy")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 3 * 1024 * 1024)]
    [ProducesOk<StudentProfileDto>]
    public async Task<IActionResult> UploadMyProfilePhoto(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length <= 0)
            throw new BadRequestException("A non-empty JPEG, PNG, or WebP image file is required.");

        await using var stream = file.OpenReadStream();
        var photoUrl = await photoStorage.SaveProfilePhotoAsync(CurrentStudentId, stream, file.Length, ct);
        var result = await profileService.UpdateProfilePhotoAsync(CurrentStudentId, photoUrl, ct);
        return OkResponse(result, "Profile photo uploaded successfully.");
    }
}
