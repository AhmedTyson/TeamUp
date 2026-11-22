using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Gamification;
using TeamUp.Application.Students;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Services;

namespace TeamUp.IntegrationTests;

public class AuthAndProfileTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static async Task<T> GetDataAsync<T>(HttpResponseMessage res)
    {
        var envelope = await res.Content.ReadFromJsonAsync<BaseResponse<T>>();
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task Register_Returns201_AndSetsJwtToken()
    {
        var email = $"register_{Guid.NewGuid():N}@test.com";
        var req = new RegisterRequest("John", "Doe", email, "P@ssword123!", 2, factory.SeededFacultyId);

        var res = await _client.PostAsJsonAsync("/api/v1/auth/register", req);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var auth = await GetDataAsync<AuthResponse>(res);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.Equal(email, auth.Student.Email);
        Assert.Equal("John", auth.Student.FirstName);
        Assert.Equal("Doe", auth.Student.LastName);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var req = new RegisterRequest("Alice", "Smith", email, "P@ssword123!", 1, factory.SeededFacultyId);

        var first = await _client.PostAsJsonAsync("/api/v1/auth/register", req);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/v1/auth/register", req);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidPayload_Returns400WithStructuredValidationErrors()
    {
        var req = new RegisterRequest("", "", "not-an-email", "weak", 0, -1);

        var res = await _client.PostAsJsonAsync("/api/v1/auth/register", req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var err = await res.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(err);
        Assert.False(err.Success);
        Assert.Equal(400, err.StatusCode);
        Assert.NotNull(err.Errors);
        Assert.NotEmpty(err.Errors);
        Assert.Contains(err.Errors, e => e.Field == "email");
        Assert.Contains(err.Errors, e => e.Field == "password");
    }

    [Fact]
    public async Task Register_InvalidFaculty_Returns404NotFound()
    {
        var email = $"bad_faculty_{Guid.NewGuid():N}@test.com";
        var req = new RegisterRequest("Bob", "Smith", email, "P@ssword123!", 1, 999999);

        var res = await _client.PostAsJsonAsync("/api/v1/auth/register", req);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200AndTokens()
    {
        var email = $"login_ok_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Omar", "Tarek", email, "Secret123!", 3, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        var login = new LoginRequest(email, "Secret123!");
        var res = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var auth = await GetDataAsync<AuthResponse>(res);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401Unauthorized()
    {
        var email = $"bad_pass_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Sara", "Ali", email, "Correct123!", 1, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        var login = new LoginRequest(email, "WrongPassword!");
        var res = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Valid_ReturnsRotatedToken()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Laila", "Hassan", email, "Secret123!", 2, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var initialAuth = await GetDataAsync<AuthResponse>(regRes);

        var refreshReq = new RefreshTokenRequest(initialAuth.RefreshToken);
        var res = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshReq);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var newAuth = await GetDataAsync<AuthResponse>(res);
        Assert.NotEqual(initialAuth.RefreshToken, newAuth.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_Invalid_Returns401Unauthorized()
    {
        var res = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest("non_existent_refresh_token"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task GetMe_Unauthenticated_Returns401Unauthorized()
    {
        using var unauthClient = factory.CreateClient();
        var res = await unauthClient.GetAsync("/api/v1/students/me");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Profile_FullLifecycle_WorksEndToEnd()
    {
        // 1. Register student
        var email = $"profile_lifecycle_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Kareem", "Adel", email, "Password123!", 2, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // 2. Get /students/me
        var meRes = await client.GetAsync("/api/v1/students/me");
        Assert.Equal(HttpStatusCode.OK, meRes.StatusCode);
        var me = await GetDataAsync<StudentDetailDto>(meRes);
        Assert.Equal("Kareem", me.FirstName);
        Assert.Equal("Adel", me.LastName);
        Assert.Equal(2, me.AcademicYear);
        Assert.Null(me.Profile);

        // 3. Update /students/me
        var updateRes = await client.PatchAsJsonAsync("/api/v1/students/me", new UpdateStudentRequest("Karim", "Adel", 3));
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updatedMe = await GetDataAsync<StudentDetailDto>(updateRes);
        Assert.Equal("Karim", updatedMe.FirstName);
        Assert.Equal(3, updatedMe.AcademicYear);

        // 4. Upsert extended profile
        var profileReq = new UpsertProfileRequest("Passionate about AI", "https://img.local/avatar.png", "https://github.com/karim", "https://linkedin.com/in/karim");
        var upsertRes = await client.PutAsJsonAsync("/api/v1/students/me/profile", profileReq);
        Assert.Equal(HttpStatusCode.OK, upsertRes.StatusCode);
        var profileDto = await GetDataAsync<StudentProfileDto>(upsertRes);
        Assert.Equal("Passionate about AI", profileDto.Bio);
        Assert.Equal("https://github.com/karim", profileDto.GitHubUrl);

        // 5. Fetch public extended profile by ID
        var publicProfileRes = await client.GetAsync($"/api/v1/students/{me.StudentId}/profile");
        Assert.Equal(HttpStatusCode.OK, publicProfileRes.StatusCode);
        var publicProfile = await GetDataAsync<StudentProfileDto>(publicProfileRes);
        Assert.Equal("Passionate about AI", publicProfile.Bio);

        // 6. Manage interests
        var addRes = await client.PostAsJsonAsync("/api/v1/students/me/interests", new AddInterestRequest("Artificial Intelligence"));
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        var listInterestsRes = await client.GetAsync($"/api/v1/students/{me.StudentId}/interests");
        Assert.Equal(HttpStatusCode.OK, listInterestsRes.StatusCode);
        var interests = await GetDataAsync<List<string>>(listInterestsRes);
        Assert.Contains("Artificial Intelligence", interests);

        // 7. Delete interest (soft delete)
        var deleteInterestRes = await client.DeleteAsync("/api/v1/students/me/interests/Artificial Intelligence");
        Assert.Equal(HttpStatusCode.NoContent, deleteInterestRes.StatusCode);

        var listAfterDelete = await GetDataAsync<List<string>>(await client.GetAsync($"/api/v1/students/{me.StudentId}/interests"));
        Assert.DoesNotContain("Artificial Intelligence", listAfterDelete);

        // 8. Re-adding the same interest tag restores the soft-deleted record without throwing unique PK error
        var reAddRes = await client.PostAsJsonAsync("/api/v1/students/me/interests", new AddInterestRequest("Artificial Intelligence"));
        Assert.Equal(HttpStatusCode.OK, reAddRes.StatusCode);

        var listAfterReAdd = await GetDataAsync<List<string>>(await client.GetAsync($"/api/v1/students/{me.StudentId}/interests"));
        Assert.Contains("Artificial Intelligence", listAfterReAdd);

        // 9. Logout
        var logoutRes = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutRes.StatusCode);

        // 10. Refresh with logged-out token fails
        var refreshAfterLogout = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest(auth.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    [Fact]
    public async Task StudentsDirectory_ReturnsFilteredStudents_AndPublicProfile()
    {
        var email = $"dir_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Nader", "Fahmy", email, "Password123!", 4, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Directory Search
        var dirRes = await client.GetAsync($"/api/v1/students?search=Nader&academicYear=4");
        Assert.Equal(HttpStatusCode.OK, dirRes.StatusCode);
        var dir = await GetDataAsync<TeamUp.Application.Common.PagedResult<StudentCardDto>>(dirRes);
        Assert.NotNull(dir);
        Assert.NotEmpty(dir.Items);
        Assert.Contains(dir.Items, s => s.Email == email);

        // Public Profile Card
        var pubRes = await client.GetAsync($"/api/v1/students/{auth.Student.StudentId}");
        Assert.Equal(HttpStatusCode.OK, pubRes.StatusCode);
        var pub = await GetDataAsync<StudentPublicProfileDto>(pubRes);
        Assert.NotNull(pub);
        Assert.Equal("Nader", pub.FirstName);
        Assert.Equal(4, pub.AcademicYear);
    }

    [Fact]
    public async Task Student_SelfSoftDelete_PreventsSubsequentLoginAndDirectoryInclusion()
    {
        var email = $"delete_me_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Tamer", "Hosny", email, "Password123!", 1, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Self soft-delete
        var delRes = await client.DeleteAsync("/api/v1/students/me");
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        // Subsequent login must fail
        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "Password123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, loginRes.StatusCode);

        // Deleted student's existing access token is immediately revoked (STRIDE THR-E-02)
        var selfAfterDelete = await client.GetAsync("/api/v1/students?search=Tamer");
        Assert.Equal(HttpStatusCode.Unauthorized, selfAfterDelete.StatusCode);

        // Subsequent directory search by another active student must not include deleted student
        using var viewerClient = factory.CreateClient();
        viewerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateStudentToken(7777));
        var dirRes = await viewerClient.GetAsync("/api/v1/students?search=Tamer");
        Assert.Equal(HttpStatusCode.OK, dirRes.StatusCode);
        var dir = await GetDataAsync<TeamUp.Application.Common.PagedResult<StudentCardDto>>(dirRes);
        Assert.DoesNotContain(dir.Items, s => s.StudentId == auth.Student.StudentId);
    }

    [Fact]
    public async Task PasswordRecovery_FullFlow_WorksAsExpected()
    {
        var email = $"recovery_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Kareem", "Reset", email, "InitialPassword123!", 2, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        // 1. Request forgot password - must not leak reset token in HTTP response
        var forgotRes = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(email));
        Assert.Equal(HttpStatusCode.OK, forgotRes.StatusCode);
        var forgot = await GetDataAsync<ForgotPasswordResponse>(forgotRes);
        Assert.Equal("If your email is registered, a password reset link has been dispatched.", forgot.Message);

        // 2. Retrieve dispatched token from out-of-band email sink and verify DB stores hash
        var emailSender = factory.Services.GetRequiredService<InMemoryEmailSender>();
        var resetToken = emailSender.GetLastResetToken(email);
        Assert.NotNull(resetToken);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var student = await db.Students.FirstAsync(s => s.Email == email);
            Assert.NotNull(student.PasswordResetToken);
            Assert.NotEqual(resetToken, student.PasswordResetToken); // Token in DB must be hashed (SHA-256)
        }

        // 3. Reset password with valid token
        var newPassword = "BrandNewPassword2026!";
        var resetRes = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", new ResetPasswordRequest(email, resetToken, newPassword));
        Assert.Equal(HttpStatusCode.OK, resetRes.StatusCode);

        // 4. Login with old password fails
        var oldLoginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "InitialPassword123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, oldLoginRes.StatusCode);

        // 5. Login with new password succeeds
        var newLoginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, newPassword));
        Assert.Equal(HttpStatusCode.OK, newLoginRes.StatusCode);
    }

    [Fact]
    public async Task PasswordRecovery_InvalidOrExpiredToken_Fails()
    {
        var email = $"recovery_fail_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Sami", "Fail", email, "ValidPassword123!", 2, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        // Invalid token
        var invalidReset = await _client.PostAsJsonAsync(
            "/api/v1/auth/reset-password",
            new ResetPasswordRequest(email, "completely_invalid_token", "BrandNewPassword2026!"));
        Assert.Equal(HttpStatusCode.Unauthorized, invalidReset.StatusCode);
    }

    [Fact]
    public async Task PublicPeopleImagesAndShowcaseEndpoint_ServeDatabasePortraitsWithoutPii()
    {
        // 1. Public static image in wwwroot/images/people is served without authentication
        var staticImgRes = await _client.GetAsync("/images/people/salma-khaled.jpg");
        Assert.Equal(HttpStatusCode.OK, staticImgRes.StatusCode);
        Assert.Equal("image/jpeg", staticImgRes.Content.Headers.ContentType?.MediaType);

        // 2. Register a student and store the public relative PhotoUrl in their database profile
        var email = $"showcase_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Salma", "Showcase", email, "Password123!", 4, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        using var authClient = factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var upsertRes = await authClient.PutAsJsonAsync(
            "/api/v1/students/me/profile",
            new UpsertProfileRequest("AI & React builder", "/images/people/salma-khaled.jpg", null, null));
        Assert.Equal(HttpStatusCode.OK, upsertRes.StatusCode);

        // 3. Unauthenticated GET /api/v1/students/showcase exposes PhotoUrl & zero PII (no private email)
        var showcaseRes = await _client.GetAsync("/api/v1/students/showcase?limit=6");
        Assert.Equal(HttpStatusCode.OK, showcaseRes.StatusCode);
        var showcaseRaw = await showcaseRes.Content.ReadAsStringAsync();
        Assert.DoesNotContain(email, showcaseRaw, StringComparison.OrdinalIgnoreCase);

        var showcase = await GetDataAsync<List<PublicStudentShowcaseDto>>(showcaseRes);
        Assert.Contains(showcase, s =>
            s.StudentId == auth.Student.StudentId
            && s.PhotoUrl == "/images/people/salma-khaled.jpg"
            && s.FirstName == "Salma");

        // 4. Directory cards & leaderboard also return the database PhotoUrl
        var dirRes = await authClient.GetAsync("/api/v1/students?search=Showcase");
        Assert.Equal(HttpStatusCode.OK, dirRes.StatusCode);
        var dir = await GetDataAsync<PagedResult<StudentCardDto>>(dirRes);
        Assert.Contains(dir.Items, s =>
            s.StudentId == auth.Student.StudentId
            && s.PhotoUrl == "/images/people/salma-khaled.jpg");

        var lbRes = await authClient.GetAsync("/api/v1/gamification/leaderboard?page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, lbRes.StatusCode);
        var lb = await GetDataAsync<PagedResult<LeaderboardEntryDto>>(lbRes);
        Assert.Contains(lb.Items, e =>
            e.StudentId == auth.Student.StudentId
            && e.PhotoUrl == "/images/people/salma-khaled.jpg");
    }

    [Fact]
    public async Task UploadProfilePhoto_ValidatesMagicBytes_StoresPublicImage_AndUpdatesDatabase()
    {
        // 1. Unauthenticated upload must return 401 Unauthorized
        using var emptyContent = new MultipartFormDataContent();
        var unauthRes = await _client.PostAsync("/api/v1/students/me/profile/photo", emptyContent);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthRes.StatusCode);

        // 2. Register and authenticate student
        var email = $"upload_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Nadia", "Uploader", email, "Password123!", 3, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        using var authClient = factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // 3. Spoofed SVG disguised with .jpg filename/Content-Type must return 400 Bad Request
        using (var spoofForm = new MultipartFormDataContent())
        {
            var svgBytes = "<svg xmlns='http://www.w3.org/2000/svg'><script>alert(1)</script></svg>"u8.ToArray();
            var spoofFile = new ByteArrayContent(svgBytes);
            spoofFile.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            spoofForm.Add(spoofFile, "file", "evil.jpg");

            var spoofRes = await authClient.PostAsync("/api/v1/students/me/profile/photo", spoofForm);
            Assert.Equal(HttpStatusCode.BadRequest, spoofRes.StatusCode);
        }

        // 4. Valid 1x1 PNG upload returns 200 OK, writes file publicly, and updates database profile
        var validPng = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+ip1sAAAAASUVORK5CYII=");

        string? uploadedPhotoUrl = null;
        try
        {
            using var validForm = new MultipartFormDataContent();
            var pngContent = new ByteArrayContent(validPng);
            pngContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            validForm.Add(pngContent, "file", "portrait.png");

            var uploadRes = await authClient.PostAsync("/api/v1/students/me/profile/photo", validForm);
            Assert.Equal(HttpStatusCode.OK, uploadRes.StatusCode);

            var profile = await GetDataAsync<StudentProfileDto>(uploadRes);
            Assert.NotNull(profile.PhotoUrl);
            Assert.Matches($"^/images/people/student-{auth.Student.StudentId}-[a-f0-9]{{32}}\\.png$", profile.PhotoUrl);
            uploadedPhotoUrl = profile.PhotoUrl;

            // Public static serving verification (unauthenticated)
            var publicGet = await _client.GetAsync(uploadedPhotoUrl);
            Assert.Equal(HttpStatusCode.OK, publicGet.StatusCode);
            Assert.Equal("image/png", publicGet.Content.Headers.ContentType?.MediaType);

            // Persisted in /students/me and public /students/showcase
            var me = await GetDataAsync<StudentDetailDto>(await authClient.GetAsync("/api/v1/students/me"));
            Assert.Equal(uploadedPhotoUrl, me.Profile?.PhotoUrl);

            var showcaseRaw = await (await _client.GetAsync("/api/v1/students/showcase?limit=12")).Content.ReadAsStringAsync();
            Assert.Contains(uploadedPhotoUrl, showcaseRaw);
            Assert.DoesNotContain(email, showcaseRaw, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(uploadedPhotoUrl))
            {
                var env = factory.Services.GetRequiredService<IWebHostEnvironment>();
                var webRoot = string.IsNullOrWhiteSpace(env.WebRootPath)
                    ? Path.Combine(env.ContentRootPath, "wwwroot")
                    : env.WebRootPath;
                var fullPath = Path.Combine(webRoot, uploadedPhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
        }
    }
}
