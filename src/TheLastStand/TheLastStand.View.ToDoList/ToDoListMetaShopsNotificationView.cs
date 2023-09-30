using System;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListMetaShopsNotificationView : ToDoListNotificationView
{
	[SerializeField]
	private GameObject darkShopSubNotification;

	[SerializeField]
	private GameObject lightShopSubNotification;

	public override void Refresh()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (base.OnlyShowDuringDayTurn != 0 && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			Display(show: false);
			return;
		}
		darkShopSubNotification.SetActive(TPSingleton<DarkShopManager>.Instance.IsAnyUpgradeAffordable());
		lightShopSubNotification.SetActive(TPSingleton<LightShopManager>.Instance.IsAnyUpgradeAffordable());
		Display(MetaShopsManager.CanOpenShops() && (darkShopSubNotification.activeSelf || lightShopSubNotification.activeSelf));
		LayoutRebuilder.ForceRebuildLayoutImmediate(notificationGroupRectTransform);
		notificationRectTransform.sizeDelta = new Vector2(notificationRectTransform.sizeDelta.x, notificationGroupRectTransform.sizeDelta.y);
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
