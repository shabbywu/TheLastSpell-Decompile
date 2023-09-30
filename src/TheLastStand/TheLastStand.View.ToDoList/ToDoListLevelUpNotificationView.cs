using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListLevelUpNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn != 0 && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			return;
		}
		for (int i = 0; i < toDoTexts.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)toDoTexts[i]).gameObject);
		}
		toDoTexts.Clear();
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.StatsPoints > 0)
			{
				TextMeshProUGUI val = Object.Instantiate<TextMeshProUGUI>(toDoTextPrefab, (Transform)(object)notificationGroupRectTransform);
				((TMP_Text)val).text = "- " + playableUnit.Name + ((playableUnit.LevelPoints > 1) ? string.Format(" <style=\"Outline\">({0} {1})</style>", playableUnit.LevelPoints, Localizer.Get("ToDoList_LevelUpText")) : string.Empty);
				toDoTexts.Add(val);
			}
		}
		CheckDisplay();
	}
}
