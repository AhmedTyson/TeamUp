using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("TeamUpDb")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:TeamUpDb is missing. Set it with user-secrets or the ConnectionStrings__TeamUpDb env var.");

        services.AddSingleton<SoftDeleteInterceptor>();
        services.AddDbContext<AppDbContext>((sp, o) => o
            .UseSqlServer(cs, sql => sql.EnableRetryOnFailure())
            .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>()));
        return services;
    }
}
