using Steamworks;

namespace TheLastStand.DRM.Achievements;

public class SteamAchievementHandler : AAchievementHandler
{
	public override void RefreshAchievements()
	{
		SteamUserStats.StoreStats();
	}

	public override void SetAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		SteamUserStats.SetStat(stat.SteamId, value);
		if (refreshAchievements)
		{
			RefreshAchievements();
		}
	}

	public override void IncreaseAchievementProgression(Stat stat, int value, bool refreshAchievements = true)
	{
		int num = default(int);
		SteamUserStats.GetStat(stat.SteamId, ref num);
		SteamUserStats.SetStat(stat.SteamId, num + value);
		if (refreshAchievements)
		{
			RefreshAchievements();
		}
	}

	public override void UnlockAchievement(Achievement achievement, bool refreshIfAchieved = true)
	{
		bool flag = default(bool);
		SteamUserStats.GetAchievement(achievement.SteamId, ref flag);
		if (!flag)
		{
			SteamUserStats.SetAchievement(achievement.SteamId);
			if (refreshIfAchieved)
			{
				RefreshAchievements();
			}
		}
	}

	public override void UnlockAchievement(string achievementId, bool refreshIfAchieved = true)
	{
		bool flag = default(bool);
		SteamUserStats.GetAchievement(achievementId, ref flag);
		if (!flag)
		{
			SteamUserStats.SetAchievement(achievementId);
			if (refreshIfAchieved)
			{
				RefreshAchievements();
			}
		}
	}

	public override bool IsAchievementUnlocked(Achievement achievement)
	{
		bool result = default(bool);
		SteamUserStats.GetAchievement(achievement.SteamId, ref result);
		return result;
	}

	public override void ClearAchievements()
	{
		SteamUserStats.ResetAllStats(true);
	}
}
