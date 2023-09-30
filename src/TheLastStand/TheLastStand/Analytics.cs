using NaconAPI;
using Steamworks;

namespace TheLastStand;

public static class Analytics
{
	public static class AnalyticsEventTypes
	{
		public const string GAME_START = "game_start";
	}

	public static void InitializeNaconAPIHandler()
	{
		NaconAPIHandler.Init("https://nacon-os.com/v2", "g3ivky9iXU", "STEAM", GetPlatformUserId(), "9tw1Nu3f9LCttEYpXsvFnFuMTxT0p74x", "the-last-spell", "ishtar-the-last-spell", GetPlatform());
	}

	public static void SendGameStartEvent()
	{
		if (!NaconAPIHandler.IsInitialized)
		{
			InitializeNaconAPIHandler();
		}
		NaconAPIHandler.Analytics.SendEventAsync("game_start");
	}

	private static string GetPlatform()
	{
		return "PC";
	}

	private static string GetPlatformUserId()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		CSteamID steamID = SteamUser.GetSteamID();
		return ((object)(CSteamID)(ref steamID)).ToString();
	}
}
