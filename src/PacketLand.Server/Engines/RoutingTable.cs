using PacketLand.Core.Model;

namespace PacketLand.Server.Engines;

public sealed record Route(Ipv4Cidr Destination, string NextHopNodeId, int Metric);

public sealed class RoutingTable{
    private readonly List<Route> _routes;

    public RoutingTable(IEnumerable<Route> routes) => _routes = routes.ToList();

    public IReadOnlyList<Route> Routes => _routes;

    public Route? Lookup(uint destinationIp) =>
        _routes.Where(r => r.Destination.Contains(destinationIp))
               .OrderByDescending(r => r.Destination.PrefixLength)
               .ThenBy(r => r.Metric)
               .ThenBy(r => r.NextHopNodeId, StringComparer.Ordinal)
               .FirstOrDefault();
}