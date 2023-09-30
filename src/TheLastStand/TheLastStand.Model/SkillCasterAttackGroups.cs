using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TPLib.Log;
using TheLastStand.Dev;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model;

public class SkillCasterAttackGroups : List<SkillCasterCluster>
{
	public static bool MaxDistancesAreInBound(KMeansClustersInfo clusters)
	{
		return !clusters.maxDistanceByCluster.Any((Vector2 distance) => Mathf.Abs(distance.x) > CameraView.CameraVision.ColliderSize.x / 2f || Mathf.Abs(distance.y) > CameraView.CameraVision.ColliderSize.y / 2f);
	}

	public void Init(List<ComputedGoal> goalsToExecute)
	{
		if (goalsToExecute.Count == 0)
		{
			return;
		}
		List<ComputedGoal> list = goalsToExecute.Where((ComputedGoal goal) => goal.TargetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO || goal.TargetType == SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE).ToList();
		List<ComputedGoal> list2 = goalsToExecute.Except(list).ToList();
		KMeansClustersInfo kMeansClustersInfo = null;
		if (list2.Count > 0)
		{
			int[] array = GenerateBaseKClusterCentroids(list2.Select((ComputedGoal computedGoal) => computedGoal.TargetTileInfo.Tile).ToList());
			int num = Mathf.Max(0, array.Length);
			int num2 = Mathf.Max(0, (num - num % 2) / 2 - 1);
			int totalWeight = Mathf.Max(1, TPSingleton<SectorManager>.Instance.TargetFocusCameraWeight + TPSingleton<SectorManager>.Instance.CasterFocusCameraWeight);
			List<Vector2> data = list2.Select((ComputedGoal computedGoal) => (TileMapView.GetTileCenter(computedGoal.TargetTileInfo.Tile) * (float)TPSingleton<SectorManager>.Instance.TargetFocusCameraWeight + TileMapView.GetTileCenter(computedGoal.Goal.Owner.OriginTile) * (float)TPSingleton<SectorManager>.Instance.CasterFocusCameraWeight) / (float)totalWeight).ToList();
			while (kMeansClustersInfo == null || !MaxDistancesAreInBound(kMeansClustersInfo))
			{
				kMeansClustersInfo = KMeans.GetBestClusters(data, ++num2, 10, 50, null, MaxDistancesAreInBound);
			}
		}
		Enrich(list2, kMeansClustersInfo, list);
		MergeClustersIfNeeded();
		SortByClusterThenTargetType();
	}

	private void Enrich(List<ComputedGoal> data, KMeansClustersInfo clusters, List<ComputedGoal> specificTargetGoals)
	{
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		if (specificTargetGoals != null)
		{
			for (int i = 0; i < specificTargetGoals.Count; i++)
			{
				IBehaviorModel owner = specificTargetGoals[i].Goal.Owner;
				ComputedGoal computedGoal2 = specificTargetGoals[i];
				Tile tile = computedGoal2.TargetTileInfo.Tile;
				SkillCasterAttackGroup.E_Target targetType2 = computedGoal2.TargetType;
				SkillCasterCluster skillCasterCluster = null;
				if (computedGoal2.CanBeMerged)
				{
					using Enumerator enumerator = GetEnumerator();
					while (enumerator.MoveNext())
					{
						SkillCasterCluster current = enumerator.Current;
						if (current.TargetType == targetType2 && current.CanBeMerged && (current.TargetedPlayableUnits.Any((PlayableUnit x) => computedGoal2.TargetPlayableUnits.Contains(x)) || targetType2 == SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE))
						{
							skillCasterCluster = current;
							break;
						}
					}
				}
				if (skillCasterCluster == null)
				{
					skillCasterCluster = targetType2 switch
					{
						SkillCasterAttackGroup.E_Target.ATTACK_HERO => new SkillCasterCluster(GetAverageUnitWorldPosition(computedGoal2.TargetPlayableUnits), targetType2, computedGoal2.TargetPlayableUnits, -1, default(Vector2), computedGoal2.CanBeMerged), 
						SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE => new SkillCasterCluster(BuildingManager.MagicCircle.OriginTile, targetType2, computedGoal2.TargetPlayableUnits, -1, default(Vector2), computedGoal2.CanBeMerged), 
						_ => new SkillCasterCluster(tile.OriginTile, targetType2, computedGoal2.TargetPlayableUnits, -1, default(Vector2), computedGoal2.CanBeMerged), 
					};
					Add(skillCasterCluster);
				}
				else if (computedGoal2.TargetPlayableUnits.Count != 0)
				{
					LinqExtensions.AddRange<PlayableUnit>(skillCasterCluster.TargetedPlayableUnits, (IEnumerable<PlayableUnit>)computedGoal2.TargetPlayableUnits);
					skillCasterCluster.WorldPositionFocus = GetAverageUnitWorldPosition(skillCasterCluster.TargetedPlayableUnits);
				}
				SkillCasterAttackGroup skillCasterAttackGroup = skillCasterCluster.SkillCasterAttackGroups.FirstOrDefault((SkillCasterAttackGroup attackGroup) => attackGroup.SkillSoundId == computedGoal2.Goal.Skill.SkillDefinition.SoundId);
				if (skillCasterAttackGroup == null)
				{
					skillCasterAttackGroup = new SkillCasterAttackGroup(computedGoal2);
					skillCasterCluster.SkillCasterAttackGroups.Add(skillCasterAttackGroup);
				}
				else if (!skillCasterAttackGroup.GoalsToExecute.Contains(computedGoal2))
				{
					skillCasterAttackGroup.GoalsToExecute.Add(computedGoal2);
				}
				owner.Log($"Added into skillGroup cluster {skillCasterCluster.ClusterOrder} with target {targetType2} and target tile {tile}", (CLogLevel)0);
			}
		}
		if (clusters == null)
		{
			return;
		}
		Vector2 lastCameraPosition = Vector2.op_Implicit(this.LastOrDefault((SkillCasterCluster x) => x.TargetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO)?.WorldPositionFocus ?? this.LastOrDefault()?.WorldPositionFocus ?? ((Component)ACameraView.MainCam).transform.position);
		List<Vector2> list = clusters.means.ToList();
		int[] clusterOrder = new int[clusters.means.Length];
		for (int j = 0; j < clusters.means.Length; j++)
		{
			lastCameraPosition = list.OrderBy(delegate(Vector2 x)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				Vector2 val = x - lastCameraPosition;
				return ((Vector2)(ref val)).magnitude;
			}).First();
			list.Remove(lastCameraPosition);
			clusterOrder[Array.IndexOf(clusters.means, lastCameraPosition)] = j;
		}
		for (int k = 0; k < clusters.clusterIdByData.Length; k++)
		{
			IBehaviorModel owner2 = data[k].Goal.Owner;
			ComputedGoal computedGoal = data[k];
			Tile tile2 = computedGoal.TargetTileInfo.Tile;
			SkillCasterAttackGroup.E_Target targetType = computedGoal.TargetType;
			int clusterId = clusters.clusterIdByData[k];
			SkillCasterCluster skillCasterCluster2 = null;
			if (computedGoal.CanBeMerged)
			{
				skillCasterCluster2 = this.FirstOrDefault((SkillCasterCluster casterCluster) => casterCluster.ClusterOrder == clusterOrder[clusterId] && casterCluster.TargetType == targetType && casterCluster.CanBeMerged);
			}
			if (skillCasterCluster2 == null)
			{
				skillCasterCluster2 = new SkillCasterCluster((computedGoal.Goal.Skill.SkillAction.SkillActionExecution.PreCastFx?.CastFxDefinition != null) ? ((Component)tile2.TileView).transform.position : Vector2.op_Implicit(clusters.means[clusterId]), targetType, null, clusterOrder[clusterId], clusters.maxDistanceByCluster[clusterId], computedGoal.CanBeMerged);
				Add(skillCasterCluster2);
			}
			SkillCasterAttackGroup skillCasterAttackGroup2 = skillCasterCluster2.SkillCasterAttackGroups.FirstOrDefault((SkillCasterAttackGroup attackGroup) => attackGroup.SkillSoundId == computedGoal.Goal.Skill.SkillDefinition.SoundId);
			if (skillCasterAttackGroup2 == null)
			{
				skillCasterAttackGroup2 = new SkillCasterAttackGroup(computedGoal);
				skillCasterCluster2.SkillCasterAttackGroups.Add(skillCasterAttackGroup2);
			}
			else if (!skillCasterAttackGroup2.GoalsToExecute.Contains(computedGoal))
			{
				skillCasterAttackGroup2.GoalsToExecute.Add(computedGoal);
			}
			owner2.Log($"Added into skillGroup cluster {skillCasterCluster2.ClusterOrder} with target {targetType} and target tile {tile2}", (CLogLevel)0);
		}
	}

	private int[] GenerateBaseKClusterCentroids(List<Tile> data)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<CameraAreaOfInterest, int> dictionary = new Dictionary<CameraAreaOfInterest, int>();
		foreach (Tile datum in data)
		{
			Vector3 tileWorldPos = Vector2.op_Implicit(TileMapView.GetTileCenter(datum));
			IEnumerable<CameraAreaOfInterest> source = from x in TPSingleton<SectorManager>.Instance.Sectors
				where x.AreaCollider.OverlapPoint(Vector2.op_Implicit(tileWorldPos))
				orderby x.AreaWeight descending
				select x;
			CameraAreaOfInterest cameraAreaOfInterest = ((!source.Any()) ? TPSingleton<SectorManager>.Instance.Sectors.OrderBy(delegate(CameraAreaOfInterest x)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				Vector3 val3 = ((Component)x).transform.position - tileWorldPos;
				return ((Vector3)(ref val3)).magnitude - x.AreaWeight;
			}).First() : source.First());
			if (!dictionary.ContainsKey(cameraAreaOfInterest))
			{
				dictionary.Add(cameraAreaOfInterest, data.IndexOf(datum));
				continue;
			}
			Vector3 val = Vector2.op_Implicit(TileMapView.GetTileCenter(data[dictionary[cameraAreaOfInterest]]));
			Vector3 val2 = ((Component)cameraAreaOfInterest).transform.position - tileWorldPos;
			float num = Mathf.Abs(((Vector3)(ref val2)).magnitude);
			val2 = ((Component)cameraAreaOfInterest).transform.position - val;
			if (num < Mathf.Abs(((Vector3)(ref val2)).magnitude))
			{
				dictionary[cameraAreaOfInterest] = data.IndexOf(datum);
			}
		}
		return dictionary.Values.ToArray();
	}

	private Vector3 GetAverageUnitWorldPosition(HashSet<PlayableUnit> units)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		foreach (PlayableUnit unit in units)
		{
			val += unit.DamageableView.GameObject.transform.position;
		}
		return val / (float)units.Count;
	}

	private void MergeClustersIfNeeded()
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		List<SkillCasterCluster> clustersToRemove = new List<SkillCasterCluster>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SkillCasterCluster skillCasterCluster = enumerator.Current;
				if (skillCasterCluster.TargetType != SkillCasterAttackGroup.E_Target.ATTACK_HERO || !skillCasterCluster.CanBeMerged)
				{
					continue;
				}
				SkillCasterCluster skillCasterCluster2 = (from cluster in this
					where !clustersToRemove.Contains(cluster) && cluster.CanBeMerged && cluster.TargetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO && cluster.TargetedPlayableUnits.Any((PlayableUnit unit) => skillCasterCluster.TargetedPlayableUnits.Contains(unit))
					orderby cluster.TargetedPlayableUnits.Count descending
					select cluster).FirstOrDefault();
				if (skillCasterCluster2 != null && skillCasterCluster2 != skillCasterCluster && skillCasterCluster2.TargetedPlayableUnits.Count >= skillCasterCluster.TargetedPlayableUnits.Count)
				{
					clustersToRemove.Add(skillCasterCluster);
					LinqExtensions.AddRange<PlayableUnit>(skillCasterCluster2.TargetedPlayableUnits, (IEnumerable<PlayableUnit>)skillCasterCluster.TargetedPlayableUnits);
					skillCasterCluster2.SkillCasterAttackGroups.AddRange(skillCasterCluster.SkillCasterAttackGroups);
					skillCasterCluster2.WorldPositionFocus = GetAverageUnitWorldPosition(skillCasterCluster2.TargetedPlayableUnits);
				}
			}
		}
		foreach (SkillCasterCluster item in clustersToRemove)
		{
			Remove(item);
		}
	}

	private void SortByClusterThenTargetType()
	{
		Sort(delegate(SkillCasterCluster x, SkillCasterCluster y)
		{
			int num = x.ClusterOrder.CompareTo(y.ClusterOrder);
			if (num == 0 || x.ClusterOrder == -1 || y.ClusterOrder == -1)
			{
				num = x.TargetType.CompareTo(y.TargetType);
			}
			return num;
		});
	}
}
