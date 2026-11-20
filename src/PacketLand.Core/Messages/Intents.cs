using PacketLand.Core.Model;

namespace PacketLand.Core.Messages;

public static class IntentActions{
    public const string StartRequest = "START_REQUEST";
    public const string Retry = "RETRY";
    public const string SetLinkCost = "SET_LINK_COST";
    public const string SetLinkState = "SET_LINK_STATE";
    public const string AddRule = "ADD_RULE";
    public const string RemoveRule = "REMOVE_RULE";
    public const string SetDefault = "SET_DEFAULT";
    public const string FlushCache = "FLUSH_CACHE";
    public const string SetValidation = "SET_VALIDATION";
    public const string SynFlood = "SYN_FLOOD";
    public const string PoisonDns = "POISON_DNS";
    public const string DegradeLink = "DEGRADE_LINK";
}

public sealed record StartRequestIntent(string Domain);
public sealed record SetLinkCostIntent(string LinkId, int Cost);
public sealed record SetLinkStateIntent(string LinkId, bool Up);
public sealed record AddRuleIntent(FirewallRuleDto Rule);
public sealed record RemoveRuleIntent(int RuleId);
public sealed record SetDefaultIntent(FirewallAction Action);
public sealed record SetValidationIntent(bool Enabled);
public sealed record SynFloodIntent(int Rate, bool Spoof);
public sealed record PoisonDnsIntent(string Domain, string FakeIp);
public sealed record DegradeLinkIntent(string LinkId);

public static class IntentPermissions{
    private static readonly Dictionary<RoleType, string[]> Allowed = new(){
        [RoleType.Client] = [IntentActions.StartRequest, IntentActions.Retry],
        [RoleType.Router] = [IntentActions.SetLinkCost, IntentActions.SetLinkState],
        [RoleType.Firewall] = [IntentActions.AddRule, IntentActions.RemoveRule, IntentActions.SetDefault],
        [RoleType.Dns] = [IntentActions.FlushCache, IntentActions.SetValidation],
        [RoleType.Attacker] = [IntentActions.SynFlood, IntentActions.PoisonDns, IntentActions.DegradeLink],
    };

    public static bool IsAllowed(RoleType role, string action) =>
        Allowed.TryGetValue(role, out var actions) && actions.Contains(action);
}