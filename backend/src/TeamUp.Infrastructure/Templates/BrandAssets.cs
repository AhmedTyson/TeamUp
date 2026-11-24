// Date: 2026-11-24
// Purpose: Embedded official TeamUp logo assets (vector SVG for PDF/HTML reports and retina PNG/CID/DataURI for email templates).

using System.Reflection;

namespace TeamUp.Infrastructure.Templates;

internal static class BrandAssets
{
    public const string EmailLogoContentId = "teamup-logo";
    public const string EmailLogoRelativePath = "/images/brand/teamup-logo-email.png";

    private const string AssetsResourcePrefix = "TeamUp.Infrastructure.Templates.Assets.";
    private static readonly Assembly Assembly = typeof(BrandAssets).Assembly;

    private static readonly Lazy<string> LazyLogoVectorSvg = new(() => LoadTextResource("teamup-logo-vector.svg"));
    private static readonly Lazy<byte[]> LazyEmailLogoPngBytes = new(() => LoadBinaryResource("teamup-logo-email.png"));
    private static readonly Lazy<string> LazyEmailLogoDataUri = new(() =>
        $"data:image/png;base64,{Convert.ToBase64String(EmailLogoPngBytes)}");

    public static string LogoVectorSvg => LazyLogoVectorSvg.Value;
    public static byte[] EmailLogoPngBytes => LazyEmailLogoPngBytes.Value;
    public static string EmailLogoDataUri => LazyEmailLogoDataUri.Value;

    public static string BuildPublicEmailLogoUrl(string? frontendBaseUrl)
    {
        var baseUrl = string.IsNullOrWhiteSpace(frontendBaseUrl)
            ? "https://teamup-ieee.vercel.app"
            : frontendBaseUrl.Trim().TrimEnd('/');
        return $"{baseUrl}{EmailLogoRelativePath}";
    }

    private static string LoadTextResource(string fileName)
    {
        var resourceName = $"{AssetsResourcePrefix}{fileName}";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded brand asset '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static byte[] LoadBinaryResource(string fileName)
    {
        var resourceName = $"{AssetsResourcePrefix}{fileName}";
        using var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded brand asset '{resourceName}' was not found.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
