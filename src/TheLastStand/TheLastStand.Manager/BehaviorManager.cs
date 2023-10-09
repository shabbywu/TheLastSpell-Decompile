using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.Sound;
using UnityEngine;

namespace TheLastStand.Manager;

public abstract class BehaviorManager<T> : Manager<T> where T : SerializedMonoBehaviour
{
	protected SkillCasterAttackGroups skillGroups = new SkillCasterAttackGroups();

	private Dictionary<string, AudioClipsPerId> skillGroupAudioClips = new Dictionary<string, AudioClipsPerId>();

	[SerializeField]
	protected bool turboMode;

	public TargetWaitSettings[] WaitBeforeSkillGroupDurations = new TargetWaitSettings[8];

	public TargetWaitSettings[] WaitAfterSkillGroupDurations = new TargetWaitSettings[8];

	public Dictionary<IDamageable, GroupTargetingInfo> CurrentlyTargetedDamageables = new Dictionary<IDamageable, GroupTargetingInfo>();

	public abstract List<IBehaviorModel> BehaviorModels { get; }

	public bool IsDone { get; protected set; } = true;


	public virtual void ComputeGoals(List<IBehaviorModel> sortedCasters)
	{
		((CLogger<T>)this).Log((object)"Computing goals for casters", (CLogLevel)2, false, false);
		foreach (IBehaviorModel sortedCaster in sortedCasters)
		{
			if (!(sortedCaster is EnemyUnit enemyUnit) || (!enemyUnit.IsDeathRattling && !enemyUnit.IsDead))
			{
				sortedCaster.BehaviorController.ComputeCurrentGoals(CurrentlyTargetedDamageables);
			}
		}
		CurrentlyTargetedDamageables.Clear();
	}

	public virtual IEnumerator ExecuteBehaviorModelsSkillsCoroutine()
	{
		return ExecuteBehaviorModelsSkillsCoroutine(BehaviorModels);
	}

	public virtual IEnumerator ExecuteBehaviorModelsSkillsCoroutine(List<IBehaviorModel> behaviors)
	{
		((CLogger<T>)this).Log((object)("Executing " + ((object)this).GetType().Name + " BehaviorModels skills"), (CLogLevel)2, false, false);
		ACameraView.AllowUserZoom = false;
		ExecuteTurnForBehaviorModels(behaviors);
		((CLogger<T>)this).Log((object)("Waiting for " + ((object)this).GetType().Name + " to be done..."), (CLogLevel)1, false, false);
		yield return (object)new WaitUntil((Func<bool>)(() => IsDone));
		ACameraView.AllowUserZoom = true;
		((CLogger<T>)this).Log((object)(((object)this).GetType().Name + " is done!"), (CLogLevel)1, false, false);
	}

	public void ExecuteSkillsForGroup(SkillCasterAttackGroup attackGroup)
	{
		foreach (ComputedGoal item in attackGroup.GoalsToExecute)
		{
			if (!(item.Goal.Owner is IDamageable damageable) || !damageable.IsDead)
			{
				item.Goal.Owner.BehaviorController.ExecuteGoal(item);
			}
		}
	}

	public virtual void ExecuteTurnForBehaviorModels(List<IBehaviorModel> behaviors)
	{
		IsDone = false;
		List<IBehaviorModel> skillCasters = RemoveSkippedBehaviours(behaviors);
		skillCasters = SortBehaviors(skillCasters);
		ComputeGoals(skillCasters);
		GatherUnitsByGroups(skillCasters);
		if (skillGroups.Count > 0)
		{
			((CLogger<T>)this).Log((object)"Executing turn for skillGroups.", (CLogLevel)2, false, false);
			((MonoBehaviour)(object)TPSingleton<T>.Instance).StartCoroutine(PrepareSkillsForGroups());
		}
		else
		{
			((CLogger<T>)this).Log((object)"Behaviors skill groups are all empty, end of turn.", (CLogLevel)2, false, false);
			IsDone = true;
		}
	}

	public bool EnsureUnitTargeting(IBehaviorModel caster, int goalIndex)
	{
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		ComputedGoal computedGoal = caster.CurrentGoals[goalIndex];
		if (computedGoal == null || computedGoal.Goal == null || computedGoal.TargetTileInfo == null)
		{
			return false;
		}
		computedGoal.Goal.Cooldown = computedGoal.Goal.GoalDefinition.Cooldown;
		Tile tile = null;
		TheLastStand.Model.Building.Building building = computedGoal.TargetTileInfo.Tile.Building;
		TheLastStand.Model.Unit.Unit unit = computedGoal.TargetTileInfo.Tile.Unit;
		ValidTargets.Constraints value;
		Tile tile3;
		switch (computedGoal.TargetTileType)
		{
		case SkillCasterAttackGroup.E_Target.ATTACK_BUILDING:
			if (building == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			goto IL_0120;
		case SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE:
			if (building == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			goto IL_0120;
		case SkillCasterAttackGroup.E_Target.ATTACK_ENEMY:
			if (unit == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			if (unit != caster)
			{
				goto IL_0229;
			}
			goto default;
		case SkillCasterAttackGroup.E_Target.ATTACK_HERO:
			if (unit == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			if (unit != caster)
			{
				goto IL_0229;
			}
			goto default;
		case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY:
		{
			ValidTargets validTargets = computedGoal.Goal.Skill.SkillDefinition.ValidTargets;
			if ((validTargets == null || validTargets.EmptyTiles) && (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange || computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(computedGoal.TargetTileInfo.Tile)))
			{
				tile = computedGoal.TargetTileInfo.Tile;
			}
			break;
		}
		default:
			{
				if (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange || computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(computedGoal.TargetTileInfo.Tile))
				{
					tile = computedGoal.TargetTileInfo.Tile;
				}
				break;
			}
			IL_0120:
			if (computedGoal.Goal.Skill.SkillDefinition.ValidTargets == null || (computedGoal.Goal.Skill.SkillDefinition.ValidTargets.Buildings != null && computedGoal.Goal.Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(building.BuildingDefinition.Id, out value) && (!value.MustBeEmpty || unit == null)))
			{
				Tile tile2 = (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange ? EnsureBuildingTile(computedGoal.TargetType, computedGoal.TargetTileInfo.Tile) : EnsureBuildingTileInLOS(computedGoal.TargetType, building, computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles));
				if (tile2 != null && computedGoal.Goal.Skill.SkillAction.SkillActionController.IsBuildingAffected(tile2))
				{
					tile = tile2;
				}
			}
			break;
			IL_0229:
			tile3 = (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange ? computedGoal.TargetTileInfo.Tile : computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.GetTileInLineOfSight(unit));
			if (tile3 != null && computedGoal.Goal.Skill.SkillAction.SkillActionController.IsUnitAffected(tile3))
			{
				tile = tile3;
			}
			break;
		}
		if (tile == null)
		{
			caster.LogWarning("Ended up with a null targetTile! Skipping my turn, something might be wrong here.", (CLogLevel)1);
			return false;
		}
		if (tile != computedGoal.TargetTileInfo.Tile)
		{
			caster.CurrentGoals[goalIndex] = new ComputedGoal(computedGoal.Goal, new SkillTargetedTileInfo(tile, computedGoal.TargetTileInfo.Orientation));
		}
		caster.TargetTile = tile;
		caster.Log($"Set target tile to : {tile.Position}", (CLogLevel)0);
		return true;
	}

	public void GatherUnitsByGroups(List<IBehaviorModel> skillCasters)
	{
		skillGroups.Clear();
		List<ComputedGoal> goalsToExecute = GetGoalsToExecute(skillCasters);
		skillGroups.Init(goalsToExecute);
	}

	public virtual IEnumerator MoveCameraAndExecute(SkillCasterCluster skillCasterCluster)
	{
		if (skillCasterCluster.TryGetFirstComputedGoalWithPreCastFXs(out var computedGoal))
		{
			ACameraView.MoveTo(((Component)(MonoBehaviour)computedGoal.Goal.Holder.TileObjectView).transform.position, CameraView.AnimationMoveSpeed, (Ease)0);
			yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
			float num = computedGoal.Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PlayPreCastFxs();
			if (num > 0f)
			{
				yield return SharedYields.WaitForSeconds(num);
			}
		}
		((CLogger<T>)this).Log((object)$"{skillCasterCluster.SkillCasterAttackGroups.Count} SkillCasterAttackGroups for cluster {skillCasterCluster.ClusterOrder} with target {skillCasterCluster.TargetType}, moving camera to {skillCasterCluster.WorldPositionFocus}", (CLogLevel)0, false, false);
		switch (skillCasterCluster.TargetType)
		{
		case SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE:
			ACameraView.MoveTo(((Component)BuildingManager.MagicCircle.BuildingView).transform.position, CameraView.AnimationMoveSpeed, (Ease)0);
			break;
		case SkillCasterAttackGroup.E_Target.ATTACK_HERO:
		case SkillCasterAttackGroup.E_Target.ATTACK_BUILDING:
		case SkillCasterAttackGroup.E_Target.ATTACK_ENEMY:
		case SkillCasterAttackGroup.E_Target.GENERIC:
		case SkillCasterAttackGroup.E_Target.SPAWN:
		case SkillCasterAttackGroup.E_Target.OTHER:
			ACameraView.MoveTo(skillCasterCluster.WorldPositionFocus, CameraView.AnimationMoveSpeed, (Ease)0);
			break;
		}
		((CLogger<T>)this).Log((object)$"Waiting for {WaitBeforeSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration}s. --- BeforeSkillGroup", (CLogLevel)0, false, false);
		yield return SharedYields.WaitForSeconds(WaitBeforeSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration);
		yield return ExecuteSkillsForGroups(skillCasterCluster);
		TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnBehaviorClusterExecutionEnd);
		((CLogger<T>)this).Log((object)$"Waiting for {WaitAfterSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration}s. --- AfterSkillGroup", (CLogLevel)0, false, false);
		yield return SharedYields.WaitForSeconds(WaitAfterSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration);
		yield return TPSingleton<PlayableUnitManager>.Instance.WaitUntilDeathSequences;
	}

	public virtual IEnumerator PrepareSkillsForGroups()
	{
		if (!turboMode)
		{
			IsDone = false;
			((CLogger<T>)this).Log((object)"Starting skill preparation for groups", (CLogLevel)1, false, false);
			for (int skillGroupIndex = 0; skillGroupIndex < skillGroups.Count; skillGroupIndex++)
			{
				yield return MoveCameraAndExecute(skillGroups[skillGroupIndex]);
			}
			IsDone = true;
		}
	}

	public virtual List<IBehaviorModel> SortBehaviors(List<IBehaviorModel> skillCasters, bool shuffleList = true)
	{
		new List<IBehaviorModel>();
		return (from caster in skillCasters
			orderby caster.BehaviourDefinition.GoalsComputingOrder, TileMapController.DistanceBetweenTiles(TileMapController.GetCenterTile(), caster.OriginTile)
			select caster).ThenBy((IBehaviorModel _) => (!shuffleList) ? 1 : RandomManager.GetRandomRange(TPSingleton<T>.Instance, 0, skillCasters.Count)).ToList();
	}

	protected bool CheckIfSkillGroupsAreDoneWithSkillExecution(List<SkillCasterAttackGroup> skillCasterAttackGroups)
	{
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterAttackGroups)
		{
			foreach (ComputedGoal item in skillCasterAttackGroup.GoalsToExecute)
			{
				if (item.Goal.Owner.IsExecutingSkill)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected virtual IEnumerator ExecuteSkillsForGroups(SkillCasterCluster skillCasterCluster)
	{
		((CLogger<T>)this).Log((object)$"Executing skills for SkillCasterAttackGroups in Cluster {skillCasterCluster.ClusterOrder} with target {skillCasterCluster.TargetType}", (CLogLevel)1, false, false);
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterCluster.SkillCasterAttackGroups)
		{
			ExecuteSkillsForGroup(skillCasterAttackGroup);
		}
		ExecuteSkillSound(skillCasterCluster.SkillCasterAttackGroups);
		yield return (object)new WaitUntil((Func<bool>)(() => CheckIfSkillGroupsAreDoneWithSkillExecution(skillCasterCluster.SkillCasterAttackGroups)));
	}

	protected List<ComputedGoal> GetGoalsToExecute(List<IBehaviorModel> skillCasters)
	{
		List<ComputedGoal> list = new List<ComputedGoal>();
		foreach (IBehaviorModel skillCaster in skillCasters)
		{
			for (int i = 0; i < skillCaster.CurrentGoals.Length; i++)
			{
				if (EnsureUnitTargeting(skillCaster, i) && skillCaster.CurrentGoals[i].Goal.GoalDefinition.SkillId != "SkipTurn" && skillCaster.CurrentGoals[i].Goal.GoalDefinition.SkillId != "GargoyleSkipTurn2")
				{
					list.Add(skillCaster.CurrentGoals[i]);
				}
			}
		}
		return list;
	}

	protected List<IBehaviorModel> RemoveSkippedBehaviours(List<IBehaviorModel> behaviors, bool updateSkippedTurns = true)
	{
		List<IBehaviorModel> list = new List<IBehaviorModel>(behaviors);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].TurnsToSkipOnSpawn > 0)
			{
				if (updateSkippedTurns)
				{
					list[num].TurnsToSkipOnSpawn--;
				}
				list.RemoveAt(num);
			}
		}
		return list;
	}

	private Tile EnsureBuildingTile(SkillCasterAttackGroup.E_Target targetType, Tile defaultTile)
	{
		if (targetType != 0)
		{
			return defaultTile;
		}
		return BuildingManager.MagicCircle.OriginTile;
	}

	private Tile EnsureBuildingTileInLOS(SkillCasterAttackGroup.E_Target targetType, ITileObject iTileObject, TilesInRangeInfos tilesInRangeInfos)
	{
		if (targetType != 0 || !tilesInRangeInfos.IsInLineOfSight(BuildingManager.MagicCircle.OriginTile))
		{
			return tilesInRangeInfos.GetTileInLineOfSight(iTileObject);
		}
		return BuildingManager.MagicCircle.OriginTile;
	}

	private void ExecuteSkillSound(List<SkillCasterAttackGroup> skillCasterAttackGroups)
	{
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterAttackGroups)
		{
			PlaySkillSound(skillCasterAttackGroup.SkillSoundId, skillCasterAttackGroup.GoalsToExecute.Count, skillCasterAttackGroup.GoalsToExecute[0].Goal.Owner.OriginTile, skillCasterAttackGroup.GoalsToExecute[0].TargetTileInfo.Tile);
		}
	}

