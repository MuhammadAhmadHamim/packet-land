namespace PacketLand.Core.Messages;

public enum EventSeverity { Info, Warning, Danger }

public sealed record EventMessage(long Tick, EventSeverity Severity, string Text, long? PacketId);
public sealed record AlertMessage(long Tick, string Kind, string Text);
public sealed record ErrorMessage(string Code, string Text, long? RefSeq);
public sealed record RoundEndMessage(bool Delivered, string Reason, ScoreView Final, IReadOnlyList<string> Trace);

public static class AlertKinds{
    public const string SynFlood = "SYN_FLOOD";
    public const string DnsAnomaly = "DNS_ANOMALY";
    public const string LinkDegraded = "LINK_DEGRADED";
}

public static class ErrorCodes{
    public const string NotJoined = "NOT_JOINED";
    public const string RoleTaken = "ROLE_TAKEN";
    public const string NotYourAction = "NOT_YOUR_ACTION";
    public const string UnknownAction = "UNKNOWN_ACTION";
    public const string BadPayload = "BAD_PAYLOAD";
    public const string OutOfRange = "OUT_OF_RANGE";
    public const string OnCooldown = "ON_COOLDOWN";
    public const string RoundNotRunning = "ROUND_NOT_RUNNING";
}