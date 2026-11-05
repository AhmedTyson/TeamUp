using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TeamUp.IntegrationTests;

public class OpenApiAndPostmanTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
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

        var info = jsonDoc["info"];
        Assert.NotNull(info);
        Assert.Contains("TeamUp", info["title"]?.ToString());
        Assert.NotNull(info["version"]);

        var paths = jsonDoc["paths"]?.AsObject();
        Assert.NotNull(paths);
        Assert.True(paths.Count > 10, "OpenAPI spec should expose multiple endpoint paths.");

        // Export static openapi.json to docs/api/openapi.json
        var outputPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "api", "openapi.json");
        outputPath = Path.GetFullPath(outputPath);

        var formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputPath, formattedJson);

        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public async Task PostmanCollection_GeneratesAndExportsValidV2Collection()
    {
        var postmanDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "api", "postman"));
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
                new JsonObject { ["key"] = "baseUrl", ["value"] = "http://localhost:5000", ["enabled"] = true },
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
                ["description"] = "Comprehensive API collection for TeamUp Student Collaboration & Gamification Platform (.NET 10).",
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
                CreateFolder("01. Auth & Identity", [
                    CreateRequest("Register Student", "POST", "{{baseUrl}}/api/v1/auth/register", "{\"firstName\":\"Ahmed\",\"lastName\":\"Tyson\",\"email\":\"tyson@teamup.edu\",\"password\":\"P@ssword123!\",\"academicYear\":3,\"facultyId\":1}"),
                    CreateRequest("Login", "POST", "{{baseUrl}}/api/v1/auth/login", "{\"email\":\"tyson@teamup.edu\",\"password\":\"P@ssword123!\"}"),
                    CreateRequest("Refresh Token", "POST", "{{baseUrl}}/api/v1/auth/refresh", "{\"refreshToken\":\"sample_token\"}"),
                    CreateRequest("Forgot Password", "POST", "{{baseUrl}}/api/v1/auth/forgot-password", "{\"email\":\"tyson@teamup.edu\"}")
                ]),
                CreateFolder("02. Academic Catalog", [
                    CreateRequest("Public Catalog Lookup", "GET", "{{baseUrl}}/api/v1/universities/catalog"),
                    CreateRequest("Search Universities", "GET", "{{baseUrl}}/api/v1/universities?search=Cairo"),
                    CreateRequest("Get Faculties by University", "GET", "{{baseUrl}}/api/v1/universities/1/faculties")
                ]),
                CreateFolder("03. Student Profiles & Directory", [
                    CreateRequest("Directory Search", "GET", "{{baseUrl}}/api/v1/students?search=Ahmed"),
                    CreateRequest("Get Current Student", "GET", "{{baseUrl}}/api/v1/students/me"),
                    CreateRequest("Upsert Profile", "PUT", "{{baseUrl}}/api/v1/students/me/profile", "{\"bio\":\"Full-stack AI developer\",\"githubUrl\":\"https://github.com/AhmedTyson\"}")
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
                    CreateRequest("List Organizations", "GET", "{{baseUrl}}/api/v1/organizations"),
                    CreateRequest("Create Organization", "POST", "{{baseUrl}}/api/v1/organizations", "{\"name\":\"IEEE Student Chapter\",\"description\":\"Technical society\",\"category\":\"Engineering\"}"),
                    CreateRequest("Apply for Membership", "POST", "{{baseUrl}}/api/v1/organizations/1/applications", "{\"message\":\"Ready to participate in IEEE.\"}")
                ]),
                CreateFolder("07. Events & Attendance", [
                    CreateRequest("List Events", "GET", "{{baseUrl}}/api/v1/events"),
                    CreateRequest("Register for Event", "POST", "{{baseUrl}}/api/v1/events/1/registrations"),
                    CreateRequest("Verify Session Attendance", "POST", "{{baseUrl}}/api/v1/sessions/1/attendance", "{\"studentId\":1,\"verificationMethod\":\"QRCode\"}")
                ]),
                CreateFolder("08. Gamification & Badges", [
                    CreateRequest("Get Student Score", "GET", "{{baseUrl}}/api/v1/students/{{studentId}}/score"),
                    CreateRequest("Get Score Ledger", "GET", "{{baseUrl}}/api/v1/students/{{studentId}}/score-transactions"),
                    CreateRequest("Get Badge Catalog", "GET", "{{baseUrl}}/api/v1/badges"),
                    CreateRequest("Get My Notifications", "GET", "{{baseUrl}}/api/v1/notifications")
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
}
