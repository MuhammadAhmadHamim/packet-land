using PacketLand.Core.Model;
using PacketLand.Server.Sim;
using Xunit;

namespace PacketLand.Server.Tests;

public class TopologyTests{
    internal static Topology LoadDefault() =>
        Topology.Load(Path.Combine(AppContext.BaseDirectory, "Config", "topology.json"));

    [Fact]
    public void Default_topology_loads(){
        var topo = LoadDefault();

        Assert.Equal(8, topo.Nodes.Count);
        Assert.Equal(8, topo.Links.Count());
    }

    [Fact]
    public void LinksFrom_R1_lists_five_links_in_id_order(){
        var topo = LoadDefault();

        Assert.Equal(new[] { "L1", "L2", "L3", "L7", "L8" },
                     topo.LinksFrom(NodeIds.R1).Select(l => l.Id));
    }

    [Fact]
    public void Other_returns_the_far_end(){
        Assert.True(LoadDefault().TryGetLink(LinkIds.L2, out var link));

        Assert.Equal(NodeIds.R2, link.Other(NodeIds.R1));
        Assert.Equal(NodeIds.R1, link.Other(NodeIds.R2));
    }

    [Fact]
    public void Unknown_endpoint_is_rejected(){
        const string bad = """{ "nodes":[{"id":"A"}], "links":[{"id":"L1","a":"A","b":"Z","cost":1,"latencyTicks":1}] }""";

        Assert.Throws<InvalidDataException>(() => Topology.Parse(bad));
    }
}