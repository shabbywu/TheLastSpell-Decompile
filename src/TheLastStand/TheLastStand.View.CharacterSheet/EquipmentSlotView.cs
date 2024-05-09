using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.View.Generic;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class EquipmentSlotView : ItemSlotView
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private Image slotImage;

	[SerializeField]
	private Sprite slotPotentialTargetSprite;

	[SerializeField]
	private Image chainImage;

	private Sprite slotNormalSprite;

	private Navigation? slotNavigationBackup;

	private bool isDraggableItemHover;

	public EquipmentSlot EquipmentSlot
	{
		get
		{
			return base.ItemSlot as EquipmentSlot;
		}
		set
		{
			base.ItemSlot = value;
		}
	}

	public override void DisplayRarity(ItemDefinition.E_Rarity rarity, float offsetColorValue = 0f)
	{
		base.DisplayRarity(rarity, offsetColorValue);
		ToggleRarityParticles(TPSingleton<CharacterSheetPanel>.Instance.IsOpened);
	}

	public override void DisplayTooltip(bool display)
	{
		DisplayTooltip(display, GetItem(excludeSimulatedItem: false));
	}

	private void DisplayTooltip(bool display, TheLastStand.Model.Item.Item item)
	{
		if (display)
		{
			CharacterSheetPanel.ItemTooltip.FollowElement.ChangeFollowDatas(followDatas);
			CharacterSheetPanel.ItemTooltip.SetContent(item, TileObjectSelectionManager.SelectedPlayableUnit);
			CharacterSheetPanel.ItemTooltip.Display();
		}
		else
		{
			CharacterSheetPanel.ItemTooltip.Hide();
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
		{
			base.OnBeginDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver)
		{
			base.OnEndDrag(eventData);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		if (!base.HasFocus)
		{
			base.HasFocus = true;
			TPSingleton<CharacterSheetPanel>.Instance.FocusedEquipmentSlotView = this;
		}
		InventoryManager.InventoryView.DraggableItem.TargetItemSlot = base.ItemSlot;
		TheLastStand.Model.Item.Item item = GetItem(excludeSimulatedItem: false);
		if ((item != null && !InventoryManager.InventoryView.DraggableItem.Displayed) || IsSlotValid(InventoryManager.InventoryView.DraggableItem.ItemSlot))
		{
			((Behaviour)slotImage).enabled = true;
			slotImage.sprite = ((base.ItemSlot != null) ? slotPotentialTargetSprite : lockedHighlightSprite);
			TPSingleton<UIManager>.Instance.PlayAudioClipWithoutInterrupting(UIManager.ButtonHoverAudioClip);
		}
		if (item != null && TileObjectSelectionManager.HasPlayableUnitSelected && !InventoryManager.InventoryView.DraggableItem.Displayed && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			DisplayTooltip(display: true, item);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		TPSingleton<CharacterSheetPanel>.Instance.FocusedEquipmentSlotView = null;
		CharacterSheetPanel.ItemTooltip.Hide();
		if (InventoryManager.InventoryView.DraggableItem.TargetItemSlot == base.ItemSlot)
		{
			InventoryManager.InventoryView.DraggableItem.TargetItemSlot = null;
		}
		((Behaviour)slotImage).enabled = base.ItemSlot != null;
		slotImage.sprite = slotNormalSprite;
	}

	public void OnJoystickSelect()
	{
		OnPointerEnter(null);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnJoystickSubmit()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace != null)
		{
			if (!TPSingleton<HUDJoystickNavigationManager>.Instance.SlotSelectionToggleThisFrame)
			{
				OnSlotJoystickSelected();
				TPSingleton<CharacterSheetPanel>.Instance.OnEquipmentSlotJoystickSelectionOver();
			}
			return;
		}
		OnDoubleClick();
		if ((Object)(object)joystickHighlighter != (Object)null)
		{
			joystickHighlighter.HideButtons = GetItem() == null;
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.UpdateGamepadInputDisplays();
		}
	}

	public void OnJoystickSelectionBegin()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!slotNavigationBackup.HasValue)
		{
			slotNavigationBackup = ((Selectable)joystickSelectable).navigation;
			Navigation value = slotNavigationBackup.Value;
			if (!IsSelectableSlotValid(((Navigation)(ref value)).selectOnDown))
			{
				((Selectable)(object)joystickSelectable).SetSelectOnDown(null);
			}
			value = slotNavigationBackup.Value;
			if (!IsSelectableSlotValid(((Navigation)(ref value)).selectOnUp))
			{
				((Selectable)(object)joystickSelectable).SetSelectOnUp(null);
			}
			value = slotNavigationBackup.Value;
			if (!IsSelectableSlotValid(((Navigation)(ref value)).selectOnRight))
			{
				((Selectable)(object)joystickSelectable).SetSelectOnRight(null);
			}
			value = slotNavigationBackup.Value;
			if (!IsSelectableSlotValid(((Navigation)(ref value)).selectOnLeft))
			{
				((Selectable)(object)joystickSelectable).SetSelectOnLeft(null);
			}
		}
	}

	public void OnSlotJoystickSelected()
	{
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.OnEquipmentSlotSelected(EquipmentSlot);
	}

	public void RestoreJoystickNavigation()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (slotNavigationBackup.HasValue)
		{
			((Selectable)joystickSelectable).navigation = slotNavigationBackup.Value;
			slotNavigationBackup = null;
		}
	}

	public override void Refresh()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		((Component)this).gameObject.SetActive(true);
		((Behaviour)chainImage).enabled = base.ItemSlot == null;
		((Behaviour)slotImage).enabled = base.ItemSlot != null;
		if (base.ItemSlot == null)
		{
			levelBadge.Refresh(-1);
			((Behaviour)base.ItemIcon).enabled = false;
			base.ItemIcon.sprite = null;
			((Behaviour)base.ItemIconBG).enabled = false;
			base.ItemIconBG.sprite = null;
			((Graphic)base.ItemIcon).color = Color.white;
			((Behaviour)base.BackgroundImage).enabled = false;
			base.BackgroundImage.sprite = null;
			return;
		}
		DraggedItem draggableItem = InventoryManager.InventoryView.DraggableItem;
		TheLastStand.Model.Item.Item item = TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView?.ItemSlot?.Item;
		ClearSlot();
		TheLastStand.Model.Item.Item item2 = GetItem(excludeSimulatedItem: false);
		if (item2 == null)
		{
			((Behaviour)base.ItemIcon).enabled = false;
			base.ItemIcon.sprite = null;
			base.ItemIconBG.sprite = null;
			ChangeBackgroundColor(Color.white);
		}
		else
		{
			base.ItemIcon.sprite = ItemView.GetUiSprite(item2.ItemDefinition.ArtId);
			((Behaviour)base.ItemIcon).enabled = true;
			base.ItemIconBG.sprite = ItemView.GetUiSprite(item2.ItemDefinition.ArtId, isBG: true);
			((Graphic)base.ItemIconBG).color = iconRarityColors.GetColorAt((int)(item2.Rarity - 1));
			if (EquipmentSlot.BlockedByOtherSlot != null)
			{
				Color color = default(Color);
				((Color)(ref color))._002Ector(1f, 1f, 1f, 0.5f);
				ChangeBackgroundColor(color);
				((Graphic)base.ItemIcon).color = color;
				DisplayRarity(item2.Rarity, -0.5f);
			}
			else
			{
				ChangeBackgroundColor(Color.white);
				DisplayRarity(item2.Rarity);
			}
		}
		if (base.HasFocus && TileObjectSelectionManager.HasPlayableUnitSelected && (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips))
		{
			DisplayTooltip(display: true, item2);
		}
		RefreshSlotValidity((Object)(object)draggableItem != (Object)null && draggableItem.Displayed, item != null);
	}

	public void RefreshSlotValidity(bool isDragging, bool isHovering)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (isDragging)
		{
			flag = !((Behaviour)chainImage).enabled && IsSlotValid(InventoryManager.InventoryView.DraggableItem.ItemSlot);
		}
		else if (isHovering)
		{
			flag = !((Behaviour)chainImage).enabled && IsSlotValid(TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView.ItemSlot);
		}
		((Graphic)slotImage).color = (flag ? Color.white : itemSlotInvalidColor._Color);
		((Behaviour)base.ItemIconBG).enabled = GetItem() != null && flag;
		((Graphic)base.ItemIcon).color = (Color)((!flag) ? itemSlotInvalidColor._Color : ((EquipmentSlot?.BlockedByOtherSlot != null) ? new Color(1f, 1f, 1f, 0.5f) : Color.white));
		ChangeBackgroundColor((Color)((!flag) ? itemSlotInvalidColor._Color : ((EquipmentSlot?.BlockedByOtherSlot != null) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white)));
		if (base.ItemSlot != null)
		{
			TPSingleton<CharacterSheetPanel>.Instance.SetBackgroundColorForSlot(base.ItemSlot.ItemSlotDefinition.Id, flag ? Color.white : itemSlotInvalidColor._Color);
		}
	}

	protected override void ClearSlot()
	{
		base.ClearSlot();
		TheLastStand.Model.Item.Item item = GetItem(excludeSimulatedItem: false);
		TheLastStand.Model.Item.Item item2 = GetItem();
		if (item != null)
		{
			DisplayRarity(item.Rarity, (item2 != null) ? 0f : (-0.5f));
		}
		else
		{
			DisplayRarity(ItemDefinition.E_Rarity.None);
		}
	}

	protected override TheLastStand.Model.Item.Item GetItem()
	{
		return GetItem();
	}

	protected override void OnDoubleClick()
	{
		if (base.ItemSlot?.Item != null && !InventoryManager.InventoryView.DraggableItem.Displayed)
		{
			base.OnDoubleClick();
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.OnEquipmentSlotDoubleClick(EquipmentSlot);
			TPSingleton<CharacterSheetPanel>.Instance.RefreshEquipmentSlotsValidity();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		slotNormalSprite = slotImage.sprite;
		GenerateRaritiesParticlesDictionary();
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	protected override void ToggleRarityParticlesHook(bool enable)
	{
		if (enable)
		{
			if (!isParticleSystemHooked)
			{
				TPSingleton<CharacterSheetPanel>.Instance.OnCharacterSheetToggle += base.ToggleRarityParticles;
			}
		}
		else if (isParticleSystemHooked)
		{
			TPSingleton<CharacterSheetPanel>.Instance.OnCharacterSheetToggle -= base.ToggleRarityParticles;
		}
		isParticleSystemHooked = enable;
	}

	private TheLastStand.Model.Item.Item GetItem(bool excludeSimulatedItem = true)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			return null;
		}
		if (base.ItemSlot == null)
		{
			return null;
		}
		if (excludeSimulatedItem || EquipmentSlot.BlockedByOtherSlot == null)
		{
			return EquipmentSlot.Item;
		}
		return EquipmentSlot.BlockedByOtherSlot.Item;
	}

	private bool IsSelectableSlotValid(Selectable neighbourSelectable)
	{
		EquipmentSlotView equipmentSlotView = default(EquipmentSlotView);
		if ((Object)(object)neighbourSelectable != (Object)null && ((Component)neighbourSelectable).TryGetComponent<EquipmentSlotView>(ref equipmentSlotView))
		{
			return equipmentSlotView.IsSlotValid(TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace);
		}
		return false;
	}

	private bool IsSlotValid(ItemSlot itemSlot)
	{
		if (itemSlot?.Item != null)
		{
			EquipmentSlot equipmentSlot = EquipmentSlot;
			if (equipmentSlot != null && equipmentSlot.BlockedByOtherSlot == null && EquipmentSlot.EquipmentSlotController.IsItemCompatible(itemSlot.Item))
			{
				if (EquipmentSlot.Item != null)
				{
					return itemSlot.ItemSlotController.IsItemCompatible(EquipmentSlot.Item);
				}
				return true;
			}
		}
		return false;
	}

	private void OnDestroy()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (base.HasFocus)
		{
			DisplayTooltip(showTooltips);
		}
	}
}
