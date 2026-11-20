using System.Buffers.Binary;

namespace PacketLand.Core.Net;

public static class Framing{
    public const int MaxFrameBytes = 64 * 1024;

    public static async Task WriteAsync(Stream stream, ReadOnlyMemory<byte> payload, CancellationToken ct = default){
        if (payload.Length is 0 or > MaxFrameBytes)
            throw new ArgumentException("Payload size out of range.", nameof(payload));

        var header = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
        await stream.WriteAsync(header, ct);
        await stream.WriteAsync(payload, ct);
        await stream.FlushAsync(ct);
    }

    public static async Task<byte[]?> ReadAsync(Stream stream, CancellationToken ct = default){
        var header = new byte[4];
        if (!await ReadExactAsync(stream, header, ct)) return null;

        int length = BinaryPrimitives.ReadInt32BigEndian(header);
        if (length is <= 0 or > MaxFrameBytes)
            throw new InvalidDataException($"Bad frame length: {length}.");

        var payload = new byte[length];
        return await ReadExactAsync(stream, payload, ct) ? payload : null;
    }

    private static async Task<bool> ReadExactAsync(Stream stream, byte[] buffer, CancellationToken ct){
        int offset = 0;
        while (offset < buffer.Length){
            int n = await stream.ReadAsync(buffer.AsMemory(offset), ct);
            if (n == 0) return false;
            offset += n;
        }
        return true;
    }
}