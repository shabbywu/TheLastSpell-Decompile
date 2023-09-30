using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListGoldNotificationView : ToDoListNotificationView
{
	[SerializeField]
	private GameObject shopSubNotification;

	[SerializeField]
	private GameObject innSubNotification;

	public override void Refresh()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		if (base.OnlyShowDuringDayTurn == Game.E_DayTurn.Undefined || TPSingleton<GameManager>.Instance.Game.DayTurn == base.OnlyShowDuringDayTurn)
		{
			shopSubNotification.SetActive(TPSingleton<BuildingManager>.Instance.AccessShopBuildingCount > 0 || ShopManager.DebugShopForceAccess);
			innSubNotification.SetActive(BuildingManager.HasInn());
			LayoutRebuilder.ForceRebuildLayoutImmediate(notificationGroupRectTransform);
			notificationRectTransform.sizeDelta = new Vector2(notificationRectTransform.sizeDelta.x, notificationGroupRectTransform.sizeDelta.y);
		}
	}
}
