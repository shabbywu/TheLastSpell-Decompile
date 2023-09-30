using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.Item;
using TheLastStand.View.PlayableUnitCustomisation;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Perk;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class CharacterSheetPanel : TPSingleton<CharacterSheetPanel>, IOverlayUser
{
	public static class Constants
	{
		public const string DefaultLocalizedFontChildren = "Default";

		public const string LeftPanelLocalizedFontChildren = "LeftPanel";

		public const string CharacterDetailsLocalizedFontChildren = "CharacterDetails";

		public const string InventoryPanelLocalizedFontChildren = "InventoryPanel";

		public const string PerksPanelLocalizedFontChildren = "PerksPanel";
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<KeyValuePair<Toggle, TabbedPageView>, bool> _003C_003E9__84_0;

		public static TweenCallback _003C_003E9__94_1;

		public static Func<Toggle, bool> _003C_003E9__120_0;

		internal bool _003CClose_003Eb__84_0(KeyValuePair<Toggle, TabbedPageView> x)
		{
			return x.Key.isOn;
		}

		internal void _003COpen_003Eb__94_1()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}

		internal bool _003CSelectNeighbourTab_003Eb__120_0(Toggle x)
		{
			return ((Selectable)x).interactable;
		}
	}

	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private Dictionary<Toggle, TabbedPageView> tabPagePairs;

	[SerializeField]
	private Toggle toggleUnitDetails;

	[SerializeField]
	private Toggle toggleInventory;

	[SerializeField]
	private Toggle togglePerk;

	[SerializeField]
	private GameObject inventoryUnavailableParent;

	[SerializeField]
	private GameObject inventoryUnavailableText;

	[SerializeField]
	private GameObject perkUnavailableParent;

	[SerializeField]
	private GameObject perkUnavailableText;

	[SerializeField]
	private Transform tabPosOn;

	[SerializeField]
	private Transform tabPosOff;

	[SerializeField]
	private TextMeshProUGUI unitName;

	[SerializeField]
	private UnitLevelDisplay unitLevel;

	[SerializeField]
	private GameObject changeSelectedHeroButtonsParent;

	[SerializeField]
	private UnitStatWithRegenDisplay healthStatDisplay;

	[SerializeField]
	private UnitStatDisplay healthRegenStatDisplay;

	[SerializeField]
	private UnitStatWithRegenDisplay manaStatDisplay;

	[SerializeField]
	private UnitStatDisplay manaRegenStatDisplay;

	[SerializeField]
	private UnitStatDisplay actionPointsStatDisplay;

	[SerializeField]
	private UnitStatDisplay movementPointsDisplay;

	[SerializeField]
	private UnitStatDisplay overallDamageStatDisplay;

	[SerializeField]
	private AdditionalUnitStatDisplay criticalStatDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceReductionStatDisplay;

	[SerializeField]
	private UnitStatDisplay accuracyStatDisplay;

	[SerializeField]
	private UnitStatDisplay blockStatDisplay;

	[SerializeField]
	private UnitStatDisplay armorStatDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceStatDisplay;

	[SerializeField]
	private UnitStatDisplay dodgeStatDisplay;

	[SerializeField]
	private Image avatarImage;

	[SerializeField]
	private ItemTooltip itemTooltip;

	[SerializeField]
	private Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlots = new Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>>();

	[SerializeField]
	private Dictionary<ItemSlotDefinition.E_ItemSlotId, Image> slotsBackgroundImages = new Dictionary<ItemSlotDefinition.E_ItemSlotId, Image>();

	[SerializeField]
	private HideHelmetView hideHelmetView;

	[SerializeField]
	private SkillListDisplay equippedSkillsDisplay;

	[SerializeField]
	private Image characterDetailsNotif;

	[SerializeField]
	private UnitPortraitView unitPortaitDetails;

	[SerializeField]
	private UnitPerkTreeView unitPerkTreeView;

	[SerializeField]
	private Image perkAvailableNotif;

	[SerializeField]
	private HUDJoystickTarget rightPanelJoystickTarget;

	[SerializeField]
	private HUDJoystickTarget perksJoystickTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget closeJoystickTarget;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	[SerializeField]
	private AudioClip openClip;

	[SerializeField]
	private AudioClip closeClip;

	private Canvas canvas;

	private Tweener displayTween;

	private bool initialized;

	private RectTransform rectTransform;

	private float startY;

	public static Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> EquipmentSlots => TPSingleton<CharacterSheetPanel>.Instance.equipmentSlots;

	public bool IsDisplayTweenPlaying
	{
		get
		{
			Tweener obj = displayTween;
			if (obj == null)
			{
				return false;
			}
			return TweenExtensions.IsPlaying((Tween)(object)obj);
		}
	}

	public static bool HasClosedThisFrame { get; private set; }

	public static ItemTooltip ItemTooltip => TPSingleton<CharacterSheetPanel>.Instance.itemTooltip;

	public bool IsInventoryOpened => toggleInventory.isOn;

	public bool IsPerksPanelOpened => togglePerk.isOn;

	public bool IsOpened { get; private set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 2;

	public EquipmentSlotView FocusedEquipmentSlotView { get; set; }

	public HUDJoystickTarget PerksJoystickTarget => perksJoystickTarget;

	public HUDJoystickTarget RightPanelJoystickTarget => rightPanelJoystickTarget;

	public UnitPerkTreeView UnitPerkTreeView => unitPerkTreeView;

	public event Action<bool> OnCharacterSheetToggle;

	public static void Init()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<CharacterSheetPanel>.Instance.initialized)
		{
			return;
		}
		TPSingleton<CharacterSheetPanel>.Instance.initialized = true;
		TPSingleton<CharacterSheetPanel>.Instance.canvas = ((Component)TPSingleton<CharacterSheetPanel>.Instance).GetComponent<Canvas>();
		((Behaviour)TPSingleton<CharacterSheetPanel>.Instance.canvas).enabled = false;
		TPSingleton<CharacterSheetPanel>.Instance.rectTransform = ((Component)TPSingleton<CharacterSheetPanel>.Instance).GetComponent<RectTransform>();
		TPSingleton<CharacterSheetPanel>.Instance.startY = TPSingleton<CharacterSheetPanel>.Instance.rectTransform.anchoredPosition.y;
		if (TPSingleton<CharacterSheetPanel>.Instance.tabPagePairs == null)
		{
			CLoggerManager.Log((object)"[CharacterSheetPanel] tabPagePairs is null!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			foreach (KeyValuePair<Toggle, TabbedPageView> tabbedPage in TPSingleton<CharacterSheetPanel>.Instance.tabPagePairs)
			{
				((UnityEvent<bool>)(object)tabbedPage.Key.onValueChanged).AddListener((UnityAction<bool>)delegate(bool value)
				{
					TPSingleton<CharacterSheetPanel>.Instance.TabbedPageToggle_ValueChanged(tabbedPage.Key, value);
				});
			}
		}
		TPSingleton<CharacterSheetPanel>.Instance.healthStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		TPSingleton<CharacterSheetPanel>.Instance.healthStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		TPSingleton<CharacterSheetPanel>.Instance.healthStatDisplay.RegenStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthRegen];
		TPSingleton<CharacterSheetPanel>.Instance.healthRegenStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthRegen];
		TPSingleton<CharacterSheetPanel>.Instance.manaStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Mana];
		TPSingleton<CharacterSheetPanel>.Instance.manaStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaTotal];
		TPSingleton<CharacterSheetPanel>.Instance.manaStatDisplay.RegenStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaRegen];
		TPSingleton<CharacterSheetPanel>.Instance.manaRegenStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaRegen];
		TPSingleton<CharacterSheetPanel>.Instance.actionPointsStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
		TPSingleton<CharacterSheetPanel>.Instance.actionPointsStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal];
		TPSingleton<CharacterSheetPanel>.Instance.movementPointsDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		TPSingleton<CharacterSheetPanel>.Instance.movementPointsDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal];
		TPSingleton<CharacterSheetPanel>.Instance.overallDamageStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.OverallDamage];
		TPSingleton<CharacterSheetPanel>.Instance.criticalStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Critical];
		TPSingleton<CharacterSheetPanel>.Instance.criticalStatDisplay.AdditionalStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.CriticalPower];
		TPSingleton<CharacterSheetPanel>.Instance.resistanceReductionStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ResistanceReduction];
		TPSingleton<CharacterSheetPanel>.Instance.accuracyStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Accuracy];
		TPSingleton<CharacterSheetPanel>.Instance.blockStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Block];
		TPSingleton<CharacterSheetPanel>.Instance.armorStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Armor];
		TPSingleton<CharacterSheetPanel>.Instance.armorStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ArmorTotal];
		TPSingleton<CharacterSheetPanel>.Instance.resistanceStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Resistance];
		TPSingleton<CharacterSheetPanel>.Instance.dodgeStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Dodge];
	}

	public void Close(bool toAnotherPopup = false)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOpened)
		{
			return;
		}
		IsOpened = false;
		this.OnCharacterSheetToggle?.Invoke(obj: false);
		if (tabPagePairs.Count((KeyValuePair<Toggle, TabbedPageView> x) => x.Key.isOn) == 0)
		{
			return;
		}
		if (!toAnotherPopup)
		{
			CameraView.AttenuateWorldForPopupFocus(null);
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OnPopupExitToWorld();
			}
		}
		if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			TPSingleton<UnitLevelUpView>.Instance.Close();
		}
		if (TPSingleton<ToDoListView>.Instance.IsDisplayed && IsInventoryOpened)
		{
			TPSingleton<ToDoListView>.Instance.CloseInventoryNotification();
		}
		Tweener obj = displayTween;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		if (toAnotherPopup)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, startY);
			((Behaviour)canvas).enabled = false;
		}
		else
		{
			displayTween = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, startY, 0.25f, true), (Ease)26), (TweenCallback)delegate
			{
				((Behaviour)canvas).enabled = false;
			}), (TweenCallback)delegate
			{
				displayTween = null;
			});
		}
		InventoryManager.InventoryView.DraggableItem.Reset();
		DeactivateSlots();
		((MonoBehaviour)this).StartCoroutine(CloseCoroutine());
		SoundManager.PlayAudioClip(closeClip, UIManager.PooledAudioSourceData);
	}

	public void OnChangeWeaponSetButtonClick()
	{
		TPSingleton<PlayableUnitManager>.Instance.ChangeEquipment();
	}

	public void OnCloseButtonClick()
	{
		CharacterSheetManager.CloseCharacterSheetPanel();
	}

	public void OnNextUnitButtonClick()
	{
		OnSelectNewUnitButtonClick(next: true);
	}

	public void OnPreviousUnitButtonClick()
	{
		OnSelectNewUnitButtonClick(next: false);
	}

	private void OnSelectNewUnitButtonClick(bool next)
	{
		if (TPSingleton<UnitLevelUpView>.Instance.IsProceedingToALevelUp)
		{
			return;
		}
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace != null)
		{
			if (!InputManager.JoystickConfig.HUDNavigation.CanChangeHeroWhileEquipmentSlotSelection)
			{
				return;
			}
			OnEquipmentSlotJoystickSelectionOver();
		}
		else if (TPSingleton<UnitLevelUpView>.Instance.IsOpened && !IsInventoryOpened && !IsPerksPanelOpened && InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(GameView.CharacterDetailsView.SecondaryAttributesHUDJoystickTarget.GetSelectionInfo());
		}
		bool num = InputManager.IsLastControllerJoystick && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)GameView.CharacterDetailsView.LevelUpButton).gameObject;
		InventoryManager.InventoryView.DraggableItem.Reset();
		PlayableUnitManager.SelectNewUnit(next);
		Refresh();
		RefreshLevelUpPanel();
		if (num)
		{
			((MonoBehaviour)this).StartCoroutine(LevelUpButtonJoystickDeselectedCoroutine());
		}
		if (InputManager.IsLastControllerJoystick && !TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.IsMoving)
		{
			((MonoBehaviour)this).StartCoroutine(UpdateJoystickHighlightPositionEndOfFrame());
		}
	}

	private IEnumerator UpdateJoystickHighlightPositionEndOfFrame()
	{
		yield return null;
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
	}

	public void OnCustomisationUnitButtonClick()
	{
		GameController.SetState(Game.E_State.UnitCustomisation);
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.Open(TileObjectSelectionManager.SelectedPlayableUnit);
	}

	public void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		if (state == Game.E_State.CharacterSheet && previousState == Game.E_State.UnitCustomisation && InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(rightPanelJoystickTarget.GetSelectionInfo());
		}
	}

	public void OnUnitLevelButtonClick(bool shouldRefreshTooltip = true)
	{
		if (UnitLevelUpController.CanOpenUnitLevelUpView)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
			TPSingleton<UnitLevelUpView>.Instance.Open();
			unitLevel.RefreshButton(interactable: false);
			if (shouldRefreshTooltip)
			{
				unitLevel.UnitExperienceTooltipDisplayer.Refresh();
			}
		}
	}

	public void Open(bool instant = false, PlayableUnit playableUnit = null)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		if (IsOpened)
		{
			return;
		}
		IsOpened = true;
		this.OnCharacterSheetToggle?.Invoke(obj: true);
		if ((Object)(object)complexFontLocalizedParent != (Object)null)
		{
			complexFontLocalizedParent.TargetKey = "Default";
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
			complexFontLocalizedParent.TargetKey = "LeftPanel";
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
		}
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		UIManager.HideInfoPanels();
		Tweener obj = displayTween;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		if (instant)
		{
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, panelDeltaPosY);
		}
		else
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			TweenerCore<Vector2, Vector2, VectorOptions> obj2 = TweenSettingsExtensions.OnKill<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, panelDeltaPosY, 0.25f, true), (Ease)27), (TweenCallback)delegate
			{
				displayTween = null;
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			});
			object obj3 = _003C_003Ec._003C_003E9__94_1;
			if (obj3 == null)
			{
				TweenCallback val = delegate
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				};
				_003C_003Ec._003C_003E9__94_1 = val;
				obj3 = (object)val;
			}
			displayTween = (Tweener)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(obj2, (TweenCallback)obj3);
		}
		((Behaviour)canvas).enabled = true;
		ActivateSlots();
		if (playableUnit == null)
		{
			Refresh();
		}
		else
		{
			RefreshWith(playableUnit);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			closeJoystickTarget.ClearUnavailableNavigations();
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
		}
		SoundManager.PlayAudioClip(openClip, UIManager.PooledAudioSourceData);
	}

	public void OpenUnitDetails()
	{
		if (toggleUnitDetails.isOn)
		{
			toggleUnitDetails.isOn = false;
		}
		toggleUnitDetails.isOn = true;
		if ((Object)(object)complexFontLocalizedParent != (Object)null)
		{
			complexFontLocalizedParent.TargetKey = "CharacterDetails";
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
		}
	}

	public void OpenInventory()
	{
		if (TPSingleton<ToDoListView>.Instance.IsDisplayed)
		{
			TPSingleton<ToDoListView>.Instance.CloseInventoryNotification();
		}
		TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.MarkAllItemsAsSeen();
		((MonoBehaviour)this).StartCoroutine(TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.ResetScrollbarAfterAFrame());
		if (toggleInventory.isOn)
		{
			toggleInventory.isOn = false;
		}
		toggleInventory.isOn = true;
		if ((Object)(object)complexFontLocalizedParent != (Object)null)
		{
			complexFontLocalizedParent.TargetKey = "InventoryPanel";
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
		}
	}

	public void OpenPerkTree()
	{
		if (togglePerk.isOn)
		{
			togglePerk.isOn = false;
		}
		togglePerk.isOn = true;
		if ((Object)(object)complexFontLocalizedParent != (Object)null)
		{
			complexFontLocalizedParent.TargetKey = "PerksPanel";
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
		}
	}

	public void Refresh()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"No unit selected to refresh CharacterSheetPanel!", (CLogLevel)1, true, true);
		}
		else
		{
			RefreshWith(TileObjectSelectionManager.SelectedPlayableUnit);
		}
	}

	public void RefreshAvatar(PlayableUnit unit = null)
	{
		if (unit == null)
		{
			if (!TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"No unit selected!", (CLogLevel)1, true, true);
				return;
			}
			unit = TileObjectSelectionManager.SelectedPlayableUnit;
		}
		avatarImage.sprite = unit.UiSprite;
	}

	public void RefreshCharacterDetailsNotif(PlayableUnit playableUnit)
	{
		((Behaviour)characterDetailsNotif).enabled = playableUnit != null && playableUnit.StatsPoints > 0;
	}

	public void RefreshEquipmentSlotsValidity()
	{
		DraggedItem draggableItem = InventoryManager.InventoryView.DraggableItem;
		TheLastStand.Model.Item.Item obj = TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.FocusedInventorySlotView?.ItemSlot?.Item;
		bool isDragging = (Object)(object)draggableItem != (Object)null && draggableItem.Displayed;
		bool isHovering = obj != null;
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				equipmentSlot.Value[i].RefreshSlotValidity(isDragging, isHovering);
			}
		}
	}

	public void OnEquipmentSlotJoystickSelectionOver()
	{
		((MonoBehaviour)TPSingleton<HUDJoystickNavigationManager>.Instance).StartCoroutine(TPSingleton<HUDJoystickNavigationManager>.Instance.ToggleSlotSelectionCoroutine());
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				equipmentSlot.Value[i].RestoreJoystickNavigation();
			}
		}
		EventSystem.current.SetSelectedGameObject(((Component)TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace.InventorySlotView).gameObject);
		TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace = null;
		RefreshEquipmentSlotsValidity();
	}

	public void RefreshName(PlayableUnit playableUnit)
	{
		((TMP_Text)unitName).text = playableUnit.Name;
	}

	public void RefreshOpenedPage()
	{
		foreach (KeyValuePair<Toggle, TabbedPageView> tabPagePair in tabPagePairs)
		{
			if ((Object)(object)tabPagePair.Value != (Object)null && tabPagePair.Key.isOn)
			{
				tabPagePair.Value.IsDirty = true;
			}
		}
	}

	public void RefreshPerkAvailableNotif(PlayableUnit playableUnit)
	{
		((Behaviour)perkAvailableNotif).enabled = playableUnit != null && playableUnit.PerksPoints > 0 && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
	}

	public void RefreshPortrait(PlayableUnit playableUnit)
	{
		unitPortaitDetails.PlayableUnit = playableUnit;
		unitPortaitDetails.RefreshPortrait();
	}

	public void RefreshSkills(PlayableUnit playableUnit)
	{
		List<TheLastStand.Model.Skill.Skill> skills = playableUnit.PlayableUnitController.GetSkills();
		equippedSkillsDisplay.SetSkills(skills, playableUnit);
	}

	public void RefreshStats()
	{
		healthStatDisplay.Refresh();
		healthRegenStatDisplay.Refresh();
		manaStatDisplay.Refresh();
		manaRegenStatDisplay.Refresh();
		actionPointsStatDisplay.Refresh();
		movementPointsDisplay.Refresh();
		overallDamageStatDisplay.Refresh();
		criticalStatDisplay.Refresh();
		resistanceReductionStatDisplay.Refresh();
		accuracyStatDisplay.Refresh();
		blockStatDisplay.Refresh();
		armorStatDisplay.Refresh();
		resistanceStatDisplay.Refresh();
		dodgeStatDisplay.Refresh();
	}

	public void RefreshUnitHeader(PlayableUnit playableUnit)
	{
		RefreshName(playableUnit);
		unitLevel.PlayableUnit = playableUnit;
		unitLevel.Refresh();
	}

	public void SetBackgroundColorForSlot(ItemSlotDefinition.E_ItemSlotId slotId, Color color)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (slotsBackgroundImages != null && slotsBackgroundImages.TryGetValue(slotId, out var value))
		{
			((Graphic)value).color = color;
		}
	}

	private void ActivateSlots()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			foreach (EquipmentSlotView item in equipmentSlot.Value)
			{
				((Behaviour)item).enabled = true;
			}
		}
		foreach (InventorySlot inventorySlot in InventoryManager.InventoryView.Inventory.InventorySlots)
		{
			((Behaviour)inventorySlot.InventorySlotView).enabled = true;
		}
	}

	private IEnumerator CloseCoroutine()
	{
		HasClosedThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		HasClosedThisFrame = false;
	}

	private void DeactivateSlots()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			foreach (EquipmentSlotView item in equipmentSlot.Value)
			{
				item.OnPointerExit(null);
				((Behaviour)item).enabled = false;
			}
		}
		foreach (InventorySlot inventorySlot in InventoryManager.InventoryView.Inventory.InventorySlots)
		{
			inventorySlot.InventorySlotView.OnPointerExit(null);
			((Behaviour)inventorySlot.InventorySlotView).enabled = false;
		}
	}

	private void RefreshEquipment(PlayableUnit playableUnit)
	{
		hideHelmetView.Refresh(playableUnit);
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in equipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				EquipmentSlotView equipmentSlotView = equipmentSlot.Value[i];
				if (!playableUnit.EquipmentSlots.TryGetValue(equipmentSlot.Key, out var value) || i >= value.Count)
				{
					equipmentSlotView.ItemSlot = null;
				}
				else if (equipmentSlotView.ItemSlot != null && !((Component)equipmentSlotView).gameObject.activeInHierarchy)
				{
					((Component)equipmentSlotView).gameObject.SetActive(true);
				}
				equipmentSlotView.Refresh();
			}
		}
		List<TheLastStand.Model.Skill.Skill> skills = playableUnit.PlayableUnitController.GetSkills();
		equippedSkillsDisplay.SetSkills(skills, playableUnit);
		RefreshCharacterDetailsNotif(playableUnit);
		RefreshPerkAvailableNotif(playableUnit);
		RefreshOpenedPage();
	}

	private void RefreshLevelUpPanel()
	{
		if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = ((TileObjectSelectionManager.SelectedPlayableUnit.StatsPoints > 0) ? TileObjectSelectionManager.SelectedPlayableUnit.LevelUp : null);
			TPSingleton<UnitLevelUpView>.Instance.Reinitialize();
		}
	}

	private void RefreshNotifs(PlayableUnit playableUnit)
	{
		RefreshCharacterDetailsNotif(playableUnit);
		RefreshPerkAvailableNotif(playableUnit);
	}

	public void RefreshTabs()
	{
		((Selectable)toggleInventory).interactable = TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory();
		inventoryUnavailableParent.SetActive(!TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory());
		inventoryUnavailableText.SetActive(TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver);
		((Selectable)togglePerk).interactable = true;
		perkUnavailableParent.SetActive(false);
		perkUnavailableText.SetActive(false);
		changeSelectedHeroButtonsParent.SetActive(TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver);
	}

	private void RefreshWith(PlayableUnit playableUnit)
	{
		if (playableUnit == null)
		{
			CLoggerManager.Log((object)"Tried to refresh CharacterSheet with a null playableUnit. Skipping refresh.", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RefreshStats();
		RefreshPortrait(playableUnit);
		RefreshUnitHeader(playableUnit);
		RefreshTabs();
		RefreshAvatar(playableUnit);
		RefreshEquipment(playableUnit);
		RefreshSkills(playableUnit);
		RefreshNotifs(playableUnit);
		RefreshOpenedPage();
		if (PlayableUnitManager.StatTooltip.Displayed)
		{
			PlayableUnitManager.StatTooltip.Refresh();
		}
	}

	private void Update()
	{
		if (!(((StateMachine)ApplicationManager.Application).State is GameState))
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			if (!IsOpened)
			{
				return;
			}
			if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80))
			{
				CharacterSheetManager.CloseCharacterSheetPanel();
			}
			else if (InputManager.GetButtonDown(88))
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					SelectNeighbourTab(next: true);
				}
			}
			else if (InputManager.GetButtonDown(89) && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
			{
				SelectNeighbourTab(next: false);
			}
		}
		else if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet)
		{
			if (InputManager.GetButtonDown(29))
			{
				CharacterSheetManager.CloseCharacterSheetPanel();
			}
			else if (InputManager.GetButtonDown(80) && InputManager.IsLastControllerJoystick)
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace != null)
				{
					OnEquipmentSlotJoystickSelectionOver();
				}
				else if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
				{
					TPSingleton<UnitLevelUpView>.Instance.Close();
					TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(GameView.CharacterDetailsView.SecondaryAttributesHUDJoystickTarget.GetSelectionInfo());
				}
				else
				{
					CharacterSheetManager.CloseCharacterSheetPanel();
				}
			}
			else if (InputManager.GetButtonDown(0))
			{
				OnNextUnitButtonClick();
			}
			else if (InputManager.GetButtonDown(11))
			{
				OnPreviousUnitButtonClick();
			}
			else if (InputManager.GetButtonDown(10))
			{
				if (IsInventoryOpened)
				{
					CharacterSheetManager.CloseCharacterSheetPanel();
				}
				else if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
				{
					OpenInventory();
				}
			}
			else if (InputManager.GetButtonDown(16))
			{
				if (toggleUnitDetails.isOn)
				{
					CharacterSheetManager.CloseCharacterSheetPanel();
				}
				else
				{
					OpenUnitDetails();
				}
			}
			else if (InputManager.GetButtonDown(88))
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					SelectNeighbourTab(next: true);
				}
			}
			else if (InputManager.GetButtonDown(89))
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					SelectNeighbourTab(next: false);
				}
			}
			else if (InputManager.GetButtonDown(94))
			{
				if ((!IsInventoryOpened || InputManager.JoystickConfig.HUDNavigation.CanLevelUpInInventoryTab) && !IsPerksPanelOpened && !TPSingleton<UnitLevelUpView>.Instance.IsOpened && TileObjectSelectionManager.SelectedPlayableUnit.LevelPoints > 0 && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
					TPSingleton<UnitLevelUpView>.Instance.Open();
				}
			}
			else if (InputManager.GetButtonDown(79))
			{
				if (!IsPerksPanelOpened)
				{
					if ((Object)(object)FocusedEquipmentSlotView != (Object)null && InputManager.IsLastControllerJoystick)
					{
						FocusedEquipmentSlotView.OnJoystickSubmit();
					}
					return;
				}
				bool flag = false;
				if (IsPerksPanelOpened && InputManager.GetButtonDown(79) && (Object)(object)UnitPerkTreeView.SelectedPerk != (Object)null && !UnitPerkTreeView.SelectedPerk.Perk.Unlocked && UnitPerkTreeView.SelectedPerk.Perk.PerkTier.Available && UnitPerkTreeView.UnitPerkTree.CanBuyPerk())
				{
					unitPerkTreeView.OnTrainButtonClick();
					flag = true;
				}
				if (InputManager.GetButtonDown(94) && (!IsInventoryOpened || InputManager.JoystickConfig.HUDNavigation.CanLevelUpInInventoryTab) && !flag && !TPSingleton<UnitLevelUpView>.Instance.IsOpened && TileObjectSelectionManager.SelectedPlayableUnit.LevelPoints > 0 && TPSingleton<HUDJoystickNavigationManager>.Instance.InventorySlotToPlace == null)
				{
					TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
					TPSingleton<UnitLevelUpView>.Instance.Open();
				}
			}
			else
			{
				int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
				if (unitIndexHotkeyPressed != -1 && !InventoryManager.InventoryView.DraggableItem.Displayed && !TPSingleton<UnitLevelUpView>.Instance.IsProceedingToALevelUp)
				{
					TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed]);
					Refresh();
					RefreshLevelUpPanel();
				}
			}
		}
		else if (InputManager.GetButtonDown(10) && TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Shopping)
			{
				CharacterSheetManager.OpenCharacterSheetPanel();
				OpenInventory();
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.Shop.ShopController.CloseShopPanel(toAnotherPopup: true);
				CharacterSheetManager.OpenCharacterSheetPanel(fromAnotherPopup: true, TPSingleton<BuildingManager>.Instance.Shop.UnitToCompareIndex);
				OpenInventory();
			}
		}
		else if (CharacterSheetManager.CanOpenCharacterSheetPanel() && InputManager.GetButtonDown(16))
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			CharacterSheetManager.OpenCharacterSheetPanel();
			OpenUnitDetails();
		}
	}

	private void SelectNeighbourTab(bool next)
	{
		List<Toggle> list = tabPagePairs.Keys.Where((Toggle x) => ((Selectable)x).interactable).ToList();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (list[i].isOn)
			{
				list[i].isOn = false;
				int index = IntExtensions.Mod(i + (next ? 1 : (-1)), count);
				list[index].isOn = true;
				break;
			}
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(rightPanelJoystickTarget.GetSelectionInfo());
		}
	}

	private void TabbedPageToggle_ValueChanged(Toggle sender, bool value)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		((Component)sender).transform.localPosition = new Vector3(((Component)sender).transform.localPosition.x, value ? ((Component)tabPosOn).transform.localPosition.y : ((Component)tabPosOff).transform.localPosition.y, ((Component)sender).transform.localPosition.z);
		if (value)
		{
			tabPagePairs[sender].Open();
		}
		else
		{
			tabPagePairs[sender].Close();
		}
	}

	private IEnumerator LevelUpButtonJoystickDeselectedCoroutine()
	{
		yield return SharedYields.WaitForEndOfFrame;
		if ((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)null)
		{
			if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<UnitLevelUpView>.Instance.HudTarget.GetSelectionInfo());
			}
		}
		else
		{
			EventSystem.current.SetSelectedGameObject(((Component)GameView.CharacterDetailsView.LevelUpButtonDisabledTarget).gameObject);
		}
	}
}
