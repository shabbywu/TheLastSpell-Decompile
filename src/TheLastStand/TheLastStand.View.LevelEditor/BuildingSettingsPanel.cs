using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Model.Building;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsPanel : MonoBehaviour
{
	[SerializeField]
	private Button panelDisplayButton;

	[SerializeField]
	private TextMeshProUGUI buildingNameText;

	[SerializeField]
	private Image buildingIcon;

	[SerializeField]
	private RectTransform settingsLayout;

	[SerializeField]
	private BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevelPrefab;

	[SerializeField]
	private BuildingSettingsHealth buildingSettingsHealth;

	[SerializeField]
	private GameObject panel;

	[SerializeField]
	private Vector2 panelOffset = Vector2.zero;

	private RectTransform rectTransform;

	public TheLastStand.Model.Building.Building Building { get; private set; }

	public BuildingSettingsHealth BuildingSettingsHealth => buildingSettingsHealth;

	public List<BuildingSettingsUpgradeLevel> BuildingSettingsUpgradeLevels { get; } = new List<BuildingSettingsUpgradeLevel>();


	public RectTransform RectTransform
	{
		get
		{
			if ((Object)(object)rectTransform == (Object)null)
			{
				ref RectTransform reference = ref rectTransform;
				Transform transform = ((Component)this).transform;
				reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			}
			return rectTransform;
		}
	}

	public static event Action<BuildingSettingsPanel> PanelOpen;

	public void DisplayPanel(bool show)
	{
		panel.SetActive(show);
		if (show)
		{
			BuildingSettingsPanel.PanelOpen?.Invoke(this);
		}
	}

	public void Init(TheLastStand.Model.Building.Building building)
	{
		Building = building;
		((TMP_Text)buildingNameText).text = Building.BuildingDefinition.Id;
		buildingIcon.sprite = Building.BuildingView.GetPortraitSprite();
		if (!building.BlueprintModule.IsIndestructible)
		{
			buildingSettingsHealth.Init(building.BuildingDefinition);
		}
		if (building.UpgradeModule != null && building.BuildingDefinition.UpgradeModuleDefinition.BuildingUpgradeDefinitions != null)
		{
			foreach (BuildingUpgradeDefinition buildingUpgradeDefinition in building.BuildingDefinition.UpgradeModuleDefinition.BuildingUpgradeDefinitions)
			{
				BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel = Object.Instantiate<BuildingSettingsUpgradeLevel>(buildingSettingsUpgradeLevelPrefab, (Transform)(object)settingsLayout);
				buildingSettingsUpgradeLevel.Init(buildingUpgradeDefinition);
				BuildingSettingsUpgradeLevels.Add(buildingSettingsUpgradeLevel);
			}
		}
		DisplayPanel(show: false);
		LayoutRebuilder.ForceRebuildLayoutImmediate(settingsLayout);
		((Component)this).gameObject.SetActive(false);
	}

	public void Init(TheLastStand.Model.Building.Building building, XContainer buildingContainer)
	{
		Init(building);
		XElement val = buildingContainer.Element(XName.op_Implicit("Health"));
		if (val != null)
		{
			BuildingSettingsHealth.SetHealth(int.Parse(val.Value));
		}
		XElement val2 = buildingContainer.Element(XName.op_Implicit("UpgradesLevels"));
		if (val2 == null)
		{
			return;
		}
		foreach (XElement upgradeLevelElement in ((XContainer)val2).Elements())
		{
			BuildingSettingsUpgradeLevels.Where((BuildingSettingsUpgradeLevel o) => o.BuildingUpgradeDefinition.Id == upgradeLevelElement.Name.LocalName).First().SetUpgradeLevel(int.Parse(upgradeLevelElement.Value));
		}
	}

	private void OnPanelOpen(BuildingSettingsPanel buildingSettingsPanel)
	{
		if ((Object)(object)buildingSettingsPanel != (Object)(object)this)
		{
			DisplayPanel(show: false);
		}
	}

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)panelDisplayButton.onClick).AddListener((UnityAction)delegate
		{
			DisplayPanel(!panel.activeSelf);
		});
		PanelOpen += OnPanelOpen;
	}

	private void Update()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(((Component)Building.BuildingView).transform.position + Vector2.op_Implicit(panelOffset));
		Vector2 val2 = Vector2.op_Implicit(ACameraView.MainCam.WorldToViewportPoint(Vector2.op_Implicit(val)));
		RectTransform.anchorMin = val2;
		RectTransform.anchorMax = val2;
	}

	private void OnDestroy()
	{
		((UnityEventBase)panelDisplayButton.onClick).RemoveAllListeners();
		PanelOpen -= OnPanelOpen;
	}
}
