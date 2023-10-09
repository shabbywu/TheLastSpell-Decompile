using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public abstract class ItemSlotView : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	[SerializeField]
	protected DataColor itemSlotInvalidColor;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private Image itemIconBG;

	[SerializeField]
	protected DataColorTable iconRarityColors;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Image backgroundGlowImage;

	[SerializeField]
	protected Sprite lockedHighlightSprite;

	[SerializeField]
	protected ItemLevelBadgeView levelBadge;

	[SerializeField]
	private GameObject epicParticles;

	[SerializeField]
	private GameObject rareParticles;

	[SerializeField]
	private bool isSmallSlot;

	[SerializeField]
	protected JoystickSelectable joystickSelectable;

	[SerializeField]
	protected JoystickHighlighter joystickHighlighter;

	protected bool isParticleSystemHooked;

	private bool hasFocus;

	public bool HasFocus
	{
		get
		{
			return hasFocus;
		}
		protected set
		{
			if (value != hasFocus)
			{
				if ((Object)(object)backgroundGlowImage.sprite != (Object)null)
				{
					((Behaviour)backgroundGlowImage).enabled = value && !InventoryManager.InventoryView.DraggableItem.Displayed;
				}
				hasFocus = value;
			}
		}
	}

	public Image BackgroundImage => backgroundImage;

	public Image BackgroundGlowImage => backgroundGlowImage;

	public bool IsSmallSlot
	{
		get
		{
			return isSmallSlot;
		}
		set
		{
			isSmallSlot = value;
			if ((Object)(object)levelBadge != (Object)null)
			{
				levelBadge.IsSmallSlot = IsSmallSlot;
			}
		}
	}

	public Image ItemIcon => itemIcon;

	public Image ItemIconBG => itemIconBG;

	public ItemSlot ItemSlot { get; set; }

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	protected Dictionary<ItemDefinition.E_Rarity, GameObject> RarityParticles { get; private set; }

	public virtual void DisplayRarity(ItemDefinition.E_Rarity rarity, float offsetColorValue = 0f)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)BackgroundImage == (Object)null))
		{
			Sprite val = ((rarity == ItemDefinition.E_Rarity.None) ? null : ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}{2}_Off", "View/Sprites/UI/Items/Rarity/ItemBox_0", (int)rarity, isSmallSlot ? "_Small" : string.Empty), failSilently: false));
			Sprite val2 = ((rarity == ItemDefinition.E_Rarity.None) ? null : ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}{2}_On", "View/Sprites/UI/Items/Rarity/ItemBox_0", (int)rarity, isSmallSlot ? "_Small" : string.Empty), failSilently: false));
			((Behaviour)BackgroundImage).enabled = (Object)(object)val != (Object)null;
			BackgroundImage.sprite = val;
			if ((Object)(object)val2 == (Object)null)
			{
				((Behaviour)backgroundGlowImage).enabled = false;
			}
			backgroundGlowImage.sprite = val2;
			Color color = TPHelpers.OffsetValue(Color.white, offsetColorValue);
			color.a = 1f;
			ChangeBackgroundColor(color);
			RefreshHookRarityParticles(rarity);
		}
	}

	public abstract void DisplayTooltip(bool display);

	protected void DisplayTooltip(bool display, int playerIndex, FollowElement.FollowDatas followData)
	{
		if (ItemSlot == null)
		{
			TPDebug.LogError((object)"ItemSlot can't be null! Aborting...", (Object)(object)this);
			return;
		}
		UnregisterComparedItems();
		if (display)
		{
			EquipmentSlot equipmentSlot = null;
			PlayableUnit playableUnit = ((playerIndex != -1) ? TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[playerIndex] : null);
			if (ItemSlot.Item != null && playableUnit != null)
			{
				equipmentSlot = playableUnit.PlayableUnitController.GetBestItemSlotToCompare(ItemSlot.Item);
				RegisterComparedItems(equipmentSlot, playableUnit);
			}
			if (followData != null)
			{
				TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.FollowElement.ChangeFollowDatas(followData);
			}
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.SetContent(ItemSlot.Item, playableUnit);
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Display();
			if (equipmentSlot?.Item != null)
			{
				CharacterSheetPanel.ItemTooltip.SetContent(equipmentSlot.Item, playableUnit);
				CharacterSheetPanel.ItemTooltip.Display();
			}
			else
			{
				CharacterSheetPanel.ItemTooltip.Hide();
			}
		}
		else
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ItemTooltip.Hide();
			CharacterSheetPanel.ItemTooltip.Hide();
		}
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)itemIcon == (Object)null) && GetItem() != null)
		{
			InventoryManager.InventoryView.DraggableItem.SetContent(ItemSlot);
			InventoryManager.InventoryView.DraggableItem.Display();
			Color color = ((Graphic)itemIcon).color;
			color.a = 0.5f;
			((Graphic)itemIcon).color = color;
			Color color2 = ((Graphic)backgroundImage).color;
			color2.a = 0.5f;
			((Graphic)backgroundImage).color = color2;
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.BeginDragAudioClip);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		InventoryManager.InventoryView.DraggableItem.Hide();
		Color color = ((Graphic)itemIcon).color;
		color.a = 1f;
		((Graphic)itemIcon).color = color;
		Color color2 = ((Graphic)backgroundImage).color;
		color2.a = 1f;
		((Graphic)backgroundImage).color = color2;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if (eventData != null && eventData.clickCount == 2)
		{
			OnDoubleClick();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (ItemSlot != null && ItemSlot.Item != null)
		{
			TPSingleton<UIManager>.Instance.PlayAudioClipWithoutInterrupting(UIManager.ButtonHoverAudioClip);
		}
		if (!HasFocus)
		{
			HasFocus = true;
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (HasFocus)
		{
			HasFocus = false;
		}
	}

	public virtual void Refresh()
	{
		if ((Object)(object)joystickHighlighter != (Object)null)
		{
			joystickHighlighter.HideButtons = GetItem() == null;
		}
		if (ItemSlot == null)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else if ((Object)(object)levelBadge != (Object)null)
		{
			levelBadge.Refresh(ItemSlot.Item?.Level ?? 0);
		}
	}

	public void StopParticleSystems()
	{
		if ((Object)(object)epicParticles != (Object)null)
		{
			epicParticles.SetActive(false);
		}
		if ((Object)(object)rareParticles != (Object)null)
		{
			rareParticles.SetActive(false);
		}
	}

	protected virtual void Awake()
	{
		if ((Object)(object)joystickSelectable == (Object)null)
		{
			joystickSelectable = ((Component)this).GetComponent<JoystickSelectable>();
		}
		IsSmallSlot = isSmallSlot;
		ItemIcon.sprite = null;
		((Behaviour)ItemIcon).enabled = false;
		GenerateRaritiesParticlesDictionary();
	}

	protected void ChangeBackgroundColor(Color color)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BackgroundImage != (Object)null)
		{
			((Graphic)BackgroundImage).color = color;
		}
	}

	protected virtual void ClearSlot()
	{
		if (InventoryManager.InventoryView.DraggableItem.TargetItemSlot == ItemSlot)
		{
			InventoryManager.InventoryView.DraggableItem.TargetItemSlot = null;
		}
	}

	protected void GenerateRaritiesParticlesDictionary()
	{
		RarityParticles = new Dictionary<ItemDefinition.E_Rarity, GameObject>(default(ItemDefinition.RarityComparer))
		{
			{
				ItemDefinition.E_Rarity.Epic,
				epicParticles
			},
			{
				ItemDefinition.E_Rarity.Rare,
				rareParticles
			}
		};
	}

	protected abstract TheLastStand.Model.Item.Item GetItem();

	protected virtual void OnDoubleClick()
	{
	}

	protected void RegisterComparedItems(EquipmentSlot equipmentSlot, PlayableUnit playableUnit)
	{
		TPSingleton<ItemManager>.Instance.EquippedItemBeingCompared = equipmentSlot?.Item;
		if (ItemSlot.Item.IsTwoHandedWeapon && playableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value))
		{
			TheLastStand.Model.Item.Item item = value[playableUnit.EquippedWeaponSetIndex]?.Item;
			if (item != equipmentSlot?.Item)
			{
				TPSingleton<ItemManager>.Instance.EquippedItemBeingComparedOffHand = item;
			}
		}
	}

	protected void UnregisterComparedItems()
	{
		TPSingleton<ItemManager>.Instance.EquippedItemBeingCompared = null;
		TPSingleton<ItemManager>.Instance.EquippedItemBeingComparedOffHand = null;
	}

	private void RefreshHookRarityParticles(ItemDefinition.E_Rarity rarity)
	{
		if ((uint)(rarity - 3) <= 1u)
		{
			ToggleRarityParticlesHook(enable: true);
		}
		else
		{
			ToggleRarityParticlesHook(enable: false);
		}
	}

	protected abstract void ToggleRarityParticlesHook(bool enable);

	protected void ToggleRarityParticles(bool enable)
	{
		if (enable)
		{
			ItemSlot itemSlot = ItemSlot;
			if (itemSlot != null)
			{
				TheLastStand.Model.Item.Item item = itemSlot.Item;
				if (item != null)
				{
					_ = item.Rarity;
					if (true)
					{
						RefreshRarityParticles(ItemSlot.Item.Rarity);
						return;
					}
				}
			}
		}
		StopParticleSystems();
	}

	protected void RefreshRarityParticles(ItemDefinition.E_Rarity rarity)
	{
		if (RarityParticles == null)
		{
			return;
		}
		foreach (KeyValuePair<ItemDefinition.E_Rarity, GameObject> rarityParticle in RarityParticles)
		{
			GameObject value = rarityParticle.Value;
			if (value != null)
			{
				value.SetActive(rarityParticle.Key == rarity);
			}
		}
	}
}
