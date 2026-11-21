// Date: 2026-11-21
// Purpose: Strict URI validator rejecting dangerous schemes (javascript:, file:, data:) and SSRF private/loopback/metadata IP ranges (STRIDE THR-T-03).

using System.Net;
using System.Net.Sockets;

namespace TeamUp.Application.Common.Validation;

public static class SafeUrlValidator
{
    private static readonly HashSet<string> ForbiddenHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "localhost.localdomain",
        "ip6-localhost",
        "ip6-loopback",
        "metadata.google.internal",
        "metadata",
        "instance-data"
    };

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

        var host = uri.IdnHost;
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
        if (trimmed.StartsWith('/') && !trimmed.StartsWith("//", StringComparison.Ordinal))
        {
            return !trimmed.Contains("..", StringComparison.Ordinal)
                && !trimmed.Any(char.IsControl)
                && Uri.TryCreate(trimmed, UriKind.Relative, out _);
        }

        return IsSafeExternalHttpUrl(trimmed);
    }

    private static bool IsRestrictedIpAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        if (IPAddress.IsLoopback(address))
            return true;

        if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) || address.Equals(IPAddress.None))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            // 0.0.0.0/8 (this network)
            if (bytes[0] == 0) return true;
            // 10.0.0.0/8 (RFC 1918)
            if (bytes[0] == 10) return true;
            // 100.64.0.0/10 (Carrier-Grade NAT)
            if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127) return true;
            // 127.0.0.0/8 (loopback)
            if (bytes[0] == 127) return true;
            // 169.254.0.0/16 (link-local & cloud metadata 169.254.169.254)
            if (bytes[0] == 169 && bytes[1] == 254) return true;
            // 172.16.0.0/12 (RFC 1918)
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
            // 192.168.0.0/16 (RFC 1918)
            if (bytes[0] == 192 && bytes[1] == 168) return true;
            // 224.0.0.0/4 (multicast) and 240.0.0.0/4 (reserved/broadcast)
            if (bytes[0] >= 224) return true;
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast || address.IsIPv6Teredo)
                return true;

            var bytes = address.GetAddressBytes();
            // fc00::/7 (Unique Local Address)
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;
        }

        return false;
    }
}
