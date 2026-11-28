using PacketLand.Core.Model;
using PacketLand.Server.Sim;

namespace PacketLand.Server.Engines;

public static class RoutingEngine{
    public static Dictionary<string, (int Dist, string FirstHop)> ShortestPaths(Topology topo, string source){
        var dist = new Dictionary<string, int> { [source] = 0 };
        var first = new Dictionary<string, string>();
        var queue = new PriorityQueue<string, int>();
        queue.Enqueue(source, 0);

        while (queue.TryDequeue(out var u, out var d)){
            if (d > dist[u]) continue;

            foreach (var link in topo.LinksFrom(u)){
                if (!link.IsUp) continue;

                var v = link.Other(u);
                if (v == source) continue;

                int nd = d + link.Cost;
                string hop = u == source ? v : first[u];

                bool better = !dist.TryGetValue(v, out var old) || nd < old;
                bool tieWin = !better && nd == old && string.CompareOrdinal(hop, first[v]) < 0;

                if (better || tieWin){
                    dist[v] = nd;
                    first[v] = hop;
                    queue.Enqueue(v, nd);
                }
            }
        }

        return dist.Where(kv => kv.Key != source)
                   .ToDictionary(kv => kv.Key, kv => (kv.Value, first[kv.Key]));
    }

    public static RoutingTable BuildTable(Topology topo, string source){
        var paths = ShortestPaths(topo, source);
        var routes = new List<Route>();

        foreach (var (nodeId, prefix) in topo.Prefixes){
            if (nodeId != source && paths.TryGetValue(nodeId, out var path))
                routes.Add(new Route(prefix, path.FirstHop, path.Dist));
        }

        return new RoutingTable(routes);
    }

    public static Dictionary<string, RoutingTable> BuildAll(Topology topo) =>
        topo.Nodes.ToDictionary(n => n.Id, n => BuildTable(topo, n.Id));
}