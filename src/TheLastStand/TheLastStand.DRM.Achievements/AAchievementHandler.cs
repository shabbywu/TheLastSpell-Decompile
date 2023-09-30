namespace TheLastStand.DRM.Achievements;

public abstract class AAchievementHandler
{
	public abstract void RefreshAchievements();

	public abstract void SetAchievementProgression(Stat stat, int value, bool refreshAchievements = true);

	public abstract void IncreaseAchievementProgression(Stat stat, int value, bool refreshAchievements = true);

	public abstract void UnlockAchievement(Achievement achievement, bool refreshIfAchieved = true);

	public abstract void UnlockAchievement(string achievementId, bool refreshIfAchieved = true);

	public abstract bool IsAchievementUnlocked(Achievement achievement);

	public abstract void ClearAchievements();
}
