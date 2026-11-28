using PacketLand.Core.Model;
using PacketLand.Server.Engines;
using Xunit;

namespace PacketLand.Server.Tests;

public class ForwarderTests{
    private static readonly uint Web = Ipv4Cidr.ParseIp(Addresses.WebIp);

    private static Packet Pkt(int ttl) =>
        new(1, 1, Protocol.Tcp, Ipv4Cidr.ParseIp(Addresses.ClientIp), Web, 1000, 443,
            TcpFlags.Syn, ttl, PacketTruth.Legit);

    private static RoutingTable Table(string nextHop) =>
        new([new Route(Ipv4Cidr.Parse("203.0.113.0/24"), nextHop, 1)]);

    [Fact]
    public void Ttl_decrements_at_each_hop(){
        var result = Forwarder.Forward(Table("R2"), Pkt(16));

        Assert.False(result.Dropped);
        Assert.Equal("R2", result.NextHop);
        Assert.Equal(15, result.Packet!.Ttl);
    }

    [Fact]
    public void Ttl_expiry_drops_the_packet(){
        var result = Forwarder.Forward(Table("R2"), Pkt(1));

        Assert.True(result.Dropped);
        Assert.Equal(DropReason.TtlExpired, result.Reason);
    }

    [Fact]
    public void Missing_route_drops_the_packet(){
        var result = Forwarder.Forward(new RoutingTable([]), Pkt(16));

        Assert.Equal(DropReason.NoRoute, result.Reason);
    }

    [Fact]
    public void Routing_loop_ends_in_ttl_expiry(){
        var tableA = Table("B");
        var tableB = Table("A");
        var packet = Pkt(5);
        var at = "A";
        var hops = 0;

        while (true){
            var result = Forwarder.Forward(at == "A" ? tableA : tableB, packet);
            if (result.Dropped){
                Assert.Equal(DropReason.TtlExpired, result.Reason);
                break;
            }
            packet = result.Packet!;
            at = result.NextHop!;
            hops++;
        }

        Assert.Equal(4, hops);
    }
}