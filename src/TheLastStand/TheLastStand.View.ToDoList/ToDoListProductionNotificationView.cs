using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListProductionNotificationView : ToDoListNotificationView
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
		if (TPSingleton<BuildingManager>.Instance.ProductionReport != null)
		{
			for (int j = 0; j < TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count; j++)
			{
				TextMeshProUGUI val = Object.Instantiate<TextMeshProUGUI>(toDoTextPrefab, (Transform)(object)notificationGroupRectTransform);
				((TMP_Text)val).text = "- " + TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects[j].Name;
				toDoTexts.Add(val);
			}
		}
		CheckDisplay();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		Refresh();
	}
}
