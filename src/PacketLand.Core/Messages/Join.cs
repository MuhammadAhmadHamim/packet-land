using PacketLand.Core.Model;

namespace PacketLand.Core.Messages;

public sealed record JoinMessage(string Name, RoleType Role);

public sealed record WelcomeMessage(string PlayerId, RoleType Role, int TickMs, int RoundSeconds);