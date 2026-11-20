using PacketLand.Core.Model;

namespace PacketLand.Core.Messages;

public enum FirewallAction { Allow, Deny, RateLimit }

public sealed record FirewallRuleDto(
    int Id,
    Protocol? Protocol,
    string? Source,
    string? Destination,
    int? DestinationPort,
    bool SynOnly,
    FirewallAction Action,
    int LimitPerSecond);