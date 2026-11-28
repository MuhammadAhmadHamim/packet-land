namespace PacketLand.Server.Sim;

public sealed class Link{
    private readonly int _baseLatencyTicks;

    public Link(string id, string a, string b, int cost, int latencyTicks){
        Id = id;
        A = a;
        B = b;
        Cost = cost;
        _baseLatencyTicks = latencyTicks;
        LatencyTicks = latencyTicks;
    }

    public string Id { get; }
    public string A { get; }
    public string B { get; }
    public int Cost { get; set; }
    public bool IsUp { get; set; } = true;
    public int LatencyTicks { get; private set; }
    public int LossPercent { get; private set; }

    public double ProgressPerTick => 1.0 / Math.Max(1, LatencyTicks);

    public string Other(string node) =>
        node == A ? B :
        node == B ? A :
        throw new ArgumentException($"Node {node} is not on link {Id}.", nameof(node));

    public void Degrade(int extraLatencyTicks, int lossPercent){
        LatencyTicks = _baseLatencyTicks + extraLatencyTicks;
        LossPercent = Math.Clamp(lossPercent, 0, 100);
    }

    public void ResetImpairment(){
        LatencyTicks = _baseLatencyTicks;
        LossPercent = 0;
    }
}