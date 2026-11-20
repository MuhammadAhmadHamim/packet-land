using PacketLand.Core.Net;
using Xunit;

namespace PacketLand.Core.Tests;

public class FramingTests{
    [Fact]
    public async Task Roundtrip_preserves_payload(){
        var ms = new MemoryStream();
        await Framing.WriteAsync(ms, new byte[] { 1, 2, 3 });
        ms.Position = 0;

        var read = await Framing.ReadAsync(ms);

        Assert.Equal(new byte[] { 1, 2, 3 }, read);
    }

    [Fact]
    public async Task Oversize_frame_is_rejected(){
        var ms = new MemoryStream(new byte[] { 0, 1, 0, 1 });
        await Assert.ThrowsAsync<InvalidDataException>(() => Framing.ReadAsync(ms));
    }

    [Fact]
    public async Task Clean_disconnect_returns_null(){
        var read = await Framing.ReadAsync(new MemoryStream());
        Assert.Null(read);
    }

    [Fact]
    public async Task Handles_data_arriving_one_byte_at_a_time(){
        var source = new MemoryStream();
        await Framing.WriteAsync(source, new byte[] { 9, 8, 7, 6 });

        var read = await Framing.ReadAsync(new OneByteStream(source.ToArray()));

        Assert.Equal(new byte[] { 9, 8, 7, 6 }, read);
    }

    private sealed class OneByteStream(byte[] data) : MemoryStream(data){
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) =>
            base.ReadAsync(buffer[..Math.Min(1, buffer.Length)], ct);
    }
}