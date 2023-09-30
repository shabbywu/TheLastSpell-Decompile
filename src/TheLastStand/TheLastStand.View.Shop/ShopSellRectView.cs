using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.Shop;

public class ShopSellRectView : MonoBehaviour
{
	[SerializeField]
	private RectTransform sellRectTransform;

	public bool IsInRect { get; private set; }

	private void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping && InventoryManager.InventoryView.DraggableItem.Displayed && InventoryManager.InventoryView.DraggableItem.ItemSlot != null)
		{
			bool flag = RectTransformUtility.RectangleContainsScreenPoint(sellRectTransform, Vector2.op_Implicit(InputManager.MousePosition));
			if (!IsInRect && flag)
			{
				IsInRect = true;
			}
			else if (IsInRect && !flag)
			{
				IsInRect = false;
			}
		}
	}
}
