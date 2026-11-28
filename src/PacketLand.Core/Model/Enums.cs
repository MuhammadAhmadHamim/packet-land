namespace PacketLand.Core.Model;

public enum RoleType { Client, Router, Firewall, Dns, Attacker}

public enum Protocol { Tcp, Udp }

[Flags]
public enum TcpFlags { None = 0, Syn = 1, Ack = 2 } 

public enum DropReason {
    TtlExpired,
    NoRoute,
    FirewallDenied,
    RateLimited,
    LinkDown,
    LinkLoss,
    BacklogFull
}