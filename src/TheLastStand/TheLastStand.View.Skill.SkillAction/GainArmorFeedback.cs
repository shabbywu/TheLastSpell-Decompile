using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib.Log;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class GainArmorFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	[SerializeField]
	private float delayBetweenArmorInstances = 0.15f;

	private Queue<Tuple<float, float>> armorInstancesBeingDisplayed = new Queue<Tuple<float, float>>();

	private Queue<Tuple<float, float>> armorInstancesInPreparation = new Queue<Tuple<float, float>>();

	private Coroutine displayCoroutine;

	public IDamageableView DamageableView { get; set; }

	public void AddArmorGainInstance(float armorAmount, float armorthAfterArmor)
	{
		armorInstancesInPreparation.Enqueue(new Tuple<float, float>(armorAmount, armorthAfterArmor));
	}

	public Coroutine Display()
	{
		while (armorInstancesInPreparation.Count > 0)
		{
			armorInstancesBeingDisplayed.Enqueue(armorInstancesInPreparation.Dequeue());
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = ((MonoBehaviour)this).StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(IDamageableView damageableView)
	{
		((Object)this).name = "Gain Armor Feedback - " + ((Object)damageableView.GameObject).name;
		DamageableView = damageableView;
		((Component)this).transform.SetParent(damageableView.GameObject.transform, false);
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log((object)"Start DisplayCoroutine", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
		_ = Vector3.zero;
		while (armorInstancesBeingDisplayed.Count > 0)
		{
			Tuple<float, float> tuple = armorInstancesBeingDisplayed.Dequeue();
			CLoggerManager.Log((object)$"Displaying damageInstance {tuple}", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
			DamageableView.DamageableHUD.PlayArmorGainAnim(tuple.Item1, tuple.Item2);
			yield return SharedYields.WaitForSeconds(delayBetweenArmorInstances);
		}
		displayCoroutine = null;
		CLoggerManager.Log((object)"Finished DisplayCoroutine", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
	}
}
