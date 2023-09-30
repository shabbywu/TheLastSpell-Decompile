using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class EquipmentBoxSlotView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private Image equippedSlotImage;

	[SerializeField]
	private Image equippedSlotBGImage;

	[SerializeField]
	private Image equippedSlotLockedImage;

	[SerializeField]
	private DataColorTable rarityColors;

	[SerializeField]
	private bool shouldAttenuateWhenTwoHandedWeapon;

	private bool hasFocus;

	public TheLastStand.Model.Item.Item item { get; private set; }

	public void DisplayTooltip(bool display)
	{
		if (display && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.FollowElement.ChangeFollowDatas(followDatas);
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.SetContent(item, TileObjectSelectionManager.SelectedPlayableUnit);
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Display();
		}
		else
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Hide();
		}
	}

	public void DisplayLockedImage(bool display)
	{
		((Behaviour)equippedSlotLockedImage).enabled = display;
	}

	public void Refresh(TheLastStand.Model.Item.Item item, bool isTwoHandedWeapon = false)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		this.item = item;
		((Behaviour)equippedSlotImage).enabled = this.item != null;
		((Behaviour)equippedSlotBGImage).enabled = this.item != null;
		if (hasFocus)
		{
			OnPointerEnter(null);
		}
		if (this.item != null)
		{
			equippedSlotImage.sprite = ItemView.GetUiSprite(this.item.ItemDefinition.ArtId);
			equippedSlotBGImage.sprite = ItemView.GetUiSprite(this.item.ItemDefinition.ArtId, isBG: true);
			Color colorAt = rarityColors.GetColorAt((int)(this.item.Rarity - 1));
			((Graphic)equippedSlotImage).color = ((shouldAttenuateWhenTwoHandedWeapon && isTwoHandedWeapon) ? ColorExtensions.WithA(Color.white, 0.5f) : Color.white);
			((Graphic)equippedSlotBGImage).color = ((shouldAttenuateWhenTwoHandedWeapon && isTwoHandedWeapon) ? ColorExtensions.WithA(colorAt, 0.5f) : colorAt);
		}
	}

	public void RefreshColor(Color color, bool isTwoHandedWeapon = false)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (isTwoHandedWeapon && shouldAttenuateWhenTwoHandedWeapon)
		{
			color.a = 0.5f;
		}
		((Graphic)equippedSlotImage).color = color;
		if (item != null)
		{
			((Graphic)equippedSlotBGImage).color = rarityColors.GetColorAt((int)(item.Rarity - 1)) * color;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hasFocus = true;
		DisplayTooltip(display: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (hasFocus)
		{
			hasFocus = false;
			DisplayTooltip(display: false);
		}
	}
}
