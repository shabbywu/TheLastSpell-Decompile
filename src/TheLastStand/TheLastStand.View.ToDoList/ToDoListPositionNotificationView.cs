using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListPositionNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn != 0 && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			return;
		}
		for (int num = toDoTexts.Count - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)toDoTexts[num]).gameObject);
		}
		toDoTexts.Clear();
		int i = 0;
		for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
		{
			if (!TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].MovedThisDay || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].OriginTile.HasFog)
			{
				TextMeshProUGUI val = Object.Instantiate<TextMeshProUGUI>(toDoTextPrefab, (Transform)(object)notificationGroupRectTransform);
				((TMP_Text)val).text = "- " + TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitName;
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
