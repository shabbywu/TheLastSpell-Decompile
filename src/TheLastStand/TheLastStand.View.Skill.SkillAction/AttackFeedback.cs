using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

public class AttackFeedback : SerializedMonoBehaviour, IDisplayableEffect
{
	public static class Constants
	{
		public const string ImpactFXObjectPoolName = "Impact FX";

		public const string DamageDisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/DamageDisplay";

		public const string ImpactFxPrefabResourcePath = "Prefab/Displayable Effect/Impact FX";
	}

	[SerializeField]
	private Vector2 impactOffsetRange = new Vector2(-0.5f, 0.5f);

	private Queue<AttackSkillActionExecutionTileData> damageInstancesBeingDisplayed = new Queue<AttackSkillActionExecutionTileData>();

	private Queue<AttackSkillActionExecutionTileData> damageInstancesInPreparation = new Queue<AttackSkillActionExecutionTileData>();

	private Coroutine displayCoroutine;

	private GameObject impactFxPrefab;

	private DamageDisplay damageDisplayPrefab;

	public IDamageableView DamageableView { get; set; }

	private DamageDisplay DamageDisplayPrefab
	{
		get
		{
			if ((Object)(object)damageDisplayPrefab == (Object)null)
			{
				damageDisplayPrefab = ResourcePooler.LoadOnce<DamageDisplay>("Prefab/Displayable Effect/UI Effect Displays/DamageDisplay", failSilently: false);
			}
			return damageDisplayPrefab;
		}
	}

	private GameObject ImpactFxPrefab
	{
		get
		{
			if ((Object)(object)impactFxPrefab == (Object)null)
			{
				impactFxPrefab = ResourcePooler.LoadOnce<GameObject>("Prefab/Displayable Effect/Impact FX", failSilently: false);
			}
			return impactFxPrefab;
		}
	}

	public void AddDamageInstance(AttackSkillActionExecutionTileData attackData)
	{
		damageInstancesInPreparation.Enqueue(attackData);
	}

	public Coroutine Display()
	{
		while (damageInstancesInPreparation.Count > 0)
		{
			damageInstancesBeingDisplayed.Enqueue(damageInstancesInPreparation.Dequeue());
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = ((MonoBehaviour)TPSingleton<EffectManager>.Instance).StartCoroutine(DisplayCoroutine());
		}
		return displayCoroutine;
	}

	public void Init(IDamageableView damageableView)
	{
		((Object)this).name = "Attack Feedback - " + ((Object)damageableView.GameObject).name;
		DamageableView = damageableView;
		((Component)this).transform.SetParent(damageableView.GameObject.transform, false);
	}

	private IEnumerator DisplayCoroutine()
	{
		CLoggerManager.Log((object)"Start DisplayCoroutine", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
		Vector3 impactOffset = Vector3.zero;
		EnemyUnit enemyUnit = DamageableView.Damageable as EnemyUnit;
		while (damageInstancesBeingDisplayed.Count > 0)
		{
			AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = damageInstancesBeingDisplayed.Dequeue();
			CLoggerManager.Log((object)$"Displaying damageInstance {attackSkillActionExecutionTileData.TotalDamage}", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
			DamageDisplay pooledComponent = ObjectPooler.GetPooledComponent<DamageDisplay>("DamageDisplay", DamageDisplayPrefab, EffectManager.EffectDisplaysParent, dontSetParent: false);
			((Object)pooledComponent).name = "DamageDisplay - " + ((Object)DamageableView.GameObject).name;
			pooledComponent.FollowElement.ChangeTarget(DamageableView.DamageableHUD.Transform);
			pooledComponent.Init(attackSkillActionExecutionTileData);
			pooledComponent.Display();
			if (!attackSkillActionExecutionTileData.Dodged)
			{
				SpriteSheetFx component = Object.Instantiate<GameObject>(ImpactFxPrefab, GameManager.ViewTransform).GetComponent<SpriteSheetFx>();
				impactOffset.x = RandomManager.GetRandomRange(this, impactOffsetRange.x, impactOffsetRange.y);
				impactOffset.y = RandomManager.GetRandomRange(this, impactOffsetRange.x, impactOffsetRange.y);
				component.Init(TileMapView.GetWorldPosition(attackSkillActionExecutionTileData.TargetTile) + impactOffset);
				component.Display();
				DamageableView.DamageableHUD.PlayDamageAnim(attackSkillActionExecutionTileData);
				DamageableView.PlayTakeDamageAnim();
			}
			if (!DamageableView.Damageable.IsDead || damageInstancesBeingDisplayed.Count != 0)
			{
				continue;
			}
			bool isFinalDeath = TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory;
			if (enemyUnit is BossUnit bossUnit && (isFinalDeath || bossUnit.AlwaysPlayDeathCutscene))
			{
				yield return TPSingleton<BossManager>.Instance.TryPlayDeathCutscene(bossUnit, isFinalDeath);
				if (isFinalDeath)
				{
					TPSingleton<NightTurnsManager>.Instance.StopCoroutinesExecution();
				}
			}
			else if (!(DamageableView.Damageable is TheLastStand.Model.Unit.Unit { HasBeenExiled: not false, ExileForcePlayDieAnim: false }))
			{
				DamageableView.PlayDieAnim();
			}
		}
		displayCoroutine = null;
		CLoggerManager.Log((object)"Finished DisplayCoroutine", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "Feedbacks", false);
	}
}
