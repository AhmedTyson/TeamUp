using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TeamUp.Application.Auth;
using TeamUp.Application.Students;

namespace TeamUp.IntegrationTests;

public class AuthAndProfileTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_Returns201_AndSetsJwtToken()
    {
        var email = $"register_{Guid.NewGuid():N}@test.com";
        var req = new RegisterRequest(email, "P@ssword123!", "John", "Doe", 2, factory.SeededFacultyId);

        var res = await _client.PostAsJsonAsync("/api/v1/auth/register", req);

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
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
        var req = new RegisterRequest(email, "P@ssword123!", "Alice", "Smith", 1, factory.SeededFacultyId);

        var first = await _client.PostAsJsonAsync("/api/v1/auth/register", req);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/v1/auth/register", req);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidPayload_Returns400WithStructuredValidationErrors()
    {
        var req = new RegisterRequest("not-an-email", "weak", "", "", 0, -1);

        var res = await _client.PostAsJsonAsync("/api/v1/auth/register", req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var err = await res.Content.ReadFromJsonAsync<TeamUp.Application.Common.Models.ErrorResponse>();
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
        var req = new RegisterRequest(email, "P@ssword123!", "Bob", "Smith", 1, 999999);

        var res = await _client.PostAsJsonAsync("/api/v1/auth/register", req);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200AndTokens()
    {
        var email = $"login_ok_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest(email, "Secret123!", "Omar", "Tarek", 3, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        var login = new LoginRequest(email, "Secret123!");
        var res = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401Unauthorized()
    {
        var email = $"bad_pass_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest(email, "Correct123!", "Sara", "Ali", 1, factory.SeededFacultyId);
        await _client.PostAsJsonAsync("/api/v1/auth/register", reg);

        var login = new LoginRequest(email, "WrongPassword!");
        var res = await _client.PostAsJsonAsync("/api/v1/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Valid_ReturnsRotatedToken()
    {
        var email = $"refresh_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest(email, "Secret123!", "Laila", "Hassan", 2, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var initialAuth = await regRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(initialAuth);

        var refreshReq = new RefreshTokenRequest(initialAuth.RefreshToken);
        var res = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshReq);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var newAuth = await res.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(newAuth);
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
        var reg = new RegisterRequest(email, "Password123!", "Kareem", "Adel", 2, factory.SeededFacultyId);
        var regRes = await _client.PostAsJsonAsync("/api/v1/auth/register", reg);
        var auth = await regRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // 2. Get /students/me
        var meRes = await client.GetAsync("/api/v1/students/me");
        Assert.Equal(HttpStatusCode.OK, meRes.StatusCode);
        var me = await meRes.Content.ReadFromJsonAsync<StudentDetailDto>();
        Assert.NotNull(me);
        Assert.Equal("Kareem", me.FirstName);
        Assert.Equal("Adel", me.LastName);
        Assert.Equal(2, me.AcademicYear);
        Assert.Null(me.Profile);

        // 3. Update /students/me
        var updateRes = await client.PatchAsJsonAsync("/api/v1/students/me", new UpdateStudentRequest("Karim", "Adel", 3));
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updatedMe = await updateRes.Content.ReadFromJsonAsync<StudentDetailDto>();
        Assert.NotNull(updatedMe);
        Assert.Equal("Karim", updatedMe.FirstName);
        Assert.Equal(3, updatedMe.AcademicYear);

        // 4. Upsert extended profile
        var profileReq = new UpsertProfileRequest("Passionate about AI", "https://img.local/avatar.png", "https://github.com/karim", "https://linkedin.com/in/karim");
        var upsertRes = await client.PutAsJsonAsync("/api/v1/students/me/profile", profileReq);
        Assert.Equal(HttpStatusCode.OK, upsertRes.StatusCode);
        var profileDto = await upsertRes.Content.ReadFromJsonAsync<StudentProfileDto>();
        Assert.NotNull(profileDto);
        Assert.Equal("Passionate about AI", profileDto.Bio);
        Assert.Equal("https://github.com/karim", profileDto.GitHubUrl);

        // 5. Fetch public extended profile by ID
        var publicProfileRes = await client.GetAsync($"/api/v1/students/{me.StudentId}/profile");
        Assert.Equal(HttpStatusCode.OK, publicProfileRes.StatusCode);
        var publicProfile = await publicProfileRes.Content.ReadFromJsonAsync<StudentProfileDto>();
        Assert.NotNull(publicProfile);
        Assert.Equal("Passionate about AI", publicProfile.Bio);

        // 6. Manage interests
        var addRes = await client.PostAsJsonAsync("/api/v1/students/me/interests", new AddInterestRequest("Artificial Intelligence"));
        Assert.Equal(HttpStatusCode.OK, addRes.StatusCode);

        var listInterestsRes = await client.GetAsync($"/api/v1/students/{me.StudentId}/interests");
        Assert.Equal(HttpStatusCode.OK, listInterestsRes.StatusCode);
        var interests = await listInterestsRes.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(interests);
        Assert.Contains("Artificial Intelligence", interests);

        // 7. Delete interest (soft delete)
        var deleteInterestRes = await client.DeleteAsync("/api/v1/students/me/interests/Artificial Intelligence");
        Assert.Equal(HttpStatusCode.NoContent, deleteInterestRes.StatusCode);

        var listAfterDelete = await (await client.GetAsync($"/api/v1/students/{me.StudentId}/interests")).Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(listAfterDelete);
        Assert.DoesNotContain("Artificial Intelligence", listAfterDelete);

        // 8. Re-adding the same interest tag restores the soft-deleted record without throwing unique PK error
        var reAddRes = await client.PostAsJsonAsync("/api/v1/students/me/interests", new AddInterestRequest("Artificial Intelligence"));
        Assert.Equal(HttpStatusCode.OK, reAddRes.StatusCode);

        var listAfterReAdd = await (await client.GetAsync($"/api/v1/students/{me.StudentId}/interests")).Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(listAfterReAdd);
        Assert.Contains("Artificial Intelligence", listAfterReAdd);

        // 9. Logout
        var logoutRes = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logoutRes.StatusCode);

        // 10. Refresh with logged-out token fails
        var refreshAfterLogout = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new RefreshTokenRequest(auth.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }
}
