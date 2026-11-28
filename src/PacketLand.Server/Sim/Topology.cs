using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using PacketLand.Core.Model;
using PacketLand.Core.Net;

namespace PacketLand.Server.Sim;

public sealed record NodeSpec(string Id, string? Prefix);
public sealed record LinkSpec(string Id, string A, string B, int Cost, int LatencyTicks);
public sealed record TopologyFile(List<NodeSpec> Nodes, List<LinkSpec> Links);

public sealed class Topology{
    private readonly List<NodeSpec> _nodes;
    private readonly Dictionary<string, Link> _links = new();
    private readonly Dictionary<string, List<Link>> _adjacent = new();

    public Topology(TopologyFile file){
        _nodes = file.Nodes.OrderBy(n => n.Id, StringComparer.Ordinal).ToList();
        var nodeIds = _nodes.Select(n => n.Id).ToHashSet();
        if (nodeIds.Count != _nodes.Count)
            throw new InvalidDataException("Duplicate node id.");

        foreach (var spec in file.Links){
            if (!nodeIds.Contains(spec.A) || !nodeIds.Contains(spec.B))
                throw new InvalidDataException($"Link {spec.Id} refers to an unknown node.");
            if (_links.ContainsKey(spec.Id))
                throw new InvalidDataException($"Duplicate link id {spec.Id}.");

            var link = new Link(spec.Id, spec.A, spec.B, spec.Cost, spec.LatencyTicks);
            _links[link.Id] = link;
            AddAdjacent(spec.A, link);
            AddAdjacent(spec.B, link);
        }

        foreach (var list in _adjacent.Values)
            list.Sort((x, y) => string.CompareOrdinal(x.Id, y.Id));
    }

    public IReadOnlyList<NodeSpec> Nodes => _nodes;

    public IEnumerable<Link> Links => _links.Values.OrderBy(l => l.Id, StringComparer.Ordinal);

    public IEnumerable<(string NodeId, Ipv4Cidr Prefix)> Prefixes =>
        _nodes.Where(n => n.Prefix is not null)
              .Select(n => (n.Id, Ipv4Cidr.Parse(n.Prefix!)));

    public IEnumerable<Link> LinksFrom(string nodeId) =>
        _adjacent.TryGetValue(nodeId, out var list) ? list : Enumerable.Empty<Link>();

    public bool TryGetLink(string id, [NotNullWhen(true)] out Link? link) =>
        _links.TryGetValue(id, out link);

    public static Topology Parse(string json){
        var file = JsonSerializer.Deserialize<TopologyFile>(json, JsonOptions.Default)
                   ?? throw new InvalidDataException("Empty topology file.");
        return new Topology(file);
    }

    public static Topology Load(string path) => Parse(File.ReadAllText(path));

    private void AddAdjacent(string node, Link link){
        if (!_adjacent.TryGetValue(node, out var list))
            _adjacent[node] = list = new List<Link>();
        list.Add(link);
    }
}