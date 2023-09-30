using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller;
using TheLastStand.Controller.Meta;
using TheLastStand.Controller.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel;

public class BottomLeftPanel : MonoBehaviour
{
	[Serializable]
	public struct BottomLeftButton
	{
		public BetterButton Button;

		public Image Image;

		public GameObject Chains;

		public List<Selectable> NavigationUp;

		public List<Selectable> NavigationDown;
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject joystickHUDNavigationIcon;

	[SerializeField]
	private BottomLeftButton inventoryButton;

	[SerializeField]
	private BottomLeftButton characterSheetButton;

	[SerializeField]
	private BottomLeftButton shopButton;

	[SerializeField]
	private BottomLeftButton constructionButton;

	[SerializeField]
	private BottomLeftButton metaShopsButton;

	[SerializeField]
	private BottomLeftButton innButton;

	[SerializeField]
	private Button settingsButton;

	[SerializeField]
	[Range(0f, 1f)]
	private float nonInteractableButtonAlpha = 0.33f;

	[SerializeField]
	private BuildingDefinition.E_ConstructionCategory constructionCategoryOnOpen = BuildingDefinition.E_ConstructionCategory.Production;

	[SerializeField]
	private CancelMovementPanel cancelMovementPanel;

	[SerializeField]
	private Image noSelectionBackgroundImage;

	[SerializeField]
	private Image selectionBackgroundImage;

	[SerializeField]
	private Image constructionBackgroundImage;

	private List<Selectable> characterSheetNavigationLeft;

	private List<Selectable> characterSheetNavigationDown;

	private List<Selectable> inventoryNavigationRight;

	private List<Selectable> inventoryNavigationDown;

	private List<Selectable> metaShopsNavigationRight;

	private List<Selectable> metaShopsNavigationUp;

	private List<Selectable> metaShopsNavigationDown;

	private List<Selectable> innNavigationLeft;

	private List<Selectable> innNavigationUp;

	private List<Selectable> innNavigationDown;

	private List<Selectable> shopNavigationRight;

	private List<Selectable> shopNavigationDown;

	private List<Selectable> shopNavigationUp;

	private List<Selectable> constructionNavigationLeft;

	private List<Selectable> constructionNavigationDown;

	private List<Selectable> constructionNavigationUp;

	private List<Selectable> settingsNavigationUp;

	public CancelMovementPanel CancelMovementPanel => cancelMovementPanel;

	public Canvas Canvas => canvas;

	public void OnGameStateChange(Game.E_State state)
	{
		if ((Object)(object)joystickHUDNavigationIcon != (Object)null)
		{
			switch (state)
			{
			case Game.E_State.Management:
			case Game.E_State.UnitPreparingSkill:
			case Game.E_State.BuildingPreparingSkill:
			case Game.E_State.Construction:
			case Game.E_State.BuildingUpgrade:
			case Game.E_State.Wait:
				joystickHUDNavigationIcon.SetActive(true);
				break;
			default:
				joystickHUDNavigationIcon.SetActive(false);
				break;
			}
		}
	}

	public void OnCancelMovementButtonClick()
	{
		PlayableUnitManager.UndoLastCommand();
	}

	public void OnSettingsButtonClick()
	{
		if (SettingsManager.CanOpenSettings())
		{
			ApplicationManager.Application.ApplicationController.SetState("Settings");
		}
	}

	private void OnSelectionChange()
	{
		RefreshConstructionBackground();
		((Behaviour)selectionBackgroundImage).enabled = TileObjectSelectionManager.HasAnythingSelected;
		((Behaviour)noSelectionBackgroundImage).enabled = !((Behaviour)selectionBackgroundImage).enabled && !((Behaviour)constructionBackgroundImage).enabled;
	}

	public void Refresh()
	{
		if (UIManager.DebugToggleUI == false || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits))
		{
			((Behaviour)canvas).enabled = false;
			return;
		}
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Management:
		case Game.E_State.CharacterSheet:
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.BuildingPreparingAction:
		case Game.E_State.BuildingExecutingAction:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.BuildingExecutingSkill:
		case Game.E_State.Construction:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
			((Behaviour)canvas).enabled = true;
			cancelMovementPanel.Refresh();
			RefreshButtons();
			RefreshJoystickNavigation();
			OnSelectionChange();
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.Refresh(fullRefresh: true);
			break;
		case Game.E_State.Wait:
			((Behaviour)canvas).enabled = true;
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				cancelMovementPanel.Refresh();
				RefreshButtons();
				RefreshJoystickNavigation();
			}
			OnSelectionChange();
			break;
		default:
			((Behaviour)canvas).enabled = false;
			break;
		}
	}

	public void RefreshConstructionBackground()
	{
		bool flag = TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.PlaceBuilding;
		if (flag)
		{
			((Behaviour)noSelectionBackgroundImage).enabled = true;
		}
		((Behaviour)constructionBackgroundImage).enabled = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction && !flag;
	}

	private void OnCharacterSheetButtonClick()
	{
		if (CharacterSheetManager.CanOpenCharacterSheetPanel())
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			CharacterSheetManager.OpenCharacterSheetPanel();
			TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
		}
	}

	private void OnConstructionButtonClick()
	{
		ConstructionManager.OpenConstructionMode(constructionCategoryOnOpen);
	}

	private void OnInventoryButtonClick()
	{
		TileObjectSelectionManager.EnsureUnitSelection();
		CharacterSheetManager.OpenCharacterSheetPanel();
		TPSingleton<CharacterSheetPanel>.Instance.OpenInventory();
	}

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		((UnityEvent)((Button)inventoryButton.Button).onClick).AddListener(new UnityAction(OnInventoryButtonClick));
		((UnityEvent)((Button)characterSheetButton.Button).onClick).AddListener(new UnityAction(OnCharacterSheetButtonClick));
		((UnityEvent)((Button)shopButton.Button).onClick).AddListener(new UnityAction(ShopManager.OpenShop));
		((UnityEvent)((Button)constructionButton.Button).onClick).AddListener(new UnityAction(OnConstructionButtonClick));
		((UnityEvent)((Button)metaShopsButton.Button).onClick).AddListener(new UnityAction(TPSingleton<MetaShopsManager>.Instance.OpenShops));
		((UnityEvent)((Button)innButton.Button).onClick).AddListener(new UnityAction(RecruitmentController.OpenRecruitmentPanel));
		InitJoystickNavigation();
	}

	private void OnDestroy()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		((UnityEvent)((Button)inventoryButton.Button).onClick).RemoveListener(new UnityAction(OnInventoryButtonClick));
		((UnityEvent)((Button)characterSheetButton.Button).onClick).RemoveListener(new UnityAction(OnCharacterSheetButtonClick));
		((UnityEvent)((Button)shopButton.Button).onClick).RemoveListener(new UnityAction(ShopManager.OpenShop));
		((UnityEvent)((Button)constructionButton.Button).onClick).RemoveListener(new UnityAction(OnConstructionButtonClick));
		if (TPSingleton<MetaShopsManager>.Exist())
		{
			((UnityEvent)((Button)metaShopsButton.Button).onClick).RemoveListener(new UnityAction(TPSingleton<MetaShopsManager>.Instance.OpenShops));
		}
		((UnityEvent)((Button)innButton.Button).onClick).RemoveListener(new UnityAction(RecruitmentController.OpenRecruitmentPanel));
	}

	private void RefreshButtons()
	{
		RefreshInventoryButton();
		RefreshCharacterSheetButton();
		RefreshMetaShopsButton();
		RefreshInnButton();
		RefreshShopButton();
		RefreshConstructionButton();
	}

	private void RefreshButton(BottomLeftButton button, bool interactable)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)button.Button).interactable = interactable;
		((Graphic)button.Image).color = new Color(1f, 1f, 1f, interactable ? 1f : nonInteractableButtonAlpha);
		button.Chains.SetActive(!interactable);
	}

	private void RefreshButtonNavigationLeft(Selectable button, IEnumerable<Selectable> targets)
	{
		SelectableExtensions.SetSelectOnLeft(button, targets.FirstOrDefault((Func<Selectable, bool>)((Selectable t) => ((Component)t).gameObject.activeInHierarchy && t.IsInteractable())));
	}

	private void RefreshButtonNavigationRight(Selectable button, IEnumerable<Selectable> targets)
	{
		SelectableExtensions.SetSelectOnRight(button, targets.FirstOrDefault((Func<Selectable, bool>)((Selectable t) => ((Component)t).gameObject.activeInHierarchy && t.IsInteractable())));
	}

	private void RefreshButtonNavigationUp(Selectable button, IEnumerable<Selectable> targets)
	{
		SelectableExtensions.SetSelectOnUp(button, targets.FirstOrDefault((Func<Selectable, bool>)((Selectable t) => ((Component)t).gameObject.activeInHierarchy && t.IsInteractable())));
	}

	private void RefreshButtonNavigationDown(Selectable button, IEnumerable<Selectable> targets)
	{
		SelectableExtensions.SetSelectOnDown(button, targets.FirstOrDefault((Func<Selectable, bool>)((Selectable t) => ((Component)t).gameObject.activeInHierarchy && t.IsInteractable())));
	}

	private void RefreshCharacterSheetButton()
	{
		RefreshButton(characterSheetButton, CharacterSheetManager.CanOpenCharacterSheetPanel());
	}

	private void RefreshInventoryButton()
	{
		RefreshButton(inventoryButton, TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.CanOpenInventory());
	}

	private void RefreshMetaShopsButton()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap && !MetaShopsManager.OraculumForceAccess)
		{
			((Component)metaShopsButton.Button).gameObject.SetActive(false);
			metaShopsButton.Chains.SetActive(false);
		}
		else
		{
			((Component)metaShopsButton.Button).gameObject.SetActive(true);
			metaShopsButton.Chains.SetActive(true);
			RefreshButton(metaShopsButton, MetaShopsManager.CanOpenShops() && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management);
		}
	}

	private void RefreshInnButton()
	{
		List<UnlockBuildingMetaEffectDefinition> list = null;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockBuildingMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			list = effects.ToList();
		}
		if (MetaUpgradesManager.IsThisBuildingUnlockedByDefault("Inn") || (list != null && list.Any((UnlockBuildingMetaEffectDefinition x) => x.BuildingId == "Inn")))
		{
			((Component)innButton.Button).gameObject.SetActive(true);
			RefreshButton(innButton, RecruitmentController.CanOpenRecruitmentPanel());
		}
		else
		{
			((Component)innButton.Button).gameObject.SetActive(false);
			innButton.Chains.SetActive(false);
		}
	}

	private void RefreshShopButton()
	{
		RefreshButton(shopButton, TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel());
	}

	private void RefreshConstructionButton()
	{
		RefreshButton(constructionButton, TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Construction && GameController.CanOpenConstructionMode(BuildingDefinition.E_ConstructionCategory.Defensive));
	}

	private void RefreshJoystickNavigation()
	{
		Selectable lastActiveSelectable = TPSingleton<ToDoListView>.Instance.GetLastActiveSelectable();
		Selectable val = (InputManager.JoystickConfig.HUDNavigation.SelectLastButtonFromBottomPanel ? lastActiveSelectable : TPSingleton<ToDoListView>.Instance.GetFirstActiveSelectable());
		if ((Object)(object)lastActiveSelectable != (Object)null)
		{
			SelectableExtensions.SetSelectOnDown(lastActiveSelectable, (Selectable)(object)inventoryButton.Button);
		}
		RefreshButtonNavigationLeft((Selectable)(object)characterSheetButton.Button, characterSheetNavigationLeft);
		RefreshButtonNavigationDown((Selectable)(object)characterSheetButton.Button, characterSheetNavigationDown);
		SelectableExtensions.SetSelectOnUp((Selectable)(object)characterSheetButton.Button, val);
		RefreshButtonNavigationRight((Selectable)(object)inventoryButton.Button, inventoryNavigationRight);
		RefreshButtonNavigationDown((Selectable)(object)inventoryButton.Button, inventoryNavigationDown);
		SelectableExtensions.SetSelectOnUp((Selectable)(object)inventoryButton.Button, val);
		RefreshButtonNavigationRight((Selectable)(object)metaShopsButton.Button, metaShopsNavigationRight);
		RefreshButtonNavigationUp((Selectable)(object)metaShopsButton.Button, metaShopsNavigationUp);
		RefreshButtonNavigationDown((Selectable)(object)metaShopsButton.Button, metaShopsNavigationDown);
		RefreshButtonNavigationLeft((Selectable)(object)innButton.Button, innNavigationLeft);
		RefreshButtonNavigationUp((Selectable)(object)innButton.Button, innNavigationUp);
		RefreshButtonNavigationDown((Selectable)(object)innButton.Button, innNavigationDown);
		RefreshButtonNavigationRight((Selectable)(object)shopButton.Button, shopNavigationRight);
		RefreshButtonNavigationDown((Selectable)(object)shopButton.Button, shopNavigationDown);
		RefreshButtonNavigationUp((Selectable)(object)shopButton.Button, shopNavigationUp);
		RefreshButtonNavigationUp((Selectable)(object)constructionButton.Button, constructionNavigationUp);
		RefreshButtonNavigationLeft((Selectable)(object)constructionButton.Button, constructionNavigationLeft);
		RefreshButtonNavigationDown((Selectable)(object)constructionButton.Button, constructionNavigationDown);
		RefreshButtonNavigationUp((Selectable)(object)settingsButton, settingsNavigationUp);
	}

	private void InitJoystickNavigation()
	{
		SelectableExtensions.SetMode((Selectable)(object)inventoryButton.Button, (Mode)4);
		SelectableExtensions.SetMode((Selectable)(object)characterSheetButton.Button, (Mode)4);
		SelectableExtensions.SetMode((Selectable)(object)shopButton.Button, (Mode)4);
		SelectableExtensions.SetMode((Selectable)(object)constructionButton.Button, (Mode)4);
		SelectableExtensions.SetMode((Selectable)(object)metaShopsButton.Button, (Mode)4);
		SelectableExtensions.SetMode((Selectable)(object)innButton.Button, (Mode)4);
		SelectableExtensions.SetMode((Selectable)(object)settingsButton, (Mode)4);
		characterSheetNavigationLeft = new List<Selectable> { (Selectable)(object)inventoryButton.Button };
		characterSheetNavigationDown = new List<Selectable>
		{
			(Selectable)(object)constructionButton.Button,
			(Selectable)(object)innButton.Button,
			(Selectable)(object)metaShopsButton.Button,
			(Selectable)(object)settingsButton
		};
		inventoryNavigationRight = new List<Selectable> { (Selectable)(object)characterSheetButton.Button };
		inventoryNavigationDown = new List<Selectable>
		{
			(Selectable)(object)shopButton.Button,
			(Selectable)(object)metaShopsButton.Button,
			(Selectable)(object)innButton.Button,
			(Selectable)(object)settingsButton
		};
		metaShopsNavigationUp = new List<Selectable>
		{
			(Selectable)(object)shopButton.Button,
			(Selectable)(object)inventoryButton.Button
		};
		metaShopsNavigationRight = new List<Selectable> { (Selectable)(object)innButton.Button };
		metaShopsNavigationDown = new List<Selectable> { (Selectable)(object)settingsButton };
		innNavigationLeft = new List<Selectable> { (Selectable)(object)metaShopsButton.Button };
		innNavigationUp = new List<Selectable>
		{
			(Selectable)(object)constructionButton.Button,
			(Selectable)(object)characterSheetButton.Button
		};
		innNavigationDown = new List<Selectable> { (Selectable)(object)settingsButton };
		shopNavigationRight = new List<Selectable> { (Selectable)(object)constructionButton.Button };
		shopNavigationDown = new List<Selectable>
		{
			(Selectable)(object)metaShopsButton.Button,
			(Selectable)(object)innButton.Button,
			(Selectable)(object)settingsButton
		};
		shopNavigationUp = new List<Selectable> { (Selectable)(object)inventoryButton.Button };
		constructionNavigationUp = new List<Selectable> { (Selectable)(object)characterSheetButton.Button };
		constructionNavigationLeft = new List<Selectable> { (Selectable)(object)shopButton.Button };
		constructionNavigationDown = new List<Selectable>
		{
			(Selectable)(object)innButton.Button,
			(Selectable)(object)metaShopsButton.Button,
			(Selectable)(object)settingsButton
		};
		settingsNavigationUp = new List<Selectable>
		{
			(Selectable)(object)metaShopsButton.Button,
			(Selectable)(object)innButton.Button,
			(Selectable)(object)shopButton.Button,
			(Selectable)(object)constructionButton.Button,
			(Selectable)(object)inventoryButton.Button,
			(Selectable)(object)characterSheetButton.Button
		};
	}

	private void OnEnable()
	{
		TileObjectSelectionManager.OnUnitSelectionChange += OnSelectionChange;
	}

	private void OnDisable()
	{
		TileObjectSelectionManager.OnUnitSelectionChange -= OnSelectionChange;
	}
}
