using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.Controller.Unit.Enemy;

public class SpawnWaveController
{
	public readonly SpawnWave SpawnWave;

	public SpawnWaveController(SpawnWaveDefinition definition, SpawnWaveView spawnWaveView, bool isReroll = false, SpawnDirectionsDefinition spawnDirectionsDefinition = null)
	{
		SpawnWave = new SpawnWave(definition, this, spawnWaveView);
		SpawnWave.SpawnWaveView.SpawnWave = SpawnWave;
		float num = SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave.EvalToFloat((object)TPSingleton<SpawnWaveManager>.Instance.SpawnWaveInterpreterObject);
		SpawnWave.SpawnsCount = ComputeCountWithExternalModifiers(Mathf.RoundToInt(num * SpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier));
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)("Spawn wave: " + SpawnWave.SpawnWaveDefinition.Id), (CLogLevel)2, false, false);
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"I'm evaluating the formula {SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave} to the value {num}.\nMultiplying this value with the scm {SpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier}, rounding and obtaining a SpawnCount of {SpawnWave.SpawnsCount}", (CLogLevel)0, false, false);
		BossManager.ResetPhases();
		if (definition.IsBossWave)
		{
			BossManager.Init(definition.WaveEnemiesDefinition.BossWaveSettings.BossUnitTemplateId);
		}
		PopulateSpawnWave(isReroll);
		SpawnWave.SpawnDirectionsDefinition = spawnDirectionsDefinition ?? SpawnWaveManager.GetRandomSpawnWaveDirection(isReroll);
		if (spawnDirectionsDefinition != null)
		{
			SpawnWave.RotationsCount = 0;
		}
		else
		{
			List<int> list = SpawnWaveManager.AllowedRotationCountsPerDirection[SpawnWave.SpawnDirectionsDefinition];
			if (isReroll && list.Count > 1)
			{
				list.Remove(SpawnWaveManager.RerolledSpawnWave.RotationsCount);
			}
			SpawnWave.RotationsCount = RandomManager.GetRandomElement(TPSingleton<SpawnWaveManager>.Instance, list);
		}
		SpawnWave.RotatedProportionPerDirection = RotateDirection(SpawnWave.SpawnDirectionsDefinition.SpawnDirectionsInfo, SpawnWave.RotationsCount);
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)string.Format("Spawn direction pattern: {0} rotated {1} {2} clockwise.", SpawnWave.SpawnDirectionsDefinition.Id, SpawnWave.RotationsCount, (SpawnWave.RotationsCount > 1) ? "times" : "time"), (CLogLevel)1, false, false);
		if (SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count != SpawnWave.SpawnsCount)
		{
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogWarning((object)$"Something may have gone very wrong in the generation of spawn wave! I expected a spawn count of {SpawnWave.SpawnsCount} but the final number of enemies will be {SpawnWave.RemainingEnemiesToSpawn.Count} + {SpawnWave.RemainingEliteEnemiesToSpawn.Count}(elites).", (CLogLevel)1, true, false);
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Evaluated {SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave} to {num}, multiplied by {SpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier}, rounded and obtained {SpawnWave.SpawnsCount}", (CLogLevel)0, false, false);
		}
		ComputeSpawnPoints();
	}

	public SpawnWaveController(SerializedSpawnWaveContainer serializedSpawnWaveContainer, SpawnWaveView spawnWaveView, int saveVersion)
	{
		SpawnWave = new SpawnWave(serializedSpawnWaveContainer.CurrentSpawnWave, this, spawnWaveView, saveVersion);
		SpawnWave.SpawnWaveView.SpawnWave = SpawnWave;
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)("Spawn wave: " + SpawnWave.SpawnWaveDefinition.Id), (CLogLevel)2, false, false);
		BossManager.ResetPhases();
		if (SpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			BossManager.Init(SpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.BossWaveSettings.BossUnitTemplateId, setCurrentBossPhase: false);
		}
		SpawnWave.RotatedProportionPerDirection = RotateDirection(SpawnWave.SpawnDirectionsDefinition.SpawnDirectionsInfo, SpawnWave.RotationsCount);
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)string.Format("Spawn direction pattern: {0} rotated {1} {2} clockwise.", SpawnWave.SpawnDirectionsDefinition.Id, SpawnWave.RotationsCount, (SpawnWave.RotationsCount > 1) ? "times" : "time"), (CLogLevel)1, false, false);
		ComputeSpawnPoints(serializedSpawnWaveContainer.CurrentSpawnWave, saveVersion);
	}

	public int ComputeDistanceMaxFromCenterWithModifiers()
	{
		return (from x in SpawnWaveManager.SpawnDefinition.DistanceMaxFromCenterPerDays
			where x.Key <= TPSingleton<GameManager>.Instance.Game.DayNumber + 1
			select x into y
			orderby y.Key descending
			select y).FirstOrDefault().Value + TPSingleton<MetaUpgradesManager>.Instance.ComputeDistanceMaxFromCenterModifiers();
	}

	public void PopulateSpawnWave(bool isReroll = false)
	{
		GenerateTierEnemies(SpawnWave, isReroll);
		List<EnemyUnitTemplateDefinition> collection = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, SpawnWave.RemainingEnemiesToSpawn).ToList();
		SpawnWave.RemainingEnemiesToSpawn.Clear();
		SpawnWave.RemainingEnemiesToSpawn.AddRange(collection);
		List<EliteEnemyUnitTemplateDefinition> collection2 = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, SpawnWave.RemainingEliteEnemiesToSpawn).ToList();
		SpawnWave.RemainingEliteEnemiesToSpawn.Clear();
		SpawnWave.RemainingEliteEnemiesToSpawn.AddRange(collection2);
	}

	public Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> RotateDirection(Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> proportionPerDirection, int rotationsCount)
	{
		Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> dictionary = new Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer>(SpawnDirectionsDefinition.SharedDirectionComparer);
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value = null;
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value2 = null;
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value3 = null;
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value4 = null;
		switch (rotationsCount)
		{
		case 0:
		case 4:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value2);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value3);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value4);
			break;
		case 1:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value2);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value3);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value4);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value);
			break;
		case 2:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value3);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value4);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value2);
			break;
		case 3:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value4);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value2);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value3);
			break;
		}
		if (value != null && value.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Top, value);
		}
		if (value2 != null && value2.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Right, value2);
		}
		if (value3 != null && value3.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Bottom, value3);
		}
		if (value4 != null && value4.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Left, value4);
		}
		return dictionary;
	}

	public IEnumerator SpawnEnemies()
	{
		if (SpawnWave.IsPaused)
		{
			yield break;
		}
		SpawnWave.CurrentCustomNightHour++;
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Start of spawn. Spawn wave remaining enemies : {SpawnWave.RemainingEnemiesToSpawn.Count} (+{SpawnWave.RemainingEliteEnemiesToSpawn.Count} elites) / Living enemies : {TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count} (elites included).", (CLogLevel)1, false, false);
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Current custom hour = {SpawnWave.CurrentCustomNightHour}, vs {TPSingleton<GameManager>.Instance.Game.CurrentNightHour}", (CLogLevel)1, false, false);
		if (!SpawnWave.SpawnWaveDefinition.TemporalDistribution.TryGetValue(SpawnWave.CurrentCustomNightHour, out var value) && SpawnWave.UnableToSpawnCount == 0)
		{
			yield break;
		}
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"By definition for night hour {TPSingleton<GameManager>.Instance.Game.CurrentNightHour} of wave {SpawnWave.SpawnWaveDefinition.Id}, turnSpawnsCount is {value}.", (CLogLevel)0, false, false);
		if (SpawnWave.SpawnsCount == 0)
		{
			SpawnWave.SpawnsCount = SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count;
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)$"ZERO enemies in the SpawnCount for the SWC! This is NOT correct. Making that number {SpawnWave.SpawnsCount}. This is likely due to a save corruption.\nPlease send your logs to a developer on discord", (CLogLevel)0, true, true);
		}
		SpawnWave.SuccessfulSpawnCountThisTurn = 0;
		value = (float)SpawnWave.SpawnsCount * value / SpawnWave.SpawnWaveDefinition.TemporalDistributionTotalWeight;
		if (SpawnWave.UnableToSpawnCount > 0)
		{
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Adding {SpawnWave.UnableToSpawnCount} enemies that could not be spawned during the previous turn.", (CLogLevel)2, false, false);
			value += (float)SpawnWave.UnableToSpawnCount;
			SpawnWave.UnableToSpawnCount = 0;
		}
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Trying to spawn {Mathf.RoundToInt(value)} enemies", (CLogLevel)1, false, false);
		if (value / (float)SpawnWave.SpawnsCount > BarkManager.ManyEnemyUnitSpawnPercentage)
		{
			for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num >= 0; num--)
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("ManyEnemyUnitSpawn", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num], BarkManager.DelayPostNewCycleAndEnemiesSpawn);
			}
			TPSingleton<BarkManager>.Instance.Display();
		}
		Dictionary<SpawnDirectionsDefinition.E_Direction, int> dictionary = new Dictionary<SpawnDirectionsDefinition.E_Direction, int>(SpawnDirectionsDefinition.SharedDirectionComparer);
		int num2 = Mathf.RoundToInt(value);
		float num3 = 0f;
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item in SpawnWave.RotatedProportionPerDirection)
		{
			num3 += (float)item.Value.TotalProportion;
		}
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item2 in SpawnWave.RotatedProportionPerDirection)
		{
			int num4 = Mathf.RoundToInt(value * (float)item2.Value.TotalProportion / num3);
			if (num4 > 0)
			{
				((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Calculated {value} * {item2.Value} / {num3} = {num4} spawns for direction {item2.Key}", (CLogLevel)1, false, false);
				dictionary.Add(item2.Key, num4);
				num2 -= num4;
			}
		}
		while (num2 > 0)
		{
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"There are {num2} spawns remaining: spawn them at random direction.", (CLogLevel)1, false, false);
			float num5 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0f, num3);
			foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item3 in SpawnWave.RotatedProportionPerDirection)
			{
				num5 -= (float)item3.Value.TotalProportion;
				if (num5 < 0f)
				{
					if (dictionary.ContainsKey(item3.Key))
					{
						dictionary[item3.Key]++;
					}
					else
					{
						dictionary.Add(item3.Key, 1);
					}
					num2--;
					break;
				}
			}
		}
		if (SpawnWave.IsLastSpawnTurn)
		{
			int num6 = dictionary.Sum((KeyValuePair<SpawnDirectionsDefinition.E_Direction, int> kvp) => kvp.Value);
			int num7 = SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count - num6;
			if (num7 > 0)
			{
				if (dictionary.Count == 0)
				{
					int count = SpawnWave.RotatedProportionPerDirection.Count;
					int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, count);
					dictionary.Add(SpawnWave.RotatedProportionPerDirection.ElementAt(randomRange).Key, num7);
				}
				else
				{
					int randomRange2 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, dictionary.Count);
					dictionary[dictionary.ElementAt(randomRange2).Key] += num7;
				}
			}
		}
		TaskGroup spawnTasks = new TaskGroup((UnityAction)null);
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, int> item4 in dictionary)
		{
			Task val = (Task)new CoroutineTask((MonoBehaviour)(object)TPSingleton<NightTurnsManager>.Instance, SpawnEnemiesFromDirection(item4.Value, item4.Key));
			spawnTasks.AddTask(val);
		}
		spawnTasks.OnCompleteAction = (UnityAction)delegate
		{
			spawnTasks = null;
		};
		spawnTasks.Run();
		yield return (object)new WaitUntil((Func<bool>)(() => spawnTasks == null));
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"End of spawn. Spawn wave remaining enemies : {SpawnWave.RemainingEnemiesToSpawn.Count} (+{SpawnWave.RemainingEliteEnemiesToSpawn.Count}elites) / Living enemies : {TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count}.", (CLogLevel)1, false, false);
	}

	public static int ComputeCountWithExternalModifiers(SpawnWave spawnWave, SpawnDefinition spawnDefinition)
	{
		return ComputeCountWithExternalModifiers(Mathf.RoundToInt(spawnDefinition.SpawnsCountPerWave.EvalToFloat((object)TPSingleton<SpawnWaveManager>.Instance.SpawnWaveInterpreterObject) * spawnWave.SpawnWaveDefinition.SpawnsCountMultiplier));
	}

	private static int ComputeCountWithExternalModifiers(int originalCount)
	{
		float currentWavePercentageModifier = SpawnWaveManager.CurrentWavePercentageModifier;
		return originalCount + Mathf.RoundToInt((float)originalCount * currentWavePercentageModifier * 0.01f);
	}

	public void RecomputeSpawnPoints()
	{
		SpawnWave.SpawnPointsInfo.Clear();
		ComputeSpawnPoints();
	}

	private void ComputeSpawnPoints()
	{
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item in SpawnWave.RotatedProportionPerDirection)
		{
			if (item.Value == null)
			{
				continue;
			}
			foreach (SpawnDirectionsDefinition.SpawnDirectionInfo item2 in item.Value)
			{
				if (item2.Proportion > 0)
				{
					ComputeSpawnPoints(item.Key, item2);
				}
			}
		}
	}

	private void ComputeSpawnPoints(SerializedSpawnWave serializedSpawnWave, int saveVersion)
	{
		for (int num = serializedSpawnWave.SpawnPointsInfo.Count - 1; num >= 0; num--)
		{
			SpawnPointInfo item = new SpawnPointInfo(serializedSpawnWave.SpawnPointsInfo[num], saveVersion);
			SpawnWave.SpawnPointsInfo.Add(item);
		}
	}

	private List<Tile> ComputeSpawnPoint(Tile baseTile, SpawnDirectionsDefinition.E_Direction spawnDirectionDefinition)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		List<Tile> list = new List<Tile> { baseTile };
		Vector2Int val = SpawnWaveManager.SpawnDefinition.SpawnPointRect;
		if (spawnDirectionDefinition == SpawnDirectionsDefinition.E_Direction.Left || spawnDirectionDefinition == SpawnDirectionsDefinition.E_Direction.Right)
		{
			val = VectorExtensions.Swap(val);
		}
		Vector2Int position = baseTile.Position;
		int num = ((Vector2Int)(ref position)).x - ((Vector2Int)(ref val)).x;
		position = baseTile.Position;
		List<Tile> list2 = TileMapController.GetTilesInRect(new RectInt(num, ((Vector2Int)(ref position)).y - ((Vector2Int)(ref val)).y, ((Vector2Int)(ref val)).x * 2, ((Vector2Int)(ref val)).y * 2)).ToList();
		for (int num2 = list2.Count - 1; num2 >= 0; num2--)
		{
			Tile tile = list2[num2];
			if (tile == baseTile || !tile.HasFog || !tile.GroundDefinition.IsCrossable || tile.Unit != null || tile.Building != null)
			{
				list2.RemoveAt(num2);
			}
		}
		list2 = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, list2).ToList();
		for (int i = 0; i < Mathf.Min(list2.Count, SpawnWaveManager.SpawnDefinition.SpawnPointsPerGroup - 1); i++)
		{
			list.Add(list2[i]);
		}
		return list;
	}

	private void ComputeSpawnPoints(SpawnDirectionsDefinition.E_Direction direction, SpawnDirectionsDefinition.SpawnDirectionInfo spawnDirectionInfo)
	{
		SpawnPointInfo spawnPointInfo = new SpawnPointInfo(direction, spawnDirectionInfo);
		SpawnWave.SpawnPointsInfo.Add(spawnPointInfo);
		List<Tile> list = new List<Tile>();
		Tile centerTile = TileMapController.GetCenterTile();
		int num = ComputeDistanceMaxFromCenterWithModifiers();
		int num2;
		int num3;
		int num4;
		int num5;
		switch (direction)
		{
		case SpawnDirectionsDefinition.E_Direction.Bottom:
			num2 = centerTile.X - num;
			num3 = centerTile.X + num;
			num4 = centerTile.Y - TPSingleton<FogManager>.Instance.Fog.DensityValue;
			num5 = num4;
			break;
		case SpawnDirectionsDefinition.E_Direction.Top:
			num2 = centerTile.X - num;
			num3 = centerTile.X + num;
			num4 = centerTile.Y + TPSingleton<FogManager>.Instance.Fog.DensityValue;
			num5 = num4;
			break;
		case SpawnDirectionsDefinition.E_Direction.Left:
			num2 = centerTile.X - TPSingleton<FogManager>.Instance.Fog.DensityValue;
			num3 = num2;
			num4 = centerTile.Y - num;
			num5 = centerTile.Y + num;
			break;
		case SpawnDirectionsDefinition.E_Direction.Right:
			num2 = centerTile.X + TPSingleton<FogManager>.Instance.Fog.DensityValue;
			num3 = num2;
			num4 = centerTile.Y - num;
			num5 = centerTile.Y + num;
			break;
		default:
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)("Unexpected direction in ComputeSpawnPoints : " + direction), (CLogLevel)1, true, true);
			return;
		}
		num2 = Mathf.Clamp(num2, 0, TPSingleton<TileMapManager>.Instance.TileMap.Width);
		num3 = Mathf.Clamp(num3, 0, TPSingleton<TileMapManager>.Instance.TileMap.Width);
		num4 = Mathf.Clamp(num4, 0, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		num5 = Mathf.Clamp(num5, 0, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		if (!spawnDirectionInfo.EquidistantSpawnPoints)
		{
			for (int i = num2; i <= num3; i++)
			{
				for (int j = num4; j <= num5; j++)
				{
					Tile tile = TileMapManager.GetTile(i, j);
					if (tile != null && tile.Unit == null)
					{
						TheLastStand.Model.Building.Building building = tile.Building;
						if ((building == null || !building.IsObstacle) && tile.GroundDefinition.IsCrossable)
						{
							list.Add(tile);
						}
					}
				}
			}
		}
		else
		{
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"Getting equidistant tiles on rotated direction {direction} in spawn wave {SpawnWave.SpawnDirectionsDefinition.Id} generation.", (CLogLevel)0, false, false);
			if (direction == SpawnDirectionsDefinition.E_Direction.Top || direction == SpawnDirectionsDefinition.E_Direction.Bottom)
			{
				num2 += spawnDirectionInfo.EquidistanceBordersMargin;
				num3 -= spawnDirectionInfo.EquidistanceBordersMargin;
			}
			else
			{
				num4 += spawnDirectionInfo.EquidistanceBordersMargin;
				num5 -= spawnDirectionInfo.EquidistanceBordersMargin;
			}
			foreach (Tile item2 in TileMapController.GetEquidistantTilesOnSegment(TileMapManager.GetTile(num2, num4), TileMapManager.GetTile(num3, num5), spawnDirectionInfo.SpawnPointsPerDirectionCount).ToList())
			{
				Tile closestTileFillingConditions = TileMapManager.GetClosestTileFillingConditions(4, item2, (Tile o) => o.Unit == null && (o.Building == null || !o.Building.IsObstacle) && o.GroundDefinition.IsCrossable && !o.HasFog);
				if (closestTileFillingConditions != null)
				{
					list.Add(closestTileFillingConditions);
				}
			}
		}
		if (list.Count == 0)
		{
			((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogWarning((object)"No valid tile found!", (CLogLevel)1, true, false);
			return;
		}
		list = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, list).ToList();
		int num6 = list.Count - 1;
		while (num6 >= 0)
		{
			spawnPointInfo.SpawnPointsBases.Add(list[num6]);
			List<Tile> item = ComputeSpawnPoint(list[num6], direction);
			spawnPointInfo.SpawnPoints.Add(item);
			if (spawnPointInfo.SpawnPoints.Count < spawnDirectionInfo.SpawnPointsPerDirectionCount)
			{
				num6--;
				continue;
			}
			break;
		}
	}

	public static void GenerateElites(SpawnWave spawnWave)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<string, int> eliteEnemyUnitTemplateDefinition in spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EliteEnemyUnitTemplateDefinitions)
		{
			if (!EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.TryGetValue(eliteEnemyUnitTemplateDefinition.Key, out var eliteDefinition))
			{
				((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)("Elite Enemy template id " + eliteEnemyUnitTemplateDefinition.Key + " not found in database. Abort spawn."), (CLogLevel)1, true, true);
				continue;
			}
			if (EnemyUnitDatabase.UnintegratedElites.Contains(eliteDefinition.EliteId))
			{
				while (list.Count < eliteDefinition.Tier)
				{
					list.Add(0);
				}
				list[eliteDefinition.Tier - 1] += eliteEnemyUnitTemplateDefinition.Value;
				continue;
			}
			int num = -1;
			int num2 = 0;
			while (++num < spawnWave.RemainingEnemiesToSpawn.Count && num2 < eliteEnemyUnitTemplateDefinition.Value)
			{
				if (spawnWave.RemainingEnemiesToSpawn[num].Id == eliteDefinition.Id)
				{
					num2++;
				}
			}
			int num3 = eliteEnemyUnitTemplateDefinition.Value - num2;
			if (num3 > 0)
			{
				((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogWarning((object)$"Tried to spawn {eliteEnemyUnitTemplateDefinition.Value} {eliteEnemyUnitTemplateDefinition.Key} but only found {num2} suitable enemies to replace ({eliteDefinition.Id}).", (CLogLevel)1, true, false);
				while (list.Count < eliteDefinition.Tier)
				{
					list.Add(0);
				}
				list[eliteDefinition.Tier - 1] += num3;
			}
			while (num2-- > 0)
			{
				spawnWave.RemainingEnemiesToSpawn.Remove(spawnWave.RemainingEnemiesToSpawn.First((EnemyUnitTemplateDefinition enemyDefinition) => enemyDefinition.Id == eliteDefinition.Id));
				spawnWave.RemainingEliteEnemiesToSpawn.Add(eliteDefinition);
			}
		}
		if (!spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EliteOverrideSpawnDefinition)
		{
			foreach (KeyValuePair<int, Node> item in (SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions.Count >= TPSingleton<GameManager>.Instance.Game.DayNumber + 1) ? SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions[TPSingleton<GameManager>.Instance.Game.DayNumber] : SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions[SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions.Count - 1])
			{
				while (list.Count < item.Key)
				{
					list.Add(0);
				}
				list[item.Key - 1] += item.Value.EvalToInt((InterpreterContext)(object)spawnWave.Interpreter);
			}
		}
		if (list.Count > 0)
		{
			Dictionary<int, List<EnemyUnitTemplateDefinition>> dictionary = new Dictionary<int, List<EnemyUnitTemplateDefinition>>();
			foreach (EnemyUnitTemplateDefinition item2 in spawnWave.RemainingEnemiesToSpawn)
			{
				if (dictionary.ContainsKey(item2.Tier - 1))
				{
					dictionary[item2.Tier - 1].Add(item2);
					continue;
				}
				dictionary.Add(item2.Tier - 1, new List<EnemyUnitTemplateDefinition> { item2 });
			}
			for (int num4 = list.Count - 1; num4 >= 0; num4--)
			{
				int num5 = list[num4];
				if (dictionary.ContainsKey(num4))
				{
					while (num5 > 0 && dictionary[num4].Count > 0)
					{
						int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, dictionary[num4].Count);
						if (EnemyUnitDatabase.EnemyToEliteIds.TryGetValue(dictionary[num4][randomRange].Id, out var value) && !EnemyUnitDatabase.UnintegratedElites.Contains(value))
						{
							num5--;
							spawnWave.RemainingEliteEnemiesToSpawn.Add(EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions[value]);
							spawnWave.RemainingEnemiesToSpawn.Remove(dictionary[num4][randomRange]);
						}
						dictionary[num4].RemoveAt(randomRange);
					}
				}
				if (num5 > 0)
				{
					if (num4 > 0)
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogWarning((object)$"{num5} elites could not be spawned on tier {num4 + 1}. Adding them to the lower tier.", (CLogLevel)2, true, false);
						list[num4 - 1] += num5;
					}
					else
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)$"{num5} elites could not be spawned at all.", (CLogLevel)2, true, true);
					}
				}
			}
		}
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)$"{spawnWave.RemainingEliteEnemiesToSpawn.Count} enemies have been tranformed into elites.", (CLogLevel)1, false, false);
	}

	public static void GenerateTierEnemies(SpawnWave spawnWave, bool isReroll = false)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EnemyUnitTemplateDefinitions);
		List<string> list = TPSingleton<MetaUpgradesManager>.Instance.GetUnavailableEnemiesIds();
		if (SpawnWaveManager.SpawnDefinition.DisallowedEnemies != null)
		{
			list.AddRange(SpawnWaveManager.SpawnDefinition.DisallowedEnemies);
			list = list.Distinct().ToList();
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (dictionary.ContainsKey(list[num]))
			{
				((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)("Removed " + list[num] + " from wave generation pool because it's blocked by a meta upgrade."), (CLogLevel)1, false, false);
				dictionary.Remove(list[num]);
			}
		}
		Dictionary<int, List<EnemyUnitTemplateDefinition>> enemyUnitTemplatesByTierDefinitionsCopy = EnemyUnitDatabase.GetEnemyUnitTemplatesByTierDefinitionsCopy();
		foreach (List<EnemyUnitTemplateDefinition> value2 in enemyUnitTemplatesByTierDefinitionsCopy.Values)
		{
			foreach (string key in dictionary.Keys)
			{
				for (int num2 = value2.Count - 1; num2 >= 0; num2--)
				{
					if (key == value2[num2].Id)
					{
						value2.Remove(value2[num2]);
						break;
					}
				}
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				for (int num4 = value2.Count - 1; num4 >= 0; num4--)
				{
					if (list[num3] == value2[num4].Id)
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)("Removed " + list[num3] + " from wave generation pool because it's blocked by a meta upgrade."), (CLogLevel)1, false, false);
						value2.Remove(value2[num4]);
						break;
					}
				}
			}
			if (!isReroll)
			{
				continue;
			}
			foreach (EnemyUnitTemplateDefinition item in SpawnWaveManager.RerolledSpawnWave.EnemiesToSpawn)
			{
				for (int num5 = value2.Count - 1; num5 >= 0; num5--)
				{
					if (item.Id == value2[num5].Id && value2.Count > spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EnemyTierDefinitions.Count)
					{
						((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)("removing " + item.Id + " from wave generation pool because it was present in previously rerolled wave"), (CLogLevel)0, false, false);
						value2.Remove(value2[num5]);
						break;
					}
				}
			}
		}
		string text = "<b>Added enemies by tier :</b>\n";
		foreach (Tuple<int, float> enemyTierDefinition in spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EnemyTierDefinitions)
		{
			int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, enemyUnitTemplatesByTierDefinitionsCopy[enemyTierDefinition.Item1].Count);
			EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = enemyUnitTemplatesByTierDefinitionsCopy[enemyTierDefinition.Item1][randomRange];
			dictionary.Add(enemyUnitTemplateDefinition.Id, Mathf.RoundToInt((float)enemyUnitTemplateDefinition.Weight * enemyTierDefinition.Item2));
			enemyUnitTemplatesByTierDefinitionsCopy[enemyTierDefinition.Item1].Remove(enemyUnitTemplateDefinition);
			text = text + enemyUnitTemplateDefinition.Id + "\n";
		}
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)(text ?? ""), (CLogLevel)0, false, false);
		float num6 = 0f;
		foreach (int value3 in dictionary.Values)
		{
			num6 += (float)value3;
		}
		for (int i = 0; i < spawnWave.SpawnsCount; i++)
		{
			string text2 = string.Empty;
			float num7 = 0f;
			foreach (KeyValuePair<string, int> item2 in dictionary)
			{
				num7 += (float)(spawnWave.SpawnsCount * item2.Value) / num6;
				if ((float)i < num7)
				{
					text2 = item2.Key;
					break;
				}
			}
			if (text2 != string.Empty)
			{
				if (EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(text2, out var value))
				{
					spawnWave.RemainingEnemiesToSpawn.Add(value);
				}
				else
				{
					((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).LogError((object)("Enemy template id " + text2 + " not found in database"), (CLogLevel)1, true, true);
				}
			}
		}
		GenerateElites(spawnWave);
	}

	private void GetSpawnableTile(Tile spawnPoint, Tile currentTile, Tile centerTile, int maxDistanceFromSpawnPoint, List<Tile> processedTiles, ref Tile[] tiles, ref Tile targetTile, UnitTemplateDefinition unitTemplateDefinition)
	{
		processedTiles.Add(currentTile);
		if (TileMapController.DistanceBetweenTiles(spawnPoint, currentTile) > maxDistanceFromSpawnPoint)
		{
			return;
		}
		if ((Mathf.Abs(currentTile.X - centerTile.X) > TPSingleton<FogManager>.Instance.Fog.DensityValue || Mathf.Abs(currentTile.Y - centerTile.Y) > TPSingleton<FogManager>.Instance.Fog.DensityValue) && unitTemplateDefinition.CanSpawnOn(currentTile))
		{
			targetTile = currentTile;
			return;
		}
		Vector2Int val = default(Vector2Int);
		switch (RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, 4))
		{
		default:
			((Vector2Int)(ref val))._002Ector(1, 1);
			break;
		case 1:
			((Vector2Int)(ref val))._002Ector(1, -1);
			break;
		case 2:
			((Vector2Int)(ref val))._002Ector(-1, 1);
			break;
		case 3:
			((Vector2Int)(ref val))._002Ector(-1, -1);
			break;
		}
		for (int num = currentTile.X - ((Vector2Int)(ref val)).x; num >= currentTile.X + ((Vector2Int)(ref val)).x; num--)
		{
			for (int num2 = currentTile.Y - ((Vector2Int)(ref val)).x; num2 >= currentTile.Y + ((Vector2Int)(ref val)).y; num2--)
			{
				GetSpawnableTile(num, num2, spawnPoint, currentTile, centerTile, maxDistanceFromSpawnPoint, processedTiles, ref tiles, ref targetTile, unitTemplateDefinition);
				if (targetTile != null)
				{
					return;
				}
			}
		}
	}

	private void GetSpawnableTile(int x, int y, Tile spawnPoint, Tile currentTile, Tile centerTile, int maxDistanceFromSpawnPoint, List<Tile> processedTiles, ref Tile[] tiles, ref Tile targetTile, UnitTemplateDefinition unitTemplateDefinition)
	{
		if ((x != currentTile.X || y != currentTile.Y) && (x == currentTile.X || y == currentTile.Y) && x >= 0 && x < TPSingleton<TileMapManager>.Instance.TileMap.Width && y >= 0 && y < TPSingleton<TileMapManager>.Instance.TileMap.Height && tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y] != null && !processedTiles.Contains(tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y]))
		{
			GetSpawnableTile(spawnPoint, tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y], centerTile, maxDistanceFromSpawnPoint, processedTiles, ref tiles, ref targetTile, unitTemplateDefinition);
		}
	}

	private IEnumerator SpawnEnemiesFromDirection(int spawnsCount, SpawnDirectionsDefinition.E_Direction direction)
	{
		string debugString = $"---- Spawn enemies from {direction} ----";
		List<SpawnPointInfo> spawnPointInfoForDirection = SpawnWave.SpawnPointsInfo.Where((SpawnPointInfo x) => x.Direction == direction).ToList();
		float num = spawnPointInfoForDirection.Sum((SpawnPointInfo x) => x.SpawnDirectionInfo.Proportion);
		int[] spawnPointInfoIndexThreshold = new int[spawnPointInfoForDirection.Count];
		int num2 = 0;
		int previousIndexReached = 0;
		for (int i = 0; i < spawnPointInfoForDirection.Count; i++)
		{
			float num3 = (float)spawnPointInfoForDirection[i].SpawnDirectionInfo.Proportion / num;
			spawnPointInfoIndexThreshold[i] = num2 + Mathf.RoundToInt((float)spawnsCount * num3);
			num2 = spawnPointInfoIndexThreshold[i];
		}
		int spawnIndex = 0;
		while (spawnIndex < spawnsCount)
		{
			debugString += $"\nSpawn #{spawnIndex} : ";
			int index = previousIndexReached;
			for (int j = previousIndexReached; j < spawnPointInfoIndexThreshold.Length; j++)
			{
				if (spawnPointInfoIndexThreshold[j] > spawnIndex)
				{
					previousIndexReached = j;
					index = j;
					break;
				}
			}
			SpawnPointInfo spawnPointInfo = spawnPointInfoForDirection[index];
			if (SpawnWave.RemainingEnemiesToSpawn.Count == 0 && SpawnWave.RemainingEliteEnemiesToSpawn.Count == 0)
			{
				debugString += "\nNo spawn remaining in the wave.";
				break;
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count);
			EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = ((randomRange < SpawnWave.RemainingEnemiesToSpawn.Count) ? SpawnWave.RemainingEnemiesToSpawn[randomRange] : SpawnWave.RemainingEliteEnemiesToSpawn[randomRange - SpawnWave.RemainingEnemiesToSpawn.Count]);
			Tile[] tiles = TPSingleton<TileMapManager>.Instance.TileMap.Tiles;
			int randomRange2 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, spawnPointInfo.SpawnPoints.Count);
			List<Tile> list = spawnPointInfo.SpawnPoints[randomRange2];
			int randomRange3 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, list.Count);
			Tile tile = list[randomRange3];
			Tile centerTile = TileMapController.GetCenterTile();
			Tile targetTile = null;
			int num4 = 0;
			int num5 = 0;
			do
			{
				List<Tile> processedTiles = new List<Tile>();
				GetSpawnableTile(tile, tile, centerTile, num4++, processedTiles, ref tiles, ref targetTile, enemyUnitTemplateDefinition);
				num5++;
			}
			while (targetTile == null && num5 < 20);
			if (targetTile == null)
			{
				((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)"No valid tile found to spawn an enemy, keeping him aside for next turn.", (CLogLevel)2, true, false);
				SpawnWave.UnableToSpawnCount++;
			}
			else
			{
				if (enemyUnitTemplateDefinition is EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition)
				{
					debugString += $"Selected tile {tile} (index {randomRange3}) for selected Elite Enemy Id {eliteEnemyUnitTemplateDefinition.EliteId}.";
					EnemyUnitManager.CreateEliteEnemyUnit(eliteEnemyUnitTemplateDefinition, targetTile, new UnitCreationSettings());
					SpawnWave.RemainingEliteEnemiesToSpawn.Remove(eliteEnemyUnitTemplateDefinition);
				}
				else
				{
					debugString += $"Selected tile {tile} (index {randomRange3}) for selected Enemy Id {enemyUnitTemplateDefinition.Id}.";
					EnemyUnitManager.CreateEnemyUnit(enemyUnitTemplateDefinition, targetTile, new UnitCreationSettings());
					SpawnWave.RemainingEnemiesToSpawn.Remove(enemyUnitTemplateDefinition);
				}
				SpawnWave.SuccessfulSpawnCountThisTurn++;
				yield return null;
			}
			int num6 = spawnIndex + 1;
			spawnIndex = num6;
		}
		((CLogger<SpawnWaveManager>)TPSingleton<SpawnWaveManager>.Instance).Log((object)debugString, (CLogLevel)0, false, false);
	}
}
