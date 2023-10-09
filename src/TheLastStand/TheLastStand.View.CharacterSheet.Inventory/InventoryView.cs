using System;
using System.Collections;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet.Inventory;

public class InventoryView : TabbedPageView
{
	[SerializeField]
	private Transform itemsPanelTransform;

	[SerializeField]
	private ItemTooltip itemTooltip;

	[SerializeField]
	private DraggedItem draggableItem;

	[SerializeField]
	private Scrollbar itemsPanelScrollbar;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private Button shopButton;

	[SerializeField]
	private ScrollRect itemsScrollRect;

	[SerializeField]
	private RectTransform slotsViewport;

	public DraggedItem DraggableItem => draggableItem;

	public TheLastStand.Model.Item.Inventory Inventory { get; set; }

	public InventorySlotView FocusedInventorySlotView { get; set; }

	public ItemTooltip ItemTooltip => itemTooltip;

	public Transform ItemsPanelTransform => itemsPanelTransform;

	public float ScrollButtonsSensitivity => scrollButtonsSensitivity;

	public event Action<bool> OnInventoryToggle;

	public override void Close()
	{
		if (base.IsOpened)
		{
			this.OnInventoryToggle?.Invoke(obj: false);
			base.Close();
			InventoryManager.InventoryView.DraggableItem.Reset();
			ItemTooltip.Hide();
			CharacterSheetPanel.ItemTooltip.Hide();
			((Behaviour)itemsScrollRect).enabled = false;
		}
	}

	public override void Open()
	{
		if (!base.IsOpened)
		{
			((Behaviour)itemsScrollRect).enabled = true;
			this.OnInventoryToggle?.Invoke(obj: true);
			base.Open();
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.MarkAllItemsAsSeen();
			((MonoBehaviour)this).StartCoroutine(ResetScrollbarAfterAFrame());
			((MonoBehaviour)this).StartCoroutine(TriggerTutorialAfterCharacterSheetTween());
		}
	}

	public void OnBotButtonClick()
	{
		itemsPanelScrollbar.value = Mathf.Clamp01(itemsPanelScrollbar.value - scrollButtonsSensitivity);
	}

	public void OnShopButtonClick()
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel())
		{
			CharacterSheetManager.CloseCharacterSheetPanel(toAnotherPopup: true);
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.OpenShopPanel(fromAnotherPopup: true, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit));
		}
	}

	public void OnTopButtonClick()
	{
		itemsPanelScrollbar.value = Mathf.Clamp01(itemsPanelScrollbar.value + scrollButtonsSensitivity);
	}

	public void ResetScrollbar()
	{
		itemsPanelScrollbar.value = 1f;
	}

	public IEnumerator ResetScrollbarAfterAFrame()
	{
		yield return null;
		ResetScrollbar();
	}

	public void OnSlotViewJoystickSelect(InventorySlotView source)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		GUIHelpers.AdjustScrollViewToFocusedItem((RectTransform)((Component)source).transform, slotsViewport, itemsPanelScrollbar, 0.01f, 0.01f);
	}

	public override void Refresh()
	{
		base.Refresh();
		foreach (InventorySlot inventorySlot in Inventory.InventorySlots)
		{
			inventorySlot.InventorySlotView.Refresh();
		}
		((Component)shopButton).gameObject.SetActive(TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel());
	}

	protected override void Start()
	{
		for (int i = 0; i < itemsPanelTransform.childCount; i++)
		{
			((Component)itemsPanelTransform.GetChild(i)).GetComponent<InventorySlotView>().ItemIndex = i;
		}
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		if ((Object)(object)FocusedInventorySlotView != (Object)null && InputManager.GetButtonDown(79) && InputManager.IsLastControllerJoystick)
		{
			FocusedInventorySlotView.OnJoystickSubmit();
		}
	}

	private IEnumerator TriggerTutorialAfterCharacterSheetTween()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => !TPSingleton<CharacterSheetPanel>.Instance.IsDisplayTweenPlaying));
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnInventoryOpen);
	}
}