	protected void PlaySkillSound(string skillId, int behaviorsCount, Tile launchTile, Tile impactTile)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		skillId = (EnemyUnitDatabase.SkillSoundIdOverrides.ContainsKey(skillId) ? EnemyUnitDatabase.SkillSoundIdOverrides[skillId] : skillId);
		if (!skillGroupAudioClips.ContainsKey(skillId))
		{
			ComputeSkillGroupAudioClips(skillId);
		}
		List<RangeDefinition> enemySkillSoundRangeDefinitions = EnemyUnitDatabase.EnemySkillSoundRangeDefinitions;
		int num;
		for (num = enemySkillSoundRangeDefinitions.Count - 1; num > 0; num--)
		{
			Vector2Int range = enemySkillSoundRangeDefinitions[num - 1].Range;
			if (behaviorsCount > ((Vector2Int)(ref range)).y)
			{
				break;
			}
		}
		for (int num2 = num; num2 >= 0; num2--)
		{
			if (TryPlaySkillSound(skillId, launchTile, impactTile, enemySkillSoundRangeDefinitions[num2].Id))
			{
				return;
			}
		}
		((CLogger<T>)this).LogWarning((object)("No sound found for " + skillId), (CLogLevel)1, true, false);
	}

	protected virtual string GetSkillSoundClipPathFormat()
	{
		((CLogger<T>)this).LogError((object)"Skill sound path isn't defined for this behavior manager!", (CLogLevel)1, true, true);
		return string.Empty;
	}

	protected virtual string GetSkillSoundLaunchPathFormat()
	{
		((CLogger<T>)this).LogError((object)"Skill sound (launch) path isn't defined for this behavior manager!", (CLogLevel)1, true, true);
		return string.Empty;
	}

	protected virtual string GetSkillSoundImpactPathFormat()
	{
		((CLogger<T>)this).LogError((object)"Skill sound (impact) path isn't defined for this behavior manager!", (CLogLevel)1, true, true);
		return string.Empty;
	}

	protected virtual OneShotSound GetPooledSkillSoundAudioSource()
	{
		((CLogger<T>)this).LogError((object)"Skill sound audio source isn't defined for this behavior manager!", (CLogLevel)1, true, true);
		return null;
	}

	protected virtual OneShotSound GetSpatializedPooledSkillSoundAudioSource()
	{
		((CLogger<T>)this).LogError((object)"Skill sound audio source isn't defined for this behavior manager!", (CLogLevel)1, true, true);
		return null;
	}

	private void ComputeSkillGroupAudioClips(string skillGroupId)
	{
		AudioClipsPerId audioClipsPerId = new AudioClipsPerId();
		foreach (RangeDefinition enemySkillSoundRangeDefinition in EnemyUnitDatabase.EnemySkillSoundRangeDefinitions)
		{
			audioClipsPerId[enemySkillSoundRangeDefinition.Id] = new AudioClips
			{
				Clips = FetchAllSounds(string.Format(GetSkillSoundClipPathFormat(), skillGroupId, enemySkillSoundRangeDefinition.Id)),
				LaunchClips = FetchAllSounds(string.Format(GetSkillSoundLaunchPathFormat(), skillGroupId, enemySkillSoundRangeDefinition.Id)),
				ImpactClips = FetchAllSounds(string.Format(GetSkillSoundImpactPathFormat(), skillGroupId, enemySkillSoundRangeDefinition.Id))
			};
		}
		skillGroupAudioClips[skillGroupId] = audioClipsPerId;
	}

	private List<AudioClip> FetchAllSounds(string prefix)
	{
		List<AudioClip> list = new List<AudioClip>();
		int num = 1;
		while (true)
		{
			AudioClip val = ResourcePooler<AudioClip>.LoadOnce(string.Format("{0}_{1}{2}", prefix, (num < 10) ? "0" : string.Empty, num), failSilently: true);
			if (!((Object)(object)val != (Object)null))
			{
				break;
			}
			list.Add(val);
			num++;
		}
		return list;
	}

	private bool TryPlaySkillSound(string skillId, Tile launchTile, Tile impactTile, string suffix)
	{
		bool result = false;
		if (skillGroupAudioClips[skillId][suffix].Clips.Count > 0)
		{
			result = true;
			OneShotSound pooledSkillSoundAudioSource = GetPooledSkillSoundAudioSource();
			((Object)((Component)pooledSkillSoundAudioSource).gameObject).name = skillId;
			pooledSkillSoundAudioSource.Play(skillGroupAudioClips[skillId][suffix].Clips.PickRandom());
		}
		if (skillGroupAudioClips[skillId][suffix].LaunchClips.Count > 0)
		{
			result = true;
			OneShotSound spatializedPooledSkillSoundAudioSource = GetSpatializedPooledSkillSoundAudioSource();
			((Object)((Component)spatializedPooledSkillSoundAudioSource).gameObject).name = skillId + "_Launch";
			spatializedPooledSkillSoundAudioSource.PlaySpatialized(skillGroupAudioClips[skillId][suffix].LaunchClips.PickRandom(), launchTile);
		}
		if (skillGroupAudioClips[skillId][suffix].ImpactClips.Count > 0)
		{
			result = true;
			OneShotSound spatializedPooledSkillSoundAudioSource2 = GetSpatializedPooledSkillSoundAudioSource();
			((Object)((Component)spatializedPooledSkillSoundAudioSource2).gameObject).name = skillId + "_Impact";
			spatializedPooledSkillSoundAudioSource2.PlaySpatialized(skillGroupAudioClips[skillId][suffix].ImpactClips.PickRandom(), impactTile);
		}
		return result;
	}
}
