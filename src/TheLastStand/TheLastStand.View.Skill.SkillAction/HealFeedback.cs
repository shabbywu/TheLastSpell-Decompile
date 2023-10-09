using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class HealFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	public static class Constants
	{
		public const string HealDisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/HealDisplay";
	}

	[SerializeField]
	private float delayBetweenHealInstances = 0.15f;

	private Queue<Tuple<float, float>> healInstancesBeingDisplayed = new Queue<Tuple<float, float>>();

	private Queue<Tuple<float, float>> healInstancesInPreparation = new Queue<Tuple<float, float>>();

	private RestoreStatDisplay healDisplayPrefab;

	private Coroutine displayCoroutine;

	public IDamageableView DamageableView { get; set; }

	private RestoreStatDisplay HealDisplayPrefab
	{
		get
		{
			if ((Object)(object)healDisplayPrefab == (Object)null)
			{
				healDisplayPrefab = ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay", failSilently: false);
			}
			return healDisplayPrefab;
		}
	}

	public void AddHealInstance(float healAmount, float healthAfterHeal)
	{
		healInstancesInPreparation.Enqueue(new Tuple<float, float>(healAmount, healthAfterHeal));
	}

	public Coroutine Display()
	{
		while (healInstancesInPreparation.Count > 0)
		{
			healInstancesBeingDisplayed.Enqueue(healInstancesInPreparation.Dequeue());
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = ((MonoBehaviour)this).StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(IDamageableView damageableView)
	{
		((Object)this).name = "Heal Feedback - " + ((Object)damageableView.GameObject).name;
		DamageableView = damageableView;
		((Component)this).transform.SetParent(damageableView.GameObject.transform, false);
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log((object)"Start DisplayCoroutine", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
		while (healInstancesBeingDisplayed.Count > 0)
		{
			Tuple<float, float> tuple = healInstancesBeingDisplayed.Dequeue();
			CLoggerManager.Log((object)$"Displaying healInstance {tuple}", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
			RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent<RestoreStatDisplay>("RestoreStatDisplay", HealDisplayPrefab, (Transform)null, dontSetParent: true);
			((Component)pooledComponent).transform.SetParent(EffectManager.EffectDisplaysParent, false);
			((Object)pooledComponent).name = "HealDisplay - " + ((Object)DamageableView.GameObject).name;
			pooledComponent.FollowElement.ChangeTarget(DamageableView.DamageableHUD.Transform);
			pooledComponent.Init(UnitStatDefinition.E_Stat.Health, Mathf.RoundToInt(tuple.Item1));
			pooledComponent.Display();
			DamageableView.DamageableHUD.PlayHealthGainAnim(tuple.Item1, tuple.Item2);
			yield return SharedYields.WaitForSeconds(delayBetweenHealInstances);
		}
		displayCoroutine = null;
		CLoggerManager.Log((object)"Finished DisplayCoroutine", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
	}
}
