// Date: 2026-11-21
// Purpose: Centralized IMemoryCache keys and SecurityStamp lookup cache for immediate token revocation (STRIDE THR-E-02 + Phase 18 caching).

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using TeamUp.Application.Common.Interfaces;

namespace TeamUp.Application.Common.Caching;

public static class CatalogCacheKeys
{
    public const string AcademicCatalog = "catalog:universities:academic";
    public const string MasterSkills = "catalog:skills:all";
    public const string BadgeCatalog = "catalog:badges:all";
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(1);

    public static readonly HybridCacheEntryOptions CatalogEntryOptions = new()
    {
        Expiration = DefaultTtl,
        LocalCacheExpiration = DefaultTtl
    };

    public static class Tags
    {
        public const string AcademicCatalog = "tag:catalog:universities";
        public const string MasterSkills = "tag:catalog:skills";
        public const string BadgeCatalog = "tag:catalog:badges";
    }

    public static string SecurityStampKey(int studentId) => $"auth:security-stamp:{studentId}";
}

public sealed record StudentSecurityState(Guid SecurityStamp, string Role);

public interface ISecurityStampValidator
{
    Task<StudentSecurityState?> GetActiveStudentStateAsync(int studentId, CancellationToken ct = default);
    void Invalidate(int studentId);
}

public sealed class SecurityStampValidator(IAppDbContext db, IMemoryCache cache) : ISecurityStampValidator
{
    private static readonly TimeSpan StampCacheTtl = TimeSpan.FromSeconds(30);

    public async Task<StudentSecurityState?> GetActiveStudentStateAsync(int studentId, CancellationToken ct = default)
    {
        if (studentId <= 0)
            return null;

        var key = CatalogCacheKeys.SecurityStampKey(studentId);
        if (cache.TryGetValue(key, out StudentSecurityState? cached))
            return cached;

        var state = await db.Students
            .AsNoTracking()
            .Where(s => s.StudentId == studentId)
            .Select(s => new StudentSecurityState(s.SecurityStamp, s.Role.ToString()))
            .FirstOrDefaultAsync(ct);

        if (state is not null)
        {
            cache.Set(key, state, StampCacheTtl);
        }

        return state;
    }

    public void Invalidate(int studentId)
    {
        if (studentId > 0)
            cache.Remove(CatalogCacheKeys.SecurityStampKey(studentId));
    }
}
