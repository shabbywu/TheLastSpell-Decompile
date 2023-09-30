using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsUpgradeLevelButton : MonoBehaviour
{
	[SerializeField]
	private BetterButton upgradeLevelButton;

	[SerializeField]
	private GameObject highlight;

	[SerializeField]
	private Color baseColor = Color.white;

	[SerializeField]
	private Color highlightColor = Color.white;

	private int level;

	private BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel;

	public void Highlight(bool state)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		highlight.SetActive(state);
		upgradeLevelButton.ChangeTextColor(state ? highlightColor : baseColor);
	}

	public void Init(int level, BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		this.level = level;
		this.buildingSettingsUpgradeLevel = buildingSettingsUpgradeLevel;
		upgradeLevelButton.ChangeText(level.ToString());
		Highlight(state: false);
		((UnityEvent)((Button)upgradeLevelButton).onClick).AddListener(new UnityAction(OnButtonClick));
	}

	private void OnButtonClick()
	{
		buildingSettingsUpgradeLevel.SetUpgradeLevel(level);
	}
}
