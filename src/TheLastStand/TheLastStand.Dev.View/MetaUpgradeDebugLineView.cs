using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Dev.View;

public class MetaUpgradeDebugLineView : MonoBehaviour
{
	[SerializeField]
	private MetaUpgradeDebugConditionLineView metaConditionExample;

	[SerializeField]
	private MetaConditionSeparatorDebugView metaConditionsSeparatorExample;

	[SerializeField]
	private RectTransform selfContainer;

	[SerializeField]
	private float baseHeight = 40f;

	[SerializeField]
	private Image status;

	[SerializeField]
	private Sprite activatedStatusIcon;

	[SerializeField]
	private Sprite unlockedStatusIcon;

	[SerializeField]
	private Sprite lockedStatusIcon;

	[SerializeField]
	private Button selfButton;

	[SerializeField]
	private Button activateButton;

	[SerializeField]
	private Button unlockButton;

	[SerializeField]
	private Button lockButton;

	[SerializeField]
	private RectTransform conditionsRect;

	[SerializeField]
	private Canvas conditionsCanvas;

	[SerializeField]
	private TextMeshProUGUI upgradeName;

	[SerializeField]
	private TextMeshProUGUI upgradePrice;

	private bool isDeployed;

	private List<MetaUpgradeDebugConditionLineView> metaConditions = new List<MetaUpgradeDebugConditionLineView>();

	private List<MetaConditionSeparatorDebugView> separators = new List<MetaConditionSeparatorDebugView>();

	public MetaUpgrade MetaUpgrade { get; private set; }

	public MetaUpgradesManager.E_MetaState MetaState { get; private set; }

	public void Activate()
	{
		MetaUpgradeDebugStateManager.ActivateUpgrade(MetaUpgrade);
		RefreshStatus();
	}

	public void Lock()
	{
		MetaUpgradeDebugStateManager.LockUpgrade(MetaUpgrade);
		RefreshStatus();
	}

	public void Refresh()
	{
		RefreshStatus();
		foreach (MetaUpgradeDebugConditionLineView metaCondition in metaConditions)
		{
			metaCondition.Refresh();
		}
	}

	public void Set(MetaUpgrade upgrade)
	{
		MetaUpgrade = upgrade;
		isDeployed = false;
		((Behaviour)conditionsCanvas).enabled = false;
		foreach (MetaUpgradeDebugConditionLineView metaCondition in metaConditions)
		{
			Object.Destroy((Object)(object)((Component)metaCondition).gameObject);
		}
		foreach (MetaConditionSeparatorDebugView separator in separators)
		{
			Object.Destroy((Object)(object)((Component)separator).gameObject);
		}
		if (upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock).Count > 0)
		{
			AddSeparator("Unlock conditions");
			foreach (List<MetaConditionController> condition in upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock))
			{
				foreach (MetaConditionController item in condition)
				{
					AddConditionLine(item.MetaCondition);
				}
			}
		}
		if (upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation).Count > 0)
		{
			AddSeparator("Activation conditions");
			foreach (List<MetaConditionController> condition2 in upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation))
			{
				foreach (MetaConditionController item2 in condition2)
				{
					AddConditionLine(item2.MetaCondition);
				}
			}
		}
		int num = upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation).Count + upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock).Count;
		((TMP_Text)upgradeName).text = upgrade.MetaUpgradeDefinition.Id + ((num > 0) ? $"  <i>({num} condition(s))</i>" : "");
		((Object)this).name = upgrade.MetaUpgradeDefinition.Id;
		((TMP_Text)upgradePrice).text = ((upgrade.MetaUpgradeDefinition.Price != 0) ? $"{upgrade.InvestedSouls} / {upgrade.MetaUpgradeDefinition.Price}" : string.Empty);
		Refresh();
	}

	public void ToggleDeploy()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (isDeployed)
		{
			isDeployed = false;
			TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(selfContainer, new Vector2(selfContainer.sizeDelta.x, baseHeight), 0.3f, false), (TweenCallback)delegate
			{
				((Behaviour)conditionsCanvas).enabled = false;
			});
		}
		else
		{
			isDeployed = true;
			((Behaviour)conditionsCanvas).enabled = true;
			DOTweenModuleUI.DOSizeDelta(selfContainer, new Vector2(selfContainer.sizeDelta.x, baseHeight + conditionsRect.sizeDelta.y), 0.3f, false);
		}
	}

	public void Unlock()
	{
		MetaUpgradeDebugStateManager.UnlockUpgrade(MetaUpgrade);
		RefreshStatus();
	}

	private void AddConditionLine(MetaCondition metaCondition)
	{
		MetaUpgradeDebugConditionLineView component = Object.Instantiate<GameObject>(((Component)metaConditionExample).gameObject, ((Component)metaConditionExample).transform.parent).GetComponent<MetaUpgradeDebugConditionLineView>();
		component.Set(metaCondition);
		((Component)component).gameObject.SetActive(true);
		metaConditions.Add(component);
	}

	private void AddSeparator(string name)
	{
		MetaConditionSeparatorDebugView component = Object.Instantiate<GameObject>(((Component)metaConditionsSeparatorExample).gameObject, ((Component)metaConditionsSeparatorExample).transform.parent).GetComponent<MetaConditionSeparatorDebugView>();
		component.SetText(name);
		((Component)component).gameObject.SetActive(true);
		separators.Add(component);
	}

	private void Awake()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		((Component)metaConditionsSeparatorExample).gameObject.SetActive(false);
		((Component)metaConditionExample).gameObject.SetActive(false);
		((UnityEvent)selfButton.onClick).AddListener(new UnityAction(ToggleDeploy));
		((UnityEvent)lockButton.onClick).AddListener(new UnityAction(Lock));
		((UnityEvent)unlockButton.onClick).AddListener(new UnityAction(Unlock));
		((UnityEvent)activateButton.onClick).AddListener(new UnityAction(Activate));
	}

	private void RefreshStatus()
	{
		if (TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Contains(MetaUpgrade))
		{
			MetaState = MetaUpgradesManager.E_MetaState.Activated;
			status.sprite = activatedStatusIcon;
		}
		else if (TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Contains(MetaUpgrade))
		{
			MetaState = MetaUpgradesManager.E_MetaState.Unlocked;
			status.sprite = unlockedStatusIcon;
		}
		else if (TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Contains(MetaUpgrade))
		{
			MetaState = MetaUpgradesManager.E_MetaState.Locked;
			status.sprite = lockedStatusIcon;
		}
		else
		{
			MetaState = MetaUpgradesManager.E_MetaState.NA;
			Debug.LogError((object)("Could not find Upgrade with Id " + MetaUpgrade.MetaUpgradeDefinition.Id + " in any upgrades list."));
		}
	}
}
