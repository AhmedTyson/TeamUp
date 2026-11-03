using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Students;

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
        Assert.Equal(400, err.Status);
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

        // Subsquent search must not include deleted student
        var dirRes = await client.GetAsync($"/api/v1/students?search=Tamer");
        var dir = await GetDataAsync<TeamUp.Application.Common.PagedResult<StudentCardDto>>(dirRes);
        Assert.DoesNotContain(dir.Items, s => s.Email == email);
    }

    [Fact]
    public async Task PasswordRecovery_FullFlow_WorksAsExpected()
    {
        var email = $"recovery_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Kareem", "Reset", email, "InitialPassword123!", 2, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        // 1. Request forgot password
        var forgotRes = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new ForgotPasswordRequest(email));
        Assert.Equal(HttpStatusCode.OK, forgotRes.StatusCode);
        var forgot = await GetDataAsync<ForgotPasswordResponse>(forgotRes);
        Assert.NotNull(forgot.ResetToken);

        // 2. Reset password with valid token
        var newPassword = "BrandNewPassword2026!";
        var resetRes = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", new ResetPasswordRequest(email, forgot.ResetToken, newPassword));
        Assert.Equal(HttpStatusCode.OK, resetRes.StatusCode);

        // 3. Login with old password fails
        var oldLoginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, "InitialPassword123!"));
        Assert.Equal(HttpStatusCode.Unauthorized, oldLoginRes.StatusCode);

        // 4. Login with new password succeeds
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
}
