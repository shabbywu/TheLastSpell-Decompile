using Rewired.Dev;

namespace RewiredConsts;

public static class Player
{
	[PlayerIdFieldInfo(friendlyName = "System")]
	public const int System = 9999999;

	[PlayerIdFieldInfo(friendlyName = "Debug")]
	public const int Debug = 0;

	[PlayerIdFieldInfo(friendlyName = "Player")]
	public const int Player0 = 1;
}
