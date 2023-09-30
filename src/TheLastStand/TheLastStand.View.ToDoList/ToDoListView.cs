using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Controller;
using TheLastStand.Controller.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListView : TPSingleton<ToDoListView>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Predicate<BuildingAction> _003C_003E9__52_0;

		public static Predicate<BuildingAction> _003C_003E9__53_0;

		public static TweenCallback _003C_003E9__81_2;

		public static TweenCallback _003C_003E9__91_2;

		internal bool _003COnWorkersButtonClick_003Eb__52_0(BuildingAction buildingAction)
		{
			int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(buildingAction.BuildingActionDefinition);
			if (modifiedWorkersCost > 0 && modifiedWorkersCost <= TPSingleton<ResourceManager>.Instance.Workers)
			{
				return buildingAction.UsesPerTurnRemaining != 0;
			}
			return false;
		}

		internal bool _003COnFreeActionsButtonClick_003Eb__53_0(BuildingAction buildingAction)
		{
			if (ResourceManager.GetModifiedWorkersCost(buildingAction.BuildingActionDefinition) == 0)
			{
				return buildingAction.UsesPerTurnRemaining != 0;
			}
			return false;
		}

		internal void _003CFold_003Eb__81_2()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}

		internal void _003CUnfold_003Eb__91_2()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	[SerializeField]
	private Canvas toDoListCanvas;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform toDoListRectTransform;

	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private RectTransform bgRectTransform;

	[SerializeField]
	private RectTransform scrollViewRectTransform;

	[SerializeField]
	private ScrollRect toDoListScrollRect;

	[SerializeField]
	private GameObject scrollTopButton;

	[SerializeField]
	private GameObject scrollBotButton;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	[Range(0f, 1000f)]
	[Tooltip("This is the value in pixels used to move the content of the scrollView")]
	private float scrollButtonsSensitivity = 100f;

	[SerializeField]
	private ToDoListLevelUpNotificationView toDoListUnitLevelUpNotifications;

	[SerializeField]
	private ToDoListProductionNotificationView toDoListProductionNotificationView;

	[SerializeField]
	private ToDoListGoldNotificationView toDoListGoldNotificationView;

	[SerializeField]
	private ToDoListWorkersNotificationView toDoListWorkersNotificationView;

	[SerializeField]
	private ToDoListNotificationView toDoListMaterialsNotificationView;

	[SerializeField]
	private ToDoListWavesNotificationView toDoListWavesNotificationView;

	[SerializeField]
	private ToDoListPositionNotificationView toDoListPositionNotificationView;

	[SerializeField]
	private ToDoListInventoryNotificationView toDoListInventoryNotificationView;

	[SerializeField]
	private ToDoListMetaShopsNotificationView toDoListMetaShopsNotificationView;

	[SerializeField]
	private RectTransform[] displacedRectTransforms;

	[SerializeField]
	[Tooltip("That will be the position offset of the resources panel when NOT folded")]
	private float displacedFoldRectTransformsOrigin = 12f;

	[SerializeField]
	[Tooltip("That will be the position offset of the resources panel when folded")]
	private float displacedFoldRectTransformsDestination = 60f;

	[SerializeField]
	private RectTransform foldButtonRectTransform;

	[SerializeField]
	private Button foldButton;

	[SerializeField]
	[Tooltip("That will be the position offset of the todo list panel when folded")]
	private float foldedPosition = -200f;

	[SerializeField]
	private RectTransform todoListFoldButtonRectTransform;

	[SerializeField]
	private RectTransform todoListFoldButtonNoScrollTransform;

	[SerializeField]
	private RectTransform todoListFoldButtonCanScrollTransform;

	[SerializeField]
	[Range(0f, 2f)]
	private float unfoldDuration = 0.4f;

	[SerializeField]
	private Ease unfoldEasing = (Ease)8;

	[SerializeField]
	[Range(0f, 2f)]
	private float foldDuration = 0.4f;

	[SerializeField]
	private Ease foldEasing = (Ease)9;

	[SerializeField]
	private Selectable[] notificationsButtons;

	private int currentSpawnWaveFeedbackTargetIndex;

	private bool isFolded;

	private bool canScroll;

	private float foldDisplacementPosition;

	private Tween displacedFoldTween;

	private Tween todoListFoldTween;

	public bool IsDisplayed { get; private set; } = true;


	public void CloseInventoryNotification()
	{
		toDoListInventoryNotificationView.Refresh();
		RefreshToDoList();
	}

	public Selectable GetFirstActiveSelectable()
	{
		if (!IsDisplayed)
		{
			return null;
		}
		for (int i = 0; i < notificationsButtons.Length; i++)
		{
			if (((Component)notificationsButtons[i]).gameObject.activeInHierarchy && notificationsButtons[i].IsInteractable())
			{
				return notificationsButtons[i];
			}
		}
		return null;
	}

	public Selectable GetLastActiveSelectable()
	{
		if (!IsDisplayed)
		{
			return null;
		}
		for (int num = notificationsButtons.Length - 1; num >= 0; num--)
		{
			if (((Component)notificationsButtons[num]).gameObject.activeInHierarchy && notificationsButtons[num].IsInteractable())
			{
				return notificationsButtons[num];
			}
		}
		return null;
	}

	public Selectable GetFoldButton()
	{
		return (Selectable)(object)foldButton;
	}

	public void Hide()
	{
		if (IsDisplayed)
		{
			if ((Object)(object)simpleFontLocalizedParent != (Object)null)
			{
				((FontLocalizedParent)simpleFontLocalizedParent).UnregisterChilds();
			}
			HideNotifications();
			IsDisplayed = false;
			((Behaviour)toDoListCanvas).enabled = false;
		}
	}

	private void HideNotifications()
	{
		toDoListUnitLevelUpNotifications.Display(show: false);
		toDoListProductionNotificationView.Display(show: false);
		toDoListGoldNotificationView.Display(show: false);
		toDoListWorkersNotificationView.Display(show: false);
		toDoListMaterialsNotificationView.Display(show: false);
		toDoListWavesNotificationView.Display(show: false);
		toDoListPositionNotificationView.Display(show: false);
		toDoListInventoryNotificationView.Display(show: false);
		toDoListMetaShopsNotificationView.Display(show: false);
	}

	public void OnNotificationButtonSelected(BetterButton button)
	{
		Transform transform = ((Component)button).transform;
		GUIHelpers.AdjustScrollViewToFocusedItem((RectTransform)(object)((transform is RectTransform) ? transform : null), scrollViewRectTransform, scrollbar, 0.01f, 0.01f, (float?)null);
	}

	public void OnWorkersButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != 0)
		{
			ConstructionManager.ExitConstructionMode();
		}
		Predicate<BuildingAction> filter = delegate(BuildingAction buildingAction)
		{
			int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(buildingAction.BuildingActionDefinition);
			return modifiedWorkersCost > 0 && modifiedWorkersCost <= TPSingleton<ResourceManager>.Instance.Workers && buildingAction.UsesPerTurnRemaining != 0;
		};
		List<TheLastStand.Model.Building.Building> buildingsWithActions = GetBuildingsWithActions(filter);
		if (buildingsWithActions.Count > 0)
		{
			SelectNextBuilding(buildingsWithActions);
		}
	}

	public void OnFreeActionsButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != 0)
		{
			ConstructionManager.ExitConstructionMode();
		}
		Predicate<BuildingAction> filter = (BuildingAction buildingAction) => ResourceManager.GetModifiedWorkersCost(buildingAction.BuildingActionDefinition) == 0 && buildingAction.UsesPerTurnRemaining != 0;
		List<TheLastStand.Model.Building.Building> buildingsWithActions = GetBuildingsWithActions(filter);
		if (buildingsWithActions.Count > 0)
		{
			SelectNextBuilding(buildingsWithActions);
		}
	}

	public void OnBotButtonClick()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		toDoListScrollRect.verticalScrollbar.value = Mathf.Clamp01(toDoListScrollRect.verticalScrollbar.value - scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	public void OnFoldButtonClick()
	{
		if (isFolded)
		{
			Unfold();
		}
		else
		{
			Fold();
		}
	}

	public void OnGoldConstructionButtonClick()
	{
		if (GameController.CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Production);
			ConstructionManager.SetState(Construction.E_State.ChooseBuilding);
		}
	}

	public void OnInnButtonClick()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Recruitment && RecruitmentController.CanOpenRecruitmentPanel())
		{
			RecruitmentController.OpenRecruitmentPanel();
		}
	}

	public void OnInventoryButtonClick()
	{
		if (TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory())
		{
			if (!TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[0]);
			}
			CharacterSheetManager.OpenCharacterSheetPanel();
			TPSingleton<CharacterSheetPanel>.Instance.OpenInventory();
		}
	}

	public void OnLevelUpButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != 0)
		{
			ConstructionManager.ExitConstructionMode();
		}
		PlayableUnit playableUnit = null;
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].StatsPoints > 0)
			{
				playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i];
				break;
			}
		}
		for (int j = 0; j < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; j++)
		{
			if (TileObjectSelectionManager.SelectedUnit != playableUnit)
			{
				PlayableUnitManager.SelectNextUnit();
			}
		}
		if (UnitLevelUpController.CanOpenUnitLevelUpView)
		{
			TPSingleton<UnitLevelUpView>.Instance.UnitLevelUp = TileObjectSelectionManager.SelectedPlayableUnit.LevelUp;
			TPSingleton<UnitLevelUpView>.Instance.Open();
		}
	}

	public void OnMaterialsButtonButtonClick()
	{
		if (GameController.CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive))
		{
			ConstructionManager.OpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive);
			ConstructionManager.SetState(Construction.E_State.ChooseBuilding);
		}
	}

	public void OnDarkShopButtonButtonClick()
	{
		TPSingleton<MetaShopsManager>.Instance.OpenMetaShop(darkShop: true);
	}

	public void OnLightShopButtonButtonClick()
	{
		TPSingleton<MetaShopsManager>.Instance.OpenMetaShop(darkShop: false);
	}

	public void OnProductionButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != 0 && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction)
		{
			ConstructionManager.ExitConstructionMode();
		}
		GameController.SetState(Game.E_State.ProductionReport);
		TPSingleton<ProductionReportPanel>.Instance.Open();
	}

	public void OnShopButtonClick()
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel())
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.OpenShopPanel(fromAnotherPopup: false, TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit));
		}
	}

	public void OnTopButtonClick()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		toDoListScrollRect.verticalScrollbar.value = Mathf.Clamp01(toDoListScrollRect.verticalScrollbar.value + scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	public void OnUnitPositionClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != 0)
		{
			ConstructionManager.ExitConstructionMode();
		}
		List<PlayableUnit> list = new List<PlayableUnit>();
		int i = 0;
		for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
		{
			if (!TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].MovedThisDay || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].OriginTile.HasFog)
			{
				list.Add(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i]);
			}
		}
		if (!TileObjectSelectionManager.HasPlayableUnitSelected || !list.Contains(TileObjectSelectionManager.SelectedPlayableUnit))
		{
			TileObjectSelectionManager.SetSelectedPlayableUnit(list[0], focusCameraOnUnit: true);
			return;
		}
		int num = list.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
		TileObjectSelectionManager.SetSelectedPlayableUnit(list[++num % list.Count], focusCameraOnUnit: true);
	}

	public void OnWaveIncomingButtonClick()
	{
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != 0)
		{
			ConstructionManager.ExitConstructionMode();
		}
		for (int i = 0; i < SpawnDirectionsDefinition.OrderedDirections.Count; i++)
		{
			currentSpawnWaveFeedbackTargetIndex++;
			if (currentSpawnWaveFeedbackTargetIndex == SpawnDirectionsDefinition.OrderedDirections.Count)
			{
				currentSpawnWaveFeedbackTargetIndex = 0;
			}
			SpawnWaveView.SpawnWaveArrowPair spawnWaveArrowPair = SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks.FirstOrDefault((SpawnWaveView.SpawnWaveArrowPair x) => x.SpawnWaveDetailedZone.IsCentralZone() && x.CentralSpawnDirection == SpawnDirectionsDefinition.OrderedDirections[currentSpawnWaveFeedbackTargetIndex]);
			if ((Object)(object)spawnWaveArrowPair.SpawnWaveViewPreviewFeedback != (Object)null)
			{
				SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
				if (currentSpawnWave != null && currentSpawnWave.RotatedProportionPerDirection.ContainsKey(spawnWaveArrowPair.CentralSpawnDirection))
				{
					ACameraView.MoveTo(spawnWaveArrowPair.SpawnWaveViewPreviewFeedback.CamTarget);
					break;
				}
			}
		}
	}

	[ContextMenu("Refresh all notifications")]
	public void RefreshAllNotifications()
	{
		toDoListUnitLevelUpNotifications.Refresh();
		toDoListProductionNotificationView.Refresh();
		toDoListGoldNotificationView.Refresh();
		toDoListWorkersNotificationView.Refresh();
		toDoListMaterialsNotificationView.Refresh();
		toDoListWavesNotificationView.Refresh();
		toDoListPositionNotificationView.Refresh();
		toDoListInventoryNotificationView.Refresh();
		toDoListMetaShopsNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshActionPointsNotification()
	{
	}

	public void RefreshGoldNotification()
	{
		toDoListGoldNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshInventoryNotification()
	{
		toDoListInventoryNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshMetaShopsNotification()
	{
		toDoListMetaShopsNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshProductionNotification()
	{
		toDoListProductionNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshPositionNotification()
	{
		toDoListPositionNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshSpawnWavePositionView()
	{
		toDoListWavesNotificationView.Refresh();
		RefreshToDoList();
	}

	public void RefreshToDoList()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
		Rect rect = contentRectTransform.rect;
		float height = ((Rect)(ref rect)).height;
		rect = toDoListRectTransform.rect;
		canScroll = height > ((Rect)(ref rect)).height - Mathf.Abs(scrollViewRectTransform.offsetMax.y) - Mathf.Abs(scrollViewRectTransform.offsetMin.y);
		toDoListScrollRect.vertical = canScroll;
		scrollTopButton.SetActive(canScroll && !InputManager.IsLastControllerJoystick);
		scrollBotButton.SetActive(canScroll && !InputManager.IsLastControllerJoystick);
		((Transform)todoListFoldButtonRectTransform).localPosition = (canScroll ? ((Transform)todoListFoldButtonCanScrollTransform).localPosition : ((Transform)todoListFoldButtonNoScrollTransform).localPosition);
		if (!canScroll)
		{
			toDoListScrollRect.verticalScrollbar.value = 1f;
		}
		RectTransform obj = bgRectTransform;
		float x = bgRectTransform.sizeDelta.x;
		rect = contentRectTransform.rect;
		float num = ((Rect)(ref rect)).height + Mathf.Abs(scrollViewRectTransform.offsetMax.y) + Mathf.Abs(scrollViewRectTransform.offsetMin.y);
		rect = toDoListRectTransform.rect;
		obj.sizeDelta = new Vector2(x, Mathf.Min(num, ((Rect)(ref rect)).height));
		RefreshJoystickNavigation();
	}

	public void RefreshUnitLevelUpNotification()
	{
		toDoListUnitLevelUpNotifications.Refresh();
		RefreshToDoList();
	}

	public void RefreshWorkersNotification()
	{
		toDoListWorkersNotificationView.Refresh();
		RefreshToDoList();
	}

	public void Show()
	{
		if (!IsDisplayed && UIManager.DebugToggleUI != false && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			if ((Object)(object)simpleFontLocalizedParent != (Object)null)
			{
				((FontLocalizedParent)simpleFontLocalizedParent).RegisterChilds();
			}
			toDoListWavesNotificationView.Display(show: true);
			IsDisplayed = true;
			((Behaviour)toDoListCanvas).enabled = true;
			RefreshAllNotifications();
		}
	}

	public void SwitchRaycastTargetState(bool state)
	{
		((Behaviour)graphicRaycaster).enabled = state;
	}

	private void Fold()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		if (isFolded)
		{
			return;
		}
		Tween obj = todoListFoldTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = displacedFoldTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		todoListFoldTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(toDoListRectTransform, foldedPosition, foldDuration, false), foldEasing), "FoldTween", (Component)(object)this);
		if (displacedRectTransforms != null && displacedRectTransforms.Length != 0)
		{
			foldDisplacementPosition = displacedRectTransforms[0].anchoredPosition.x;
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenerCore<float, float, FloatOptions> obj3 = TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => foldDisplacementPosition), (DOSetter<float>)delegate(float x)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < displacedRectTransforms.Length; i++)
			{
				displacedRectTransforms[i].anchoredPosition = new Vector2(x, displacedRectTransforms[i].anchoredPosition.y);
			}
		}, displacedFoldRectTransformsDestination, foldDuration), "DisplacedFoldTween", (Component)(object)this);
		object obj4 = _003C_003Ec._003C_003E9__81_2;
		if (obj4 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__81_2 = val;
			obj4 = (object)val;
		}
		displacedFoldTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj3, (TweenCallback)obj4);
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).UnregisterChilds();
		}
		((Transform)foldButtonRectTransform).localScale = new Vector3(1f, 1f, 1f);
		isFolded = true;
	}

	private List<TheLastStand.Model.Building.Building> GetBuildingsWithActions(Predicate<BuildingAction> filter)
	{
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list2 = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list3 = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list4 = new List<TheLastStand.Model.Building.Building>();
		List<TheLastStand.Model.Building.Building> list5 = new List<TheLastStand.Model.Building.Building>();
		int i = 0;
		for (int count = TPSingleton<BuildingManager>.Instance.Buildings.Count; i < count; i++)
		{
			TheLastStand.Model.Building.Building building = TPSingleton<BuildingManager>.Instance.Buildings[i];
			if (building.ProductionModule?.BuildingActions == null || !building.ProductionModule.BuildingActions.Exists(filter))
			{
				continue;
			}
			if (building.ConstructionModule.CostsGold)
			{
				if (building.BuildingDefinition.ProductionModuleDefinition?.BuildingGaugeEffectDefinition != null)
				{
					list3.Add(building);
				}
				else
				{
					list2.Add(building);
				}
			}
			else if (building.IsDefensive)
			{
				list4.Add(building);
			}
			else
			{
				list5.Add(building);
			}
		}
		list.AddRange(list2);
		list.AddRange(list3);
		list.AddRange(list4);
		list.AddRange(list5);
		return list;
	}

	private void RefreshJoystickNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < notificationsButtons.Length; i++)
		{
			if (((Component)notificationsButtons[i]).gameObject.activeInHierarchy && notificationsButtons[i].IsInteractable())
			{
				list.Add(notificationsButtons[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			SelectableExtensions.SetMode(list[j], (Mode)4);
			if (j > 0)
			{
				SelectableExtensions.SetSelectOnUp(list[j], list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				SelectableExtensions.SetSelectOnDown(list[j], list[j + 1]);
			}
		}
		SelectableExtensions.SetMode((Selectable)(object)foldButton, (Mode)4);
		if (list.Count > 0)
		{
			SelectableExtensions.SetSelectOnDown((Selectable)(object)foldButton, list[0]);
			SelectableExtensions.SetSelectOnUp(list[0], (Selectable)(object)foldButton);
		}
		else
		{
			SelectableExtensions.SetSelectOnDown((Selectable)(object)foldButton, (Selectable)null);
		}
	}

	private void OnDisable()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChange;
			((UnityEvent<float>)(object)TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent).RemoveListener((UnityAction<float>)OnUIScaleChanged);
		}
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).UnregisterChilds();
		}
		MetaConditionManager.OnConditionsRefreshed -= RefreshAllNotifications;
	}

	private void OnEnable()
	{
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChange;
		((UnityEvent<float>)(object)TPSingleton<SettingsManager>.Instance.UiScaleSettingChangeEvent).AddListener((UnityAction<float>)OnUIScaleChanged);
		MetaConditionManager.OnConditionsRefreshed += RefreshAllNotifications;
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).RegisterChilds();
		}
	}

	private void OnResolutionChange(Resolution resolution)
	{
		RefreshToDoList();
	}

	private void OnUIScaleChanged(float scale)
	{
		((MonoBehaviour)this).StartCoroutine(OnUIScaleChangedCoroutine());
	}

	private IEnumerator OnUIScaleChangedCoroutine()
	{
		yield return null;
		RefreshToDoList();
	}

	private void SelectNextBuilding(List<TheLastStand.Model.Building.Building> buildings)
	{
		int num = ((TileObjectSelectionManager.SelectedBuilding != null && buildings.Contains(TileObjectSelectionManager.SelectedBuilding)) ? (buildings.IndexOf(TileObjectSelectionManager.SelectedBuilding) + 1) : 0);
		if (num == buildings.Count)
		{
			num = 0;
		}
		TileObjectSelectionManager.SelectBuilding(buildings[num], focusCameraOnBuilding: true);
	}

	private void Start()
	{
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).RegisterChilds();
		}
	}

	private void Unfold()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		if (!isFolded)
		{
			return;
		}
		Tween obj = todoListFoldTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = displacedFoldTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		todoListFoldTween = (Tween)(object)TweenExtensions.SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(toDoListRectTransform, 0f, unfoldDuration, false), unfoldEasing), "UnfoldTween", (Component)(object)this);
		if (displacedRectTransforms != null && displacedRectTransforms.Length != 0)
		{
			foldDisplacementPosition = displacedRectTransforms[0].anchoredPosition.x;
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenerCore<float, float, FloatOptions> obj3 = TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => foldDisplacementPosition), (DOSetter<float>)delegate(float x)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < displacedRectTransforms.Length; i++)
			{
				displacedRectTransforms[i].anchoredPosition = new Vector2(x, displacedRectTransforms[i].anchoredPosition.y);
			}
		}, displacedFoldRectTransformsOrigin, foldDuration), "DisplacedUnfoldTween", (Component)(object)this);
		object obj4 = _003C_003Ec._003C_003E9__91_2;
		if (obj4 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__91_2 = val;
			obj4 = (object)val;
		}
		displacedFoldTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj3, (TweenCallback)obj4);
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).RegisterChilds();
		}
		((Transform)foldButtonRectTransform).localScale = new Vector3(-1f, 1f, 1f);
		isFolded = false;
	}
}
