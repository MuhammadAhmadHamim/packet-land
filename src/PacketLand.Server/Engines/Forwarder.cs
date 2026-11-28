using PacketLand.Core.Model;

namespace PacketLand.Server.Engines;

public readonly record struct ForwardResult(bool Dropped, DropReason? Reason, string? NextHop, Packet? Packet){
    public static ForwardResult Drop(DropReason reason) => new(true, reason, null, null);

    public static ForwardResult To(string nextHop, Packet packet) => new(false, null, nextHop, packet);
}

public static class Forwarder{
    public static ForwardResult Forward(RoutingTable table, Packet packet){
        var next = packet with { Ttl = packet.Ttl - 1 };
        if (next.Ttl <= 0)
            return ForwardResult.Drop(DropReason.TtlExpired);

        var route = table.Lookup(next.DstIp);
        return route is null
            ? ForwardResult.Drop(DropReason.NoRoute)
            : ForwardResult.To(route.NextHopNodeId, next);
    }
}