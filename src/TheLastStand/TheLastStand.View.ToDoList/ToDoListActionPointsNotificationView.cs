using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListActionPointsNotificationView : ToDoListNotificationView
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
			if (playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints) > 0f)
			{
				TextMeshProUGUI val = Object.Instantiate<TextMeshProUGUI>(toDoTextPrefab, (Transform)(object)notificationGroupRectTransform);
				((TMP_Text)val).text = string.Format("- {0} <style=\"Outline\">({1} {2})</style>", playableUnit.Name, playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints), Localizer.Get("ToDoList_ActionPointsText"));
				toDoTexts.Add(val);
			}
		}
		CheckDisplay();
	}
}
