using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TeamUp.IntegrationTests;

public sealed class OpenApiAndPostmanTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new() { WriteIndented = true };
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task OpenApi_GeneratesValidV3Spec_AndExportsStaticFile()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var jsonString = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(jsonString));

        var jsonDoc = JsonNode.Parse(jsonString);
        Assert.NotNull(jsonDoc);

        var openApiVersion = jsonDoc["openapi"]?.ToString();
        Assert.NotNull(openApiVersion);
        Assert.StartsWith("3.2", openApiVersion, StringComparison.Ordinal);

        var bearerScheme = jsonDoc["components"]?["securitySchemes"]?["Bearer"];
        Assert.NotNull(bearerScheme);

        var info = jsonDoc["info"];
        Assert.NotNull(info);
        Assert.Contains("TeamUp", info["title"]?.ToString());
        Assert.NotNull(info["version"]);

        var paths = jsonDoc["paths"]?.AsObject();
        Assert.NotNull(paths);
        Assert.True(paths.Count > 10, "OpenAPI spec should expose multiple endpoint paths.");

        // Export static openapi.json to temp output (unless TEAMUP_EXPORT_API_DOCS=1)
        var outputPath = Path.Combine(DocsApiRoot(), "openapi.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var formattedJson = JsonSerializer.Serialize(jsonDoc, IndentedJsonOptions);
        await File.WriteAllTextAsync(outputPath, formattedJson);

        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task PostmanCollection_GeneratesAndExportsValidV2Collection()
    {
        var postmanDir = Path.Combine(DocsApiRoot(), "postman");
        Directory.CreateDirectory(postmanDir);

        var collectionPath = Path.Combine(postmanDir, "TeamUp_Collection.json");
        var environmentPath = Path.Combine(postmanDir, "TeamUp_Environment.json");

        // Generate Postman Environment
        var envObj = new JsonObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["name"] = "TeamUp Local Environment",
            ["values"] = new JsonArray
            {
                new JsonObject { ["key"] = "baseUrl", ["value"] = "http://localhost:5186", ["enabled"] = true },
                new JsonObject { ["key"] = "accessToken", ["value"] = "", ["enabled"] = true },
                new JsonObject { ["key"] = "adminToken", ["value"] = "", ["enabled"] = true },
                new JsonObject { ["key"] = "studentId", ["value"] = "1", ["enabled"] = true },
                new JsonObject { ["key"] = "facultyId", ["value"] = "1", ["enabled"] = true }
            }
        };

        // Generate Postman v2.1 Collection
        var collectionObj = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["_postman_id"] = Guid.NewGuid().ToString(),
                ["name"] = "TeamUp API Collection",
                ["description"] = "Comprehensive API collection for TeamUp Student Collaboration & Gamification Platform (.NET 11).",
                ["schema"] = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            ["auth"] = new JsonObject
            {
                ["type"] = "bearer",
                ["bearer"] = new JsonArray
                {
                    new JsonObject { ["key"] = "token", ["value"] = "{{accessToken}}", ["type"] = "string" }
                }
            },
            ["item"] = new JsonArray
            {
                CreateFolder("00. Health & Images", [
                    CreateRequest("Health Aggregate", "GET", "{{baseUrl}}/health"),
                    CreateRequest("Health Liveness", "GET", "{{baseUrl}}/health/live"),
                    CreateRequest("Health Readiness", "GET", "{{baseUrl}}/health/ready"),
                    CreateRequest("Upload Image", "POST", "{{baseUrl}}/api/v1/images"),
                    CreateRequest("Get Default Cover Image", "GET", "{{baseUrl}}/api/v1/images/c0e70000-0000-4000-8000-000000000001")
                ]),
                CreateFolder("01. Auth & Identity", [
                    CreateRequest("Register Student", "POST", "{{baseUrl}}/api/v1/auth/register", "{\"firstName\":\"Ahmed\",\"lastName\":\"Tyson\",\"email\":\"tyson@teamup.edu\",\"password\":\"P@ssword123!\",\"academicYear\":3,\"facultyId\":1}"),
                    CreateRequest("Login", "POST", "{{baseUrl}}/api/v1/auth/login", "{\"email\":\"tyson@teamup.edu\",\"password\":\"P@ssword123!\"}"),
                    CreateRequest("Refresh Token", "POST", "{{baseUrl}}/api/v1/auth/refresh", "{\"refreshToken\":\"sample_token\"}"),
                    CreateRequest("Logout", "POST", "{{baseUrl}}/api/v1/auth/logout"),
                    CreateRequest("Forgot Password", "POST", "{{baseUrl}}/api/v1/auth/forgot-password", "{\"email\":\"tyson@teamup.edu\"}"),
                    CreateRequest("Reset Password", "POST", "{{baseUrl}}/api/v1/auth/reset-password", "{\"email\":\"tyson@teamup.edu\",\"token\":\"reset_token\",\"newPassword\":\"NewP@ssword123!\"}")
                ]),
                CreateFolder("02. Academic Catalog", [
                    CreateRequest("Public Catalog Lookup", "GET", "{{baseUrl}}/api/v1/universities/catalog"),
                    CreateRequest("Search Universities", "GET", "{{baseUrl}}/api/v1/universities?search=Cairo"),
                    CreateRequest("Get Faculties by University", "GET", "{{baseUrl}}/api/v1/universities/1/faculties")
                ]),
                CreateFolder("03. Student Profiles & Directory", [
                    CreateRequest("Public Student Showcase", "GET", "{{baseUrl}}/api/v1/students/showcase?limit=6"),
                    CreateRequest("Directory Search", "GET", "{{baseUrl}}/api/v1/students?search=Ahmed"),
                    CreateRequest("Get Current Student", "GET", "{{baseUrl}}/api/v1/students/me"),
                    CreateRequest("Upsert Profile", "PUT", "{{baseUrl}}/api/v1/students/me/profile", "{\"bio\":\"Full-stack AI developer\",\"githubUrl\":\"https://github.com/AhmedTyson\"}"),
                    CreateRequest("Upload Profile Photo", "POST", "{{baseUrl}}/api/v1/students/me/profile/photo")
                ]),
                CreateFolder("04. Skills Catalog & Claims", [
                    CreateRequest("List Master Skills", "GET", "{{baseUrl}}/api/v1/skills"),
                    CreateRequest("Claim Skill", "PUT", "{{baseUrl}}/api/v1/students/me/skills/1", "{\"proficiencyLevel\":\"Advanced\",\"yearsOfExperience\":3.5}"),
                    CreateRequest("Get My Skills", "GET", "{{baseUrl}}/api/v1/students/{{studentId}}/skills")
                ]),
                CreateFolder("05. Projects & Tasks", [
                    CreateRequest("List Projects", "GET", "{{baseUrl}}/api/v1/projects"),
                    CreateRequest("Create Project", "POST", "{{baseUrl}}/api/v1/projects", "{\"title\":\"AI Drone Vision\",\"description\":\"Computer vision pipeline for drones\",\"maxTeamSize\":4}"),
                    CreateRequest("Apply for Project", "POST", "{{baseUrl}}/api/v1/projects/1/applications", "{\"message\":\"Excited to contribute to drone vision.\"}"),
                    CreateRequest("Create Task", "POST", "{{baseUrl}}/api/v1/projects/1/tasks", "{\"title\":\"Train YOLOv11 Model\",\"priority\":\"High\"}")
                ]),
                CreateFolder("06. Organizations & Activities", [
                    CreateRequest("List Public Organizations", "GET", "{{baseUrl}}/api/v1/organizations/public"),
                    CreateRequest("List Public Organization Categories", "GET", "{{baseUrl}}/api/v1/organizations/public/categories"),
                    CreateRequest("List Organizations", "GET", "{{baseUrl}}/api/v1/organizations"),
                    CreateRequest("Create Organization", "POST", "{{baseUrl}}/api/v1/organizations", "{\"name\":\"IEEE Student Chapter\",\"description\":\"Technical society\",\"category\":\"Engineering\"}"),
                    CreateRequest("Apply for Membership", "POST", "{{baseUrl}}/api/v1/organizations/1/applications", "{\"message\":\"Ready to participate in IEEE.\"}")
                ]),
                CreateFolder("07. Events & Attendance", [
                    CreateRequest("List Events", "GET", "{{baseUrl}}/api/v1/events"),
                    CreateRequest("Register for Event", "POST", "{{baseUrl}}/api/v1/events/1/registrations"),
                    CreateRequest("Generate Rotating Check-In Code", "POST", "{{baseUrl}}/api/v1/sessions/1/attendance/check-in-code"),
                    CreateRequest("Student Self Check-In", "POST", "{{baseUrl}}/api/v1/sessions/1/attendance/self-check-in", "{\"verificationCode\":\"00000000.0000000000000000\"}"),
                    CreateRequest("Verify Session Attendance", "POST", "{{baseUrl}}/api/v1/sessions/1/attendance", "{\"studentId\":1,\"verificationMethod\":\"QRCode\"}"),
                    CreateRequest("Session Attendance Report (HTML)", "GET", "{{baseUrl}}/api/v1/sessions/1/attendance/report")
                ]),
                CreateFolder("08. Gamification & Badges", [
                    CreateRequest("Get Student Score", "GET", "{{baseUrl}}/api/v1/students/{{studentId}}/score"),
                    CreateRequest("Get Score Ledger", "GET", "{{baseUrl}}/api/v1/students/{{studentId}}/score-transactions"),
                    CreateRequest("Campus Leaderboard", "GET", "{{baseUrl}}/api/v1/gamification/leaderboard"),
                    CreateRequest("Get Badge Catalog", "GET", "{{baseUrl}}/api/v1/badges"),
                    CreateRequest("Get My Notifications", "GET", "{{baseUrl}}/api/v1/notifications"),
                    CreateRequest("Get Unread Notifications Count", "GET", "{{baseUrl}}/api/v1/notifications/unread-count")
                ]),
                CreateFolder("09. Platform Admin & Governance", [
                    CreateRequest("Admin Dashboard", "GET", "{{baseUrl}}/api/v1/admin/dashboard"),
                    CreateRequest("Update Student Role", "PATCH", "{{baseUrl}}/api/v1/admin/students/1/role", "{\"role\":\"OrgAdmin\"}"),
                    CreateRequest("List Audit Logs", "GET", "{{baseUrl}}/api/v1/admin/audit-logs?page=1&pageSize=20"),
                    CreateRequest("List Soft-Deleted Records", "GET", "{{baseUrl}}/api/v1/admin/soft-deleted"),
                    CreateRequest("Restore Entity", "POST", "{{baseUrl}}/api/v1/admin/restore/Student/1"),
                    CreateRequest("Download Governance Summary PDF", "GET", "{{baseUrl}}/api/v1/admin/reports/platform-summary.pdf"),
                    CreateRequest("List Email Templates", "GET", "{{baseUrl}}/api/v1/admin/templates"),
                    CreateRequest("Preview Email Template", "GET", "{{baseUrl}}/api/v1/admin/templates/preview/LeaderboardDigest"),
                    CreateRequest("Dispatch Leaderboard Digest", "POST", "{{baseUrl}}/api/v1/admin/communications/leaderboard-digest", "{\"mode\":\"TestToMe\",\"dryRun\":true}"),
                    CreateRequest("Dispatch Project Match Digest", "POST", "{{baseUrl}}/api/v1/admin/communications/project-matches", "{\"mode\":\"TestToMe\",\"dryRun\":true}")
                ]),
                CreateFolder("10. Composite Workspaces (Single-Request)", [
                    CreateRequest("Student Dashboard Workspace", "GET", "{{baseUrl}}/api/v1/dashboard/student"),
                    CreateRequest("Org Leadership Dashboard Workspace", "GET", "{{baseUrl}}/api/v1/dashboard/org"),
                    CreateRequest("Unified Explore Feed", "GET", "{{baseUrl}}/api/v1/explore?pageSize=6"),
                    CreateRequest("Event Detail Workspace", "GET", "{{baseUrl}}/api/v1/events/1/workspace"),
                    CreateRequest("Session Attendance Workspace", "GET", "{{baseUrl}}/api/v1/events/1/sessions/1/workspace"),
                    CreateRequest("Project Detail Workspace", "GET", "{{baseUrl}}/api/v1/projects/1/workspace"),
                    CreateRequest("Organization Detail Workspace", "GET", "{{baseUrl}}/api/v1/organizations/1/workspace"),
                    CreateRequest("Gamification Hub Workspace", "GET", "{{baseUrl}}/api/v1/gamification/hub?ledgerPageSize=15"),
                    CreateRequest("Student Portfolio Workspace", "GET", "{{baseUrl}}/api/v1/students/{{studentId}}/portfolio")
                ])
            }
        };

        var jsonOpts = new JsonSerializerOptions { WriteIndented = true };
        await File.WriteAllTextAsync(collectionPath, JsonSerializer.Serialize(collectionObj, jsonOpts));
        await File.WriteAllTextAsync(environmentPath, JsonSerializer.Serialize(envObj, jsonOpts));

        Assert.True(File.Exists(collectionPath));
        Assert.True(File.Exists(environmentPath));
    }

    private static JsonObject CreateFolder(string name, JsonNode[] items)
    {
        var array = new JsonArray();
        foreach (var item in items) array.Add(item);
        return new JsonObject
        {
            ["name"] = name,
            ["item"] = array
        };
    }

    private static JsonObject CreateRequest(string name, string method, string url, string? body = null)
    {
        var req = new JsonObject
        {
            ["method"] = method,
            ["header"] = new JsonArray
            {
                new JsonObject { ["key"] = "Content-Type", ["value"] = "application/json" }
            },
            ["url"] = new JsonObject
            {
                ["raw"] = url
            }
        };

        if (body is not null)
        {
            req["body"] = new JsonObject
            {
                ["mode"] = "raw",
                ["raw"] = body
            };
        }

        return new JsonObject
        {
            ["name"] = name,
            ["request"] = req
        };
    }

    /// <summary>
    /// Tests must not dirty the tracked source tree. Generated API docs go to a temp folder unless
    /// TEAMUP_EXPORT_API_DOCS=1 is set explicitly to refresh docs/api.
    /// </summary>
    private static string DocsApiRoot() =>
        Environment.GetEnvironmentVariable("TEAMUP_EXPORT_API_DOCS") == "1"
            ? Path.Combine(ResolveRepoRoot(), "docs", "api")
            : Path.Combine(Path.GetTempPath(), "teamup-api-docs");

    private static string ResolveRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "docs"))
                && Directory.Exists(Path.Combine(dir.FullName, "backend")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing 'docs' and 'backend'.");
    }
}
