// Date: 2026-11-21
// Purpose: Strict URI validator rejecting dangerous schemes (javascript:, file:, data:) and SSRF private/loopback/metadata IP ranges (STRIDE THR-T-03).

using System.Buffers;
using System.Collections.Frozen;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace TeamUp.Application.Common.Validation;

public static partial class SafeUrlValidator
{
    #region Patterns & Restrictions

    [GeneratedRegex(
        @"^/api/v1/images/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$",
        RegexOptions.CultureInvariant)]
    private static partial Regex StoredImagePathRegex();

    private static readonly FrozenSet<string> ForbiddenHosts = new[]
    {
        "localhost",
        "localhost.localdomain",
        "ip6-localhost",
        "ip6-loopback",
        "metadata.google.internal",
        "metadata",
        "instance-data"
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly SearchValues<char> DisallowedRelativePathChars =
        SearchValues.Create(BuildDisallowedRelativePathChars());

    private static char[] BuildDisallowedRelativePathChars()
    {
        List<char> chars = [with(capacity: 66), '\\'];
        for (var c = '\u0000'; c <= '\u001F'; c++) chars.Add(c);
        for (var c = '\u007F'; c <= '\u009F'; c++) chars.Add(c);
        return [.. chars];
    }

    #endregion

    #region Public Validators

    public static bool IsSafeExternalHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        if (!string.IsNullOrEmpty(uri.UserInfo))
            return false;

        var host = uri.IdnHost.TrimEnd('.');
        if (string.IsNullOrWhiteSpace(host))
            return false;

        if (ForbiddenHosts.Contains(host) || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
            return false;

        if (IPAddress.TryParse(host, out var ip) && IsRestrictedIpAddress(ip))
            return false;

        return true;
    }

    public static bool IsSafeAssetOrExternalUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        if (trimmed.StartsWith('/'))
        {
            if (trimmed.StartsWith("//", StringComparison.Ordinal)
                || trimmed.Contains("..", StringComparison.Ordinal)
                || trimmed.AsSpan().ContainsAny(DisallowedRelativePathChars))
            {
                return false;
            }

            if (StoredImagePathRegex().IsMatch(trimmed))
                return true;

            var isAllowedStaticFolder =
                trimmed.StartsWith("/images/", StringComparison.Ordinal)
                || trimmed.StartsWith("/logos/", StringComparison.Ordinal)
                || trimmed.StartsWith("/badges/", StringComparison.Ordinal);

            return isAllowedStaticFolder && Uri.TryCreate(trimmed, UriKind.Relative, out _);
        }

        return IsSafeExternalHttpUrl(trimmed);
    }

    #endregion

    #region IP CIDR Guards

    private static bool IsRestrictedIpAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address)
            || address.Equals(IPAddress.Any)
            || address.Equals(IPAddress.IPv6Any)
            || address.Equals(IPAddress.None))
        {
            return true;
        }

        Span<byte> bytes = stackalloc byte[16];
        if (!address.TryWriteBytes(bytes, out var bytesWritten))
            return false;

        if (address.AddressFamily == AddressFamily.InterNetwork && bytesWritten >= 4)
            return IsRestrictedIPv4(bytes[..4]);

        if (address.AddressFamily == AddressFamily.InterNetworkV6 && bytesWritten == 16)
            return IsRestrictedIPv6(address, bytes);

        return false;
    }

    private static bool IsRestrictedIPv4(ReadOnlySpan<byte> bytes) =>
        bytes[0] switch
        {
            0 or 10 or 127 or >= 224 => true,
            100 => bytes[1] is >= 64 and <= 127,
            169 => bytes[1] == 254,
            172 => bytes[1] is >= 16 and <= 31,
            192 => bytes[1] == 168,
            _ => false
        };

    private static bool IsRestrictedIPv6(IPAddress address, ReadOnlySpan<byte> bytes)
    {
        if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast || address.IsIPv6Teredo)
            return true;

        // fc00::/7 (Unique Local Address)
        return (bytes[0] & 0xFE) == 0xFC;
    }

    #endregion
}
