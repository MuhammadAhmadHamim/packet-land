using PacketLand.Core.Model;

namespace PacketLand.Core.Messages;

public enum RoundPhase { Waiting, Countdown, Running, Ended }
public enum NodeStatus { Ok, Stressed, Down }
public enum RequestStatus { Idle, ResolvingDns, Handshaking, AwaitingResponse, Succeeded, Failed }

public sealed record RoundState(RoundPhase Phase, int SecondsLeft);
public sealed record ScoreView(int Defenders, int Attacker);
public sealed record NodeView(string Id, NodeStatus Status);
public sealed record LinkView(string Id, string A, string B, int Cost, bool Up, int LatencyMs, int LossPercent);
public sealed record PacketView(long Id, string LinkId, string From, string To, double Progress,
                                Protocol Protocol, bool Suspicious, bool Dropped);

public sealed record ClientPanel(RequestStatus Status, int Attempts, int? LatencyMs);
public sealed record RouteView(string Destination, string NextHop, int Metric);
public sealed record RouterPanel(IReadOnlyList<RouteView> Routes);
public sealed record FirewallStats(int Allowed, int Denied, int SynPerSecond);
public sealed record FirewallPanel(IReadOnlyList<FirewallRuleDto> Rules, FirewallAction DefaultAction, FirewallStats Stats);
public sealed record DnsCacheEntry(string Domain, string Ip, int TtlSeconds, bool Suspicious);
public sealed record DnsPanel(IReadOnlyList<DnsCacheEntry> Cache, bool ValidationEnabled, int QueriesServed);
public sealed record CooldownView(string Action, int SecondsLeft);
public sealed record AttackerPanel(int DetectionRisk, IReadOnlyList<CooldownView> Cooldowns, int ServerLoadPercent);

public sealed record SnapshotMessage(
    long Tick,
    RoundState Round,
    ScoreView Score,
    IReadOnlyList<NodeView> Nodes,
    IReadOnlyList<LinkView> Links,
    IReadOnlyList<PacketView> Packets,
    ClientPanel? Client,
    RouterPanel? Router,
    FirewallPanel? Firewall,
    DnsPanel? Dns,
    AttackerPanel? Attacker);