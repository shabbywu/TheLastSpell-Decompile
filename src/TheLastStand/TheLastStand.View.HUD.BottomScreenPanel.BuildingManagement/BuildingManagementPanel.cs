using System;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework.EventSystem;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Events;
using TheLastStand.View.Building.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingManagementPanel : SerializedMonoBehaviour
{
	public enum E_State
	{
		Closed,
		Opened,
		Hidden
	}

	private static class Constants
	{
		public const string BuildingIndestructible = "Building_Indestructible";
	}

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private BuildingCapacitiesPanel buildingSkillsPanel;

	[SerializeField]
	private Image portraitBackground;

	[SerializeField]
	private Sprite portraitDefaultBackground;

	[SerializeField]
	private Sprite portraitBrazierBackground;

	[SerializeField]
	private Sprite portraitProductionBackground;

	[SerializeField]
	private TextMeshProUGUI buildingIdText;

	[SerializeField]
	private Image buildingPortraitImage;

	[SerializeField]
	private RectTransform buildingDescription;

	[SerializeField]
	private Transform buildingDescriptionTooltipAnchor;

	[SerializeField]
	private TextMeshProUGUI buildingDescriptionText;

	[SerializeField]
	private TPEmptyGraphic descriptionHitbox;

	[SerializeField]
	private Slider buildingHealthGaugeSlider;

	[SerializeField]
	private Image buildingHealthGaugeImage;

	[SerializeField]
	private TextMeshProUGUI buildingHealthGaugeText;

	[SerializeField]
	private Sprite healthGaugeClassic;

	[SerializeField]
	private Sprite healthGaugeInvulnerable;

	[SerializeField]
	private GameObject brazierHealth;

	[SerializeField]
	private Slider brazierHealthGaugeSlider;

	[SerializeField]
	private TextMeshProUGUI brazierHealthGaugeText;

	[SerializeField]
	private BuildingProductionManagementPanel buildingProductionManagementPanel;

	[SerializeField]
	private GameObject magicCircleProduction;

	[SerializeField]
	private Transform magicCircleUnitsBackground;

	[SerializeField]
	private Image buildingProductionGaugeRewardImage;

	[SerializeField]
	private BuildingManagementPanelGauge buildingProductionUnitsGaugeMagicCircle;

	[SerializeField]
	private Button shopButton;

	[SerializeField]
	private Button innButton;

	[SerializeField]
	private Image shopChains;

	[SerializeField]
	private Image innChains;

	private TheLastStand.Model.Building.Building building;

	public BuildingCapacitiesPanel BuildingCapacitiesPanel => buildingSkillsPanel;

	public E_State State { get; private set; }

	public void Close()
	{
		building = null;
		BuildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		BuildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		BuildingCapacitiesPanel.HideAllGroups();
		((Behaviour)canvas).enabled = false;
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).UnregisterChilds();
		}
		State = E_State.Closed;
		SkillManager.SkillInfoPanel.Hide();
	}

	public void OnGameStateChange(Game.E_State state)
	{
		if (State == E_State.Hidden)
		{
			Unhide();
		}
	}

	public void OnInnButtonClick()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Recruitment && RecruitmentController.CanOpenRecruitmentPanel())
		{
			RecruitmentController.OpenRecruitmentPanel();
		}
	}

	public void OnShopButtonClick()
	{
		if (TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel())
		{
			TPSingleton<BuildingManager>.Instance.Shop.ShopController.OpenShopPanel();
		}
	}

	public void Open()
	{
		((Behaviour)canvas).enabled = UIManager.DebugToggleUI != false;
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RegisterChilds();
		}
		State = E_State.Opened;
		Refresh();
	}

	public void Refresh()
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		if (!TileObjectSelectionManager.HasBuildingSelected)
		{
			Close();
			return;
		}
		TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
		if (TileObjectSelectionManager.SelectedBuilding.BattleModule?.Goals != null && TileObjectSelectionManager.SelectedBuilding.BattleModule.Goals[0].GoalController.CheckPreconditionGroups())
		{
			SkillManager.SkillInfoPanel.FollowElement.ChangeTarget(buildingDescriptionTooltipAnchor);
			SkillManager.SkillInfoPanel.SetContent(TileObjectSelectionManager.SelectedBuilding.BattleModule.Goals[0].Skill, TileObjectSelectionManager.SelectedBuilding.BattleModule);
			SkillManager.SkillInfoPanel.CompendiumFollowRight = true;
			SkillManager.SkillInfoPanel.Display();
		}
		else
		{
			SkillManager.SkillInfoPanel.Hide();
		}
		if (TileObjectSelectionManager.SelectedBuilding != building)
		{
			building = TileObjectSelectionManager.SelectedBuilding;
			buildingPortraitImage.sprite = building.BuildingView.GetPortraitSprite();
			((Behaviour)((Selectable)shopButton).image).enabled = building.BuildingDefinition.Id == "Shop";
			if (building.BuildingDefinition.Id == "Shop")
			{
				bool flag = TPSingleton<BuildingManager>.Instance.Shop.ShopController.CanOpenShopPanel();
				((Selectable)shopButton).interactable = flag;
				((Graphic)((Selectable)shopButton).image).color = (flag ? Color.white : Color.gray);
				((Behaviour)shopChains).enabled = !flag;
			}
			else
			{
				((Behaviour)shopChains).enabled = false;
			}
			((Behaviour)((Selectable)innButton).image).enabled = building.BuildingDefinition.Id == "Inn";
			if (building.BuildingDefinition.Id == "Inn")
			{
				bool flag2 = RecruitmentController.CanOpenRecruitmentPanel();
				((Selectable)innButton).interactable = flag2;
				((Graphic)((Selectable)innButton).image).color = (flag2 ? Color.white : Color.gray);
				((Behaviour)innChains).enabled = !flag2;
			}
			else
			{
				((Behaviour)innChains).enabled = false;
			}
		}
		if (building.IsTrap)
		{
			buildingPortraitImage.sprite = building.BuildingView.GetPortraitSprite();
		}
		if (!(building is MagicCircle magicCircle))
		{
			if (building.BlueprintModule.IsIndestructible)
			{
				buildingHealthGaugeImage.sprite = healthGaugeInvulnerable;
				buildingHealthGaugeSlider.value = 1f;
				((TMP_Text)buildingHealthGaugeText).text = AtlasIcons.InvulnerableIcon + " " + Localizer.Get("Building_Indestructible");
			}
			else
			{
				buildingHealthGaugeImage.sprite = healthGaugeClassic;
				buildingHealthGaugeSlider.value = ((building.DamageableModule.HealthTotal > 0f) ? (building.DamageableModule.Health / building.DamageableModule.HealthTotal) : 1f);
				((TMP_Text)buildingHealthGaugeText).text = ((building.DamageableModule.HealthTotal > 0f) ? $"{building.DamageableModule.Health}/{building.DamageableModule.HealthTotal}" : Localizer.Get("Building_Indestructible"));
			}
		}
		else
		{
			buildingHealthGaugeImage.sprite = healthGaugeClassic;
			buildingHealthGaugeSlider.value = ((magicCircle.CurrentHealthTotal > 0f) ? (building.DamageableModule.Health / magicCircle.CurrentHealthTotal) : 1f);
			((TMP_Text)buildingHealthGaugeText).text = ((magicCircle.CurrentHealthTotal > 0f) ? $"{building.DamageableModule.Health}/{magicCircle.CurrentHealthTotal}" : Localizer.Get("Building_Indestructible"));
		}
		if (building.BrazierModule != null && building.BrazierModule.BrazierPointsTotal > 0)
		{
			portraitBackground.sprite = portraitBrazierBackground;
			brazierHealth.SetActive(true);
			brazierHealthGaugeSlider.value = (float)building.BrazierModule.BrazierPoints / (float)building.BrazierModule.BrazierPointsTotal;
			((TMP_Text)brazierHealthGaugeText).text = $"{AtlasIcons.BrazierPointsIcon} {building.BrazierModule.BrazierPoints} / {building.BrazierModule.BrazierPointsTotal}";
		}
		else
		{
			portraitBackground.sprite = portraitDefaultBackground;
			brazierHealth.SetActive(false);
		}
		RefreshProduction();
		BuildingCapacitiesPanel.Refresh(building);
		RefreshLocalizedTexts();
	}

	public void Unhide()
	{
		State = E_State.Opened;
		((Behaviour)canvas).enabled = true;
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RegisterChilds();
		}
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
		if (((Behaviour)canvas).enabled)
		{
			RefreshLocalizedTexts();
		}
	}

	private void OnWorkersChange(Event e)
	{
		Refresh();
	}

	private void RefreshLocalizedTexts()
	{
		((TMP_Text)buildingIdText).text = Localizer.Get("BuildingName_" + building.BuildingDefinition.Id);
		buildingProductionManagementPanel.RefreshLocalizedTexts(building);
		string text = default(string);
		if (Localizer.TryGet("BuildingDescription_" + building.BuildingDefinition.Id, ref text))
		{
			((Component)buildingDescription).gameObject.SetActive(true);
			((Graphic)descriptionHitbox).raycastTarget = true;
			((TMP_Text)buildingDescriptionText).text = text;
		}
		else
		{
			((Graphic)descriptionHitbox).raycastTarget = false;
			((Component)buildingDescription).gameObject.SetActive(false);
		}
	}

	private void RefreshProduction()
	{
		buildingProductionManagementPanel.Refresh(building);
		if (building.ProductionModule?.BuildingGaugeEffect == null)
		{
			DisableProduction();
			return;
		}
		if (building is MagicCircle)
		{
			RefreshProductionAsMagicCircle();
			return;
		}
		portraitBackground.sprite = portraitProductionBackground;
		magicCircleProduction.SetActive(false);
	}

	private void RefreshProductionAsMagicCircle()
	{
		magicCircleProduction.SetActive(true);
		buildingProductionGaugeRewardImage.sprite = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetProductionRewardIconSpriteBig();
		buildingProductionUnitsGaugeMagicCircle.SetUnitsCount(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
		for (int i = 0; i < magicCircleUnitsBackground.childCount; i++)
		{
			((Component)magicCircleUnitsBackground.GetChild(i)).gameObject.SetActive(i < TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount);
		}
		buildingProductionUnitsGaugeMagicCircle.Clear();
		buildingProductionUnitsGaugeMagicCircle.SetUnits(building.ProductionModule.BuildingGaugeEffect.Units);
	}

	private void DisableProduction()
	{
		magicCircleProduction.SetActive(false);
	}

	private void Start()
	{
		Close();
		EventManager.AddListener(typeof(WorkersChangeEvent), OnWorkersChange);
	}
}
