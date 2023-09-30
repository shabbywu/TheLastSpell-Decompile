using System.Collections;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.View.Building;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class ExtinguishBrazierFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	private int brazierLoss;

	private int brazierRemainingAfterLoss;

	private Coroutine displayCoroutine;

	private DamageDisplay damageDisplayPrefab;

	public BuildingView BuildingView { get; set; }

	public Coroutine Display()
	{
		if (displayCoroutine == null)
		{
			displayCoroutine = ((MonoBehaviour)TPSingleton<EffectManager>.Instance).StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(BuildingView buildingView)
	{
		((Object)this).name = "Extinguish Brazier Feedback - " + ((Object)buildingView.GameObject).name;
		BuildingView = buildingView;
		((Component)this).transform.SetParent(buildingView.GameObject.transform, false);
	}

	public void InitBrazierLoss(int brazierLoss, int brazierRemainingAfterLoss)
	{
		this.brazierLoss = brazierLoss;
		this.brazierRemainingAfterLoss = brazierRemainingAfterLoss;
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log((object)$"Displaying ExtinguishBrazier Feedback {brazierLoss}", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
		DamageDisplay pooledComponent = ObjectPooler.GetPooledComponent<DamageDisplay>("DamageDisplay", ResourcePooler.LoadOnce<DamageDisplay>("Prefab/Displayable Effect/UI Effect Displays/DamageDisplay", false), EffectManager.EffectDisplaysParent, false);
		((Object)pooledComponent).name = "DamageDisplay - " + ((Object)BuildingView.GameObject).name;
		pooledComponent.FollowElement.ChangeTarget(BuildingView.DamageableHUD.Transform);
		pooledComponent.Init(brazierLoss);
		pooledComponent.Display();
		BuildingView.BuildingHUD.PlayBrazierLossAnim(brazierLoss, brazierRemainingAfterLoss);
		yield break;
	}
}
