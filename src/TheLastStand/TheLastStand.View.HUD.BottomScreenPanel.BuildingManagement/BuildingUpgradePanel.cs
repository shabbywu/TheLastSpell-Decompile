using System.Collections;
using TMPro;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.View.Building.UI;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingUpgradePanel : BuildingCapacityPanel
{
	public static class Constants
	{
		public const string UpgradeIconDefaultPath = "View/Sprites/UI/Buildings/Upgrades/Default";

		public const string UpgradeIconPathPrefix = "View/Sprites/UI/Buildings/Upgrades/";
	}

	[SerializeField]
	private GameObject goldParent;

	[SerializeField]
	private TextMeshProUGUI goldText;

	[SerializeField]
	private GameObject materialsParent;

	[SerializeField]
	private TextMeshProUGUI materialsText;

	[SerializeField]
	private GameObject maxLevelParent;

	[SerializeField]
	private Image upgradeIcon;

	[SerializeField]
	private Image upgradeLevelIcon;

	[SerializeField]
	private DataSpriteTable upgradeLevelIcons;

	[SerializeField]
	private Image maxLevelIcon;

	[SerializeField]
	private Animator upgradeLevelFXAnimator;

	[SerializeField]
	[Range(0f, 1f)]
	private float refreshDelayAfterLevelUp = 0.3f;

	private Coroutine refreshCoroutine;

	public BuildingUpgrade BuildingUpgrade { get; set; }

	public void Display(bool show)
	{
		((Component)base.BuildingCapacityRect).gameObject.SetActive(show);
	}

	public override void OnSkillPanelHovered(bool hover)
	{
		if (hover && BuildingUpgrade != null)
		{
			BuildingManager.BuildingUpgradeTooltip.SetContent(BuildingUpgrade);
			BuildingUpgradeTooltip buildingUpgradeTooltip = BuildingManager.BuildingUpgradeTooltip;
			Transform obj = tooltipAnchor;
			object obj2 = ((obj != null) ? ((Component)obj).transform : null);
			if (obj2 == null)
			{
				BetterButton obj3 = confirmButton;
				obj2 = ((obj3 != null) ? ((Component)obj3).transform : null);
			}
			buildingUpgradeTooltip.FollowTarget = (Transform)obj2;
			BuildingManager.BuildingUpgradeTooltip.Display();
		}
		else
		{
			BuildingManager.BuildingUpgradeTooltip.Hide();
		}
	}

	public void OnUpgradeButtonClick()
	{
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)("Building upgrade " + BuildingUpgrade.BuildingUpgradeDefinition.Id + " was clicked."), (CLogLevel)0, false, false);
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(this);
	}

	public void OnUpgradeConfirmButtonClick(BetterButton button)
	{
		((Selectable)button).interactable = false;
		if (refreshCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(refreshCoroutine);
		}
		OnConfirmButtonClick();
		BuildingUpgrade.Building.BuildingView.BuildingHUD.CompleteCurrentHealthAnimation();
		refreshCoroutine = ((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(WaitRefresh());
		BuildingUpgrade.BuildingUpgradeController.UnlockUpgrade();
		upgradeLevelFXAnimator.SetTrigger("levelUp");
		buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
		buildingCapacitiesPanel.JoystickSkillBar.DeselectCurrentSkill();
		GameView.BottomScreenPanel.BuildingManagementPanel.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshWorkersNotification();
		if (BuildingUpgrade != null && BuildingUpgrade.UpgradeLevel + 1 >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count)
		{
			base.button.Interactable = false;
		}
	}

	public override void Refresh()
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		if (BuildingUpgrade != null && refreshCoroutine == null)
		{
			int num = BuildingUpgrade.UpgradeLevel + 1;
			bool flag = num >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions.Count;
			goldParent.SetActive(!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost > 0);
			materialsParent.SetActive(!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost > 0);
			maxLevelParent.SetActive(flag);
			if (!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost > 0)
			{
				((TMP_Text)goldText).text = $"{BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost}";
				((Graphic)goldText).color = ((TPSingleton<ResourceManager>.Instance.Gold >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost) ? TPSingleton<ResourceManager>.Instance.GetResourceColor("Gold") : Color.red);
			}
			if (!flag && BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost > 0)
			{
				((TMP_Text)materialsText).text = $"{BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost}";
				((Graphic)materialsText).color = ((TPSingleton<ResourceManager>.Instance.Materials >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost) ? TPSingleton<ResourceManager>.Instance.GetResourceColor("Materials") : Color.red);
			}
			RefreshUpgradeLevelIcon();
			((Behaviour)maxLevelIcon).enabled = flag;
			button.Interactable = !flag && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night && TPSingleton<ResourceManager>.Instance.Gold >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].GoldCost && TPSingleton<ResourceManager>.Instance.Materials >= BuildingUpgrade.BuildingUpgradeDefinition.LeveledBuildingUpgradeDefinitions[num].MaterialCost;
			upgradeIcon.sprite = ResourcePooler<Sprite>.LoadOnce(string.Format("{0}{1}{2}", "View/Sprites/UI/Buildings/Upgrades/", BuildingUpgrade.BuildingUpgradeDefinition.Id, flag ? BuildingUpgrade.UpgradeLevel : num), false);
			if ((Object)(object)upgradeIcon.sprite == (Object)null)
			{
				upgradeIcon.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/Buildings/Upgrades/Default", false);
			}
		}
	}

	public override void SelectConfirmButton()
	{
		if (refreshCoroutine == null)
		{
			base.SelectConfirmButton();
		}
	}

	private void RefreshUpgradeLevelIcon()
	{
		((Behaviour)upgradeLevelIcon).enabled = BuildingUpgrade.UpgradeLevel != -1;
		if (BuildingUpgrade.UpgradeLevel >= 0)
		{
			upgradeLevelIcon.sprite = upgradeLevelIcons.GetSpriteAt(Mathf.Min(upgradeLevelIcons._Sprites.Length - 1, BuildingUpgrade.UpgradeLevel));
		}
	}

	private IEnumerator WaitRefresh()
	{
		yield return SharedYields.WaitForSeconds(refreshDelayAfterLevelUp);
		refreshCoroutine = null;
		Refresh();
	}
}
