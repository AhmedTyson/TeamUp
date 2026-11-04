using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TeamUp.Api.Auth;
using TeamUp.Api.Data;
using TeamUp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Database (SQLite)
var dbPath = builder.Configuration.GetValue<string>("DbPath") ?? "teamup.db";
builder.Services.AddDbContext<TeamUpDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

// 2. Add Business Services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBadgeService, BadgeService>();
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// 3. Add Controllers
builder.Services.AddControllers();

// 4. Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = JwtTokenService.Issuer,
        ValidAudience = JwtTokenService.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenService.SecretKey))
    };
});

builder.Services.AddAuthorization();

// 5. Add Swagger with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TeamUp Master Backend API (T22–T27) [C# .NET 10]",
        Version = "v1",
        Description = "Master Backend Implementation in C# / ASP.NET Core for Tasks T22 through T27 according to the Master Specification."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Seed initial database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TeamUpDbContext>();
    db.Database.EnsureCreated();
    db.SeedInitialData();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamUp API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Runtime = ".NET 10.0 (C#)",
    Service = "TeamUp Master Backend",
    Scope = "T22-T27",
    EndpointsCount = 24
}));

// Root redirect to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

public partial class Program { }
