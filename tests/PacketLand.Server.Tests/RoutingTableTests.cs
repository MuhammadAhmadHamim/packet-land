using PacketLand.Core.Model;
using PacketLand.Server.Engines;
using Xunit;

namespace PacketLand.Server.Tests;

public class RoutingTableTests{
    private static Route R(string cidr, string hop, int metric) =>
        new(Ipv4Cidr.Parse(cidr), hop, metric);

    private static uint Ip(string text) => Ipv4Cidr.ParseIp(text);

    [Fact]
    public void Prefix_match_is_numeric_not_textual(){
        var table = new RoutingTable([R("10.1.1.0/24", "R2", 1)]);

        Assert.Null(table.Lookup(Ip("10.1.10.5")));
        Assert.NotNull(table.Lookup(Ip("10.1.1.5")));
    }

    [Fact]
    public void Longest_prefix_wins_even_with_worse_metric(){
        var table = new RoutingTable([R("10.1.0.0/16", "R2", 1), R("10.1.1.0/24", "R3", 9)]);

        Assert.Equal("R3", table.Lookup(Ip("10.1.1.5"))!.NextHopNodeId);
        Assert.Equal("R2", table.Lookup(Ip("10.1.2.5"))!.NextHopNodeId);
    }

    [Fact]
    public void Lower_metric_wins_when_prefixes_are_equal(){
        var table = new RoutingTable([R("10.1.1.0/24", "R2", 5), R("10.1.1.0/24", "R3", 2)]);

        Assert.Equal("R3", table.Lookup(Ip("10.1.1.5"))!.NextHopNodeId);
    }

    [Fact]
    public void Equal_routes_break_ties_by_name(){
        var table = new RoutingTable([R("10.1.1.0/24", "R3", 2), R("10.1.1.0/24", "R2", 2)]);

        Assert.Equal("R2", table.Lookup(Ip("10.1.1.5"))!.NextHopNodeId);
    }

    [Fact]
    public void Default_route_is_used_only_as_last_resort(){
        var table = new RoutingTable([R("0.0.0.0/0", "R2", 1), R("203.0.113.0/24", "R3", 1)]);

        Assert.Equal("R3", table.Lookup(Ip("203.0.113.10"))!.NextHopNodeId);
        Assert.Equal("R2", table.Lookup(Ip("8.8.8.8"))!.NextHopNodeId);
    }
}