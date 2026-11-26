// Date: 2026-11-25
// Purpose: Catalog of generated default cover images. Each cover has a deterministic GUID so URLs are stable
// across environments and restarts; the API seeds the rows into StoredImages at startup and falls back to the
// SeedImages content files copied to the output directory, so a default cover URL never 404s.

namespace TeamUp.Application.Common.Images;

public static class DefaultCoverCatalog
{
    public sealed record Entry(Guid ImageId, string FileName, string ContentType, string Theme);

    public static readonly IReadOnlyList<Entry> Covers =
    [
        new(new Guid("c0e70000-0000-4000-8000-000000000001"), "cover-01.png", "image/png", "lime-arcs"),
        new(new Guid("c0e70000-0000-4000-8000-000000000002"), "cover-02.png", "image/png", "teal-waves"),
        new(new Guid("c0e70000-0000-4000-8000-000000000003"), "cover-03.png", "image/png", "network-nodes"),
        new(new Guid("c0e70000-0000-4000-8000-000000000004"), "cover-04.png", "image/png", "diagonal-beams"),
        new(new Guid("c0e70000-0000-4000-8000-000000000005"), "cover-05.png", "image/png", "concentric-rings"),
        new(new Guid("c0e70000-0000-4000-8000-000000000006"), "cover-06.png", "image/png", "dot-matrix"),
        new(new Guid("c0e70000-0000-4000-8000-000000000007"), "cover-07.png", "image/png", "layered-hills"),
        new(new Guid("c0e70000-0000-4000-8000-000000000008"), "cover-08.png", "image/png", "circuit-traces"),
        new(new Guid("c0e70000-0000-4000-8000-000000000009"), "cover-09.png", "image/png", "soft-blobs"),
        new(new Guid("c0e70000-0000-4000-8000-000000000010"), "cover-10.png", "image/png", "hex-lattice"),
    ];

    public static Entry? Find(Guid imageId) => Covers.FirstOrDefault(c => c.ImageId == imageId);

    /// <summary>
    /// Stable URL for a seed key (e.g. "org:42"): the same key always maps to the same cover, in every process
    /// and every deployment, because the hash is computed here instead of string.GetHashCode().
    /// </summary>
    public static string UrlFor(string seedKey)
    {
        unchecked
        {
            var hash = 2166136261u; // FNV-1a 32-bit
            foreach (var ch in seedKey)
            {
                hash ^= ch;
                hash *= 16777619u;
            }
            return $"/api/v1/images/{Covers[(int)(hash % (uint)Covers.Count)].ImageId}";
        }
    }
}
