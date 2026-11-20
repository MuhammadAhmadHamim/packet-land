using System.Text.Json.Serialization;

namespace PacketLand.Core.Model;

public enum PacketTruth { Legit, Attack }

public sealed record Packet(
    long Id,
    long FlowId,
    Protocol Protocol,
    uint SrcIp,
    uint DstIp,
    int SrcPort,
    int DstPort,
    TcpFlags Flags,
    int Ttl,
    [property: JsonIgnore] PacketTruth Truth);