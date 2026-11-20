using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;

namespace PacketLand.Core.Model;

public readonly record struct Ipv4Cidr(uint Network, int PrefixLength){
    public uint Mask => PrefixLength == 0 ? 0u : uint.MaxValue << (32 - PrefixLength);

    public bool Contains(uint ip) => (ip & Mask) == Network;

    public static uint ParseIp(string text){
        if (text.Count(c => c == '.') != 3)
            throw new FormatException("Use dotted-quad form, e.g. 10.0.0.2.");

        var addr = IPAddress.Parse(text);
        if (addr.AddressFamily != AddressFamily.InterNetwork)
            throw new FormatException("IPv4 only.");

        return BinaryPrimitives.ReadUInt32BigEndian(addr.GetAddressBytes());
    }

    public static string FormatIp(uint ip) =>
        $"{ip >> 24}.{(ip >> 16) & 255}.{(ip >> 8) & 255}.{ip & 255}";

    public static Ipv4Cidr Parse(string text){
        var parts = text.Split('/');
        int len = parts.Length == 2 ? int.Parse(parts[1]) : 32;
        if (len is < 0 or > 32) throw new FormatException("Bad prefix length.");

        uint mask = len == 0 ? 0u : uint.MaxValue << (32 - len);
        return new Ipv4Cidr(ParseIp(parts[0]) & mask, len);
    }

    public override string ToString() => $"{FormatIp(Network)}/{PrefixLength}";
}