using System.Collections.Generic;
using System.Linq;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.SpawnWave;
using UnityEngine;

namespace TheLastStand.Model.Unit.Enemy;

public class SpawnPointInfo : ISerializable, IDeserializable
{
	public SpawnDirectionsDefinition.E_Direction Direction;

	public readonly List<List<Tile>> SpawnPoints = new List<List<Tile>>();

	public readonly List<Tile> SpawnPointsBases = new List<Tile>();

	public SpawnDirectionsDefinition.SpawnDirectionInfo SpawnDirectionInfo;

	public SpawnPointInfo(SpawnDirectionsDefinition.E_Direction direction, SpawnDirectionsDefinition.SpawnDirectionInfo spawnDirectionInfo)
	{
		Direction = direction;
		SpawnDirectionInfo = spawnDirectionInfo;
	}

	public SpawnPointInfo(SerializedSpawnPointInfo serializedSpawnPointInfo, int saveVersion)
	{
		Deserialize(serializedSpawnPointInfo, saveVersion);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		SerializedSpawnPointInfo serializedSpawnPointInfo = container as SerializedSpawnPointInfo;
		Direction = (SpawnDirectionsDefinition.E_Direction)serializedSpawnPointInfo.Direction;
		int count = serializedSpawnPointInfo.SpawnPointsPositions.Count;
		for (int i = 0; i < count; i++)
		{
			List<SerializableVector2Int> list = serializedSpawnPointInfo.SpawnPointsPositions[i];
			int count2 = list.Count;
			List<Tile> list2 = new List<Tile>(count2);
			for (int j = 0; j < count2; j++)
			{
				Vector2Int val = list[j].Deserialize();
				list2.Add(TileMapManager.GetTile(((Vector2Int)(ref val)).x, ((Vector2Int)(ref val)).y));
			}
			SpawnPoints.Add(list2);
		}
		int count3 = serializedSpawnPointInfo.SpawnPointsBasesPositions.Count;
		for (int k = 0; k < count3; k++)
		{
			Vector2Int val2 = serializedSpawnPointInfo.SpawnPointsBasesPositions[k].Deserialize();
			SpawnPointsBases.Add(TileMapManager.GetTile(((Vector2Int)(ref val2)).x, ((Vector2Int)(ref val2)).y));
		}
		SpawnDirectionInfo = serializedSpawnPointInfo.SpawnDirectionInfo;
	}

	public ISerializedData Serialize()
	{
		return new SerializedSpawnPointInfo
		{
			Direction = (int)Direction,
			SpawnPointsPositions = SpawnPoints.Select((List<Tile> spawnPoints) => spawnPoints.Select((Tile tile) => new SerializableVector2Int(tile.Position)).ToList()).ToList(),
			SpawnPointsBasesPositions = SpawnPointsBases.Select((Tile tile) => new SerializableVector2Int(tile.Position)).ToList(),
			SpawnDirectionInfo = SpawnDirectionInfo
		};
	}
}
