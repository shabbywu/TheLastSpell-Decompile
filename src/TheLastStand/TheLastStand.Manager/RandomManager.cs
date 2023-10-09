using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.Manager;

public class RandomManager : Manager<RandomManager>, ISerializable, IDeserializable
{
	private static class Constants
	{
		public static Vector2Int TilesNoiseOffsetRange = new Vector2Int(-10000, 10000);
	}

	[SerializeField]
	[Tooltip("Seed used to init the PRNG for the whole game. Use 0 for a random seed.")]
	private int baseSeed;

	private string lastCaller = string.Empty;

	private Dictionary<string, System.Random> randomLibrary = new Dictionary<string, System.Random>();

	private Dictionary<string, byte[]> savedStates = new Dictionary<string, byte[]>();

	[SerializeField]
	private float debugPerlinRemapOffset = 0.2f;

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static int BaseSeed => TPSingleton<RandomManager>.Instance.baseSeed;

	public static int TilesNoiseRandomOffset { get; protected set; }

	public static int TilesNoiseRandomOffsetBis { get; protected set; }

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static int DebugTilesNoiseOffset
	{
		get
		{
			return TilesNoiseRandomOffset;
		}
		protected set
		{
			TilesNoiseRandomOffset = value;
			Tilemap[] array = Object.FindObjectsOfType<Tilemap>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshAllTiles();
			}
		}
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public static int DebugTilesNoiseOffsetBis
	{
		get
		{
			return TilesNoiseRandomOffsetBis;
		}
		protected set
		{
			TilesNoiseRandomOffsetBis = value;
			Tilemap[] array = Object.FindObjectsOfType<Tilemap>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshAllTiles();
			}
		}
	}

	public static void ClearSavedState(string id)
	{
		if (TPSingleton<RandomManager>.Instance.savedStates.ContainsKey(id))
		{
			((CLogger<RandomManager>)TPSingleton<RandomManager>.Instance).Log((object)("Cleared saved random state for ID " + id), (CLogLevel)1, false, false);
			TPSingleton<RandomManager>.Instance.savedStates.Remove(id);
		}
	}

	public static void ClearSavedState(object caller)
	{
		ClearSavedState(caller.GetType().Name);
	}

	public static float GetHashedWhiteNoise(float x, float y, int offset)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		x = TPHelpers.Frac((x + (float)offset) * 0.1031f);
		y = TPHelpers.Frac((y + (float)offset) * 0.1031f);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(x, y, x);
		val = TPHelpers.Add(val, Vector3.Dot(val, new Vector3(val.y + 19.19f, val.z + 19.19f, val.x + 19.19f)));
		return TPHelpers.Frac((val.x + val.y) * val.z);
	}

	public static float GetHashedWhiteNoise(float x, float y)
	{
		return GetHashedWhiteNoise(x, y, useTilesNoiseRandomOffsetBis: false);
	}

	public static float GetHashedWhiteNoise(float x, float y, bool useTilesNoiseRandomOffsetBis)
	{
		return GetHashedWhiteNoise(x, y, useTilesNoiseRandomOffsetBis ? TilesNoiseRandomOffsetBis : TilesNoiseRandomOffset);
	}

	public static float GetPerlinValue(Vector3Int position, float scale, float offset)
	{
		return TPHelpers.Remap(Mathf.PerlinNoise(((float)((Vector3Int)(ref position)).x + offset) * scale, ((float)((Vector3Int)(ref position)).y + offset) * scale), 0f, 1f, 0f - TPSingleton<RandomManager>.Instance.debugPerlinRemapOffset, 1f + TPSingleton<RandomManager>.Instance.debugPerlinRemapOffset);
	}

	public static int GetPositionBasedRandomIndex(Vector3Int position, int maxVal, bool useAltNoiseOffset = false)
	{
		return Mathf.Clamp(Mathf.RoundToInt(GetHashedWhiteNoise(((Vector3Int)(ref position)).x, ((Vector3Int)(ref position)).y, useAltNoiseOffset) * (float)maxVal), 0, maxVal);
	}

	public static float GetPositionBasedRandomRange(Vector3Int position, float min, float max)
	{
		return GetHashedWhiteNoise(((Vector3Int)(ref position)).x, ((Vector3Int)(ref position)).y) * (max - min) + min;
	}

	public static bool GetRandomBool(string id)
	{
		return GetRandomRange(id, 0f, 1f) > 0.5f;
	}

	public static bool GetRandomBool(object caller)
	{
		return GetRandomRange(caller, 0f, 1f) > 0.5f;
	}

	public static T GetRandomElement<T>(object caller, IEnumerable<T> list)
	{
		return list.ElementAt(GetRandomRange(caller, 0, list.Count()));
	}

	public static T GetRandomElement<T>(string id, IEnumerable<T> list)
	{
		return list.ElementAt(GetRandomRange(id, 0, list.Count()));
	}

	public static int GetRandomRange(string id, int min, int max)
	{
		TPSingleton<RandomManager>.Instance.lastCaller = id;
		return GetRandomRange(GetRandomForCaller(id), min, max);
	}

	public static float GetRandomRange(string id, float min, float max)
	{
		TPSingleton<RandomManager>.Instance.lastCaller = id;
		return GetRandomRange(GetRandomForCaller(id), min, max);
	}

	public static int GetRandomRange(object caller, int min, int max)
	{
		return GetRandomRange(caller.GetType().Name, min, max);
	}

	public static float GetRandomRange(object caller, float min, float max)
	{
		return GetRandomRange(caller.GetType().Name, min, max);
	}

	public static void SaveState(object caller)
	{
		SaveState(caller.GetType().Name);
	}

	public static void SaveState(string id)
	{
		((CLogger<RandomManager>)TPSingleton<RandomManager>.Instance).Log((object)("Saved random state for ID " + id), (CLogLevel)1, false, false);
		System.Random randomForCaller = GetRandomForCaller(id);
		TPSingleton<RandomManager>.Instance.savedStates[id] = SerializeRandom(randomForCaller);
	}

	public static IEnumerable<T> Shuffle<T>(object caller, IEnumerable<T> enumerable)
	{
		return Shuffle(caller.GetType().Name, enumerable);
	}

	public static IEnumerable<T> Shuffle<T>(string id, IEnumerable<T> enumerable)
	{
		System.Random random = GetRandomForCaller(id);
		IEnumerable<T> enumerable2 = from o in enumerable
			select (o) into o
			orderby random.Next()
			select o;
		((CLogger<RandomManager>)TPSingleton<RandomManager>.Instance).Log((object)(id + ":SHUFFLE [" + enumerable.ToString() + " => " + string.Join(", ", enumerable2) + "]"), (CLogLevel)0, false, false);
		return enumerable2;
	}

	public static System.Random GetRandomForCaller(string id)
	{
		if (!TPSingleton<RandomManager>.Instance.randomLibrary.TryGetValue(id, out var value))
		{
			value = new System.Random(TPSingleton<RandomManager>.Instance.baseSeed + id.Length);
			TPSingleton<RandomManager>.Instance.randomLibrary[id] = value;
		}
		return value;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		randomLibrary = new Dictionary<string, System.Random>();
		savedStates = new Dictionary<string, byte[]>();
		if (baseSeed != 0)
		{
			((CLogger<RandomManager>)TPSingleton<RandomManager>.Instance).LogWarning((object)"BaseSeed has been defined, it is not random.", (CLogLevel)2, true, false);
		}
		if (!(container is SerializedRandoms serializedRandoms))
		{
			baseSeed = (int)((baseSeed == 0) ? DateTime.Now.Ticks : baseSeed);
		}
		else
		{
			baseSeed = serializedRandoms.BaseSeed;
			foreach (SerializedRandom item in serializedRandoms.RandomLibrary)
			{
				using MemoryStream serializationStream = new MemoryStream(item.SavedState ?? item.CurrentState);
				System.Random value = new BinaryFormatter().Deserialize(serializationStream) as System.Random;
				randomLibrary.Add(item.CallerID, value);
			}
		}
		((CLogger<RandomManager>)TPSingleton<RandomManager>.Instance).Log((object)$"GAME BASE SEED: {baseSeed}", (CLogLevel)2, false, false);
		SaveState(this);
		TilesNoiseRandomOffset = GetRandomRange(this, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).x, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).y);
		TilesNoiseRandomOffsetBis = GetRandomRange(this, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).x, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).y);
	}

	public ISerializedData Serialize()
	{
		return new SerializedRandoms
		{
			BaseSeed = baseSeed,
			RandomLibrary = randomLibrary.Select((KeyValuePair<string, System.Random> o) => new SerializedRandom
			{
				CallerID = o.Key,
				SavedState = (savedStates.ContainsKey(o.Key) ? savedStates[o.Key] : null),
				CurrentState = SerializeRandom(GetRandomForCaller(o.Key))
			}).ToList()
		};
	}

	private static int GetRandomRange(System.Random random, int min, int max)
	{
		return random.Next(min, max);
	}

	private static float GetRandomRange(System.Random random, float min, float max)
	{
		return (float)random.NextDouble() * (max - min) + min;
	}

	private static byte[] SerializeRandom(System.Random random)
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, random);
		return memoryStream.ToArray();
	}

	[ContextMenu("Refresh Tiles Noise Offset")]
	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	public void DebugRefreshTilesPerlinOffset()
	{
		((CLogger<RandomManager>)TPSingleton<RandomManager>.Instance).Log((object)"Refreshed tiles' Noise offset", (CLogLevel)1, false, false);
		DebugTilesNoiseOffset = GetRandomRange(this, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).x, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).y);
		DebugTilesNoiseOffsetBis = GetRandomRange(this, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).x, ((Vector2Int)(ref Constants.TilesNoiseOffsetRange)).y);
	}
}
