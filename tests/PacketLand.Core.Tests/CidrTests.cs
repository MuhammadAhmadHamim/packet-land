using PacketLand.Core.Model;
using Xunit;

namespace PacketLand.Core.Tests;

public class CidrTests{
    [Theory]
    [InlineData("10.1.1.0/24", "10.1.1.5", true)]
    [InlineData("10.1.1.0/24", "10.1.10.5", false)]
    [InlineData("0.0.0.0/0", "8.8.8.8", true)]
    [InlineData("203.0.113.10/32", "203.0.113.10", true)]
    [InlineData("203.0.113.10/32", "203.0.113.11", false)]
    public void Contains_is_numeric(string cidr, string ip, bool expected) =>
        Assert.Equal(expected, Ipv4Cidr.Parse(cidr).Contains(Ipv4Cidr.ParseIp(ip)));

    [Fact]
    public void Shorthand_addresses_are_rejected() =>
        Assert.Throws<FormatException>(() => Ipv4Cidr.ParseIp("10.1"));

    [Fact]
    public void Host_bits_are_masked_on_parse() =>
        Assert.Equal("10.1.1.0/24", Ipv4Cidr.Parse("10.1.1.77/24").ToString());
}