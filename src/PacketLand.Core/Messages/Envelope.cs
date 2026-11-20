using System.Text.Json;
using PacketLand.Core.Net;

namespace PacketLand.Core.Messages;

public static class MessageTypes{
    public const string Join = "JOIN";
    public const string Welcome = "WELCOME";
    public const string Intent = "INTENT";
    public const string Snapshot = "SNAPSHOT";
    public const string Event = "EVENT";
    public const string Alert = "ALERT";
    public const string Error = "ERROR";
    public const string RoundEnd = "ROUND_END";
    public const string Ping = "PING";
    public const string Pong = "PONG";
}

public sealed record Envelope(int V, string Type, long Seq, string? Action, JsonElement? Payload){
    public const int CurrentVersion = 1;

    public static Envelope Create<T>(string type, long seq, T payload, string? action = null) =>
        new(CurrentVersion, type, seq, action,
            JsonSerializer.SerializeToElement(payload, JsonOptions.Default));

    public static Envelope Empty(string type, long seq, string? action = null) =>
        new(CurrentVersion, type, seq, action, null);

    public T ReadPayload<T>() =>
        Payload is { } p
            ? p.Deserialize<T>(JsonOptions.Default)
                ?? throw new InvalidDataException($"Empty {Type} payload.")
            : throw new InvalidDataException($"{Type} has no payload.");

    public byte[] ToBytes() => JsonSerializer.SerializeToUtf8Bytes(this, JsonOptions.Default);

    public static Envelope FromBytes(byte[] bytes) =>
        JsonSerializer.Deserialize<Envelope>(bytes, JsonOptions.Default)
        ?? throw new InvalidDataException("Empty envelope.");
}