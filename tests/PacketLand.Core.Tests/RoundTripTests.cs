using System.Text.Json;
using PacketLand.Core.Messages;
using PacketLand.Core.Model;
using PacketLand.Core.Net;
using Xunit;

namespace PacketLand.Core.Tests;

public class RoundTripTests{
    [Fact]
    public void Intent_round_trips_through_envelope(){
        var rule = new FirewallRuleDto(0, Protocol.Tcp, null, "203.0.113.0/24", 443, true,
                                       FirewallAction.RateLimit, 20);
        var sent = Envelope.Create(MessageTypes.Intent, 7, new AddRuleIntent(rule), IntentActions.AddRule);

        var received = Envelope.FromBytes(sent.ToBytes());
        var intent = received.ReadPayload<AddRuleIntent>();

        Assert.Equal(7, received.Seq);
        Assert.Equal(IntentActions.AddRule, received.Action);
        Assert.Equal(rule, intent.Rule);
    }

    [Fact]
    public void Snapshot_round_trips(){
        var snap = new SnapshotMessage(
            Tick: 42,
            Round: new RoundState(RoundPhase.Running, 90),
            Score: new ScoreView(0, 0),
            Nodes: [new NodeView(NodeIds.Web, NodeStatus.Stressed)],
            Links: [new LinkView(LinkIds.L2, NodeIds.R1, NodeIds.R2, 1, true, 20, 0)],
            Packets: [new PacketView(1, LinkIds.L2, NodeIds.R1, NodeIds.R2, 0.5, Protocol.Tcp, false, false)],
            Client: null, Router: null, Dns: null, Attacker: null,
            Firewall: new FirewallPanel([], FirewallAction.Deny, new FirewallStats(10, 2, 55)));

        var first = Envelope.Create(MessageTypes.Snapshot, 1, snap).ToBytes();
        var back = Envelope.FromBytes(first).ReadPayload<SnapshotMessage>();
        var second = Envelope.Create(MessageTypes.Snapshot, 1, back).ToBytes();

        Assert.Equal(first, second);
    }

    [Fact]
    public void Packet_truth_is_never_serialized(){
        var p = new Packet(1, 1, Protocol.Tcp, 1, 2, 1000, 443, TcpFlags.Syn, 16, PacketTruth.Attack);
        var json = JsonSerializer.Serialize(p, JsonOptions.Default);
        Assert.DoesNotContain("truth", json, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(RoleType.Client, IntentActions.AddRule, false)]
    [InlineData(RoleType.Firewall, IntentActions.AddRule, true)]
    [InlineData(RoleType.Router, IntentActions.SynFlood, false)]
    [InlineData(RoleType.Attacker, IntentActions.SynFlood, true)]
    public void Permissions_match_roles(RoleType role, string action, bool expected) =>
        Assert.Equal(expected, IntentPermissions.IsAllowed(role, action));
}