using PacketLand.Core.Model;
using PacketLand.Server.Engines;
using PacketLand.Server.Sim;
using Xunit;

namespace PacketLand.Server.Tests;

public class RoutingEngineTests{
    private static uint Web => Ipv4Cidr.ParseIp(Addresses.WebIp);

    [Fact]
    public void R1_reaches_web_via_R2_when_everything_is_up(){
        var table = RoutingEngine.BuildTable(TopologyTests.LoadDefault(), NodeIds.R1);

        var route = table.Lookup(Web);

        Assert.Equal(NodeIds.R2, route!.NextHopNodeId);
        Assert.Equal(3, route.Metric);
    }

    [Fact]
    public void Dijkstra_reroutes_when_link_is_down(){
        var topo = TopologyTests.LoadDefault();
        Assert.True(topo.TryGetLink(LinkIds.L2, out var l2));
        l2.IsUp = false;

        var route = RoutingEngine.BuildTable(topo, NodeIds.R1).Lookup(Web);

        Assert.Equal(NodeIds.R3, route!.NextHopNodeId);
        Assert.Equal(5, route.Metric);
    }

    [Fact]
    public void Raising_a_link_cost_flips_the_path(){
        var topo = TopologyTests.LoadDefault();
        Assert.True(topo.TryGetLink(LinkIds.L2, out var l2));
        l2.Cost = 10;

        var route = RoutingEngine.BuildTable(topo, NodeIds.R1).Lookup(Web);

        Assert.Equal(NodeIds.R3, route!.NextHopNodeId);
    }

    [Fact]
    public void Unreachable_node_has_no_route(){
        var topo = TopologyTests.LoadDefault();
        Assert.True(topo.TryGetLink(LinkIds.L7, out var l7));
        l7.IsUp = false;

        var table = RoutingEngine.BuildTable(topo, NodeIds.R1);

        Assert.Null(table.Lookup(Ipv4Cidr.ParseIp(Addresses.DnsIp)));
    }

    [Fact]
    public void Equal_cost_ties_are_deterministic(){
        const string diamond = """
        { "nodes":[{"id":"A"},{"id":"B"},{"id":"C"},{"id":"D","prefix":"10.9.9.0/24"}],
          "links":[
            {"id":"L1","a":"A","b":"B","cost":1,"latencyTicks":1},
            {"id":"L2","a":"A","b":"C","cost":1,"latencyTicks":1},
            {"id":"L3","a":"B","b":"D","cost":1,"latencyTicks":1},
            {"id":"L4","a":"C","b":"D","cost":1,"latencyTicks":1}] }
        """;

        var hops = Enumerable.Range(0, 20)
            .Select(_ => RoutingEngine.BuildTable(Topology.Parse(diamond), "A")
                                      .Lookup(Ipv4Cidr.ParseIp("10.9.9.1"))!.NextHopNodeId)
            .Distinct()
            .ToList();

        Assert.Equal(new[] { "B" }, hops);
    }
}