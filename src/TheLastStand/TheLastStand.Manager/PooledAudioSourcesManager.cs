using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.Manager;

public class PooledAudioSourcesManager : Manager<PooledAudioSourcesManager>
{
	[SerializeField]
	private PooledAudioSourceData defaultPooledAudioSourceData;

	[SerializeField]
	[Tooltip("The maximum amount of active pooled audio sources the system can support.")]
	private int maxGlobalPooledAudioSources = 10;

	[SerializeField]
	[Tooltip("The maximum amount of active pooled audio sources the system can support.")]
	private int maxPooledAudioSources = 10;

	private readonly Dictionary<PooledAudioSourceData, List<AudioSource>> pooledAudioSources = new Dictionary<PooledAudioSourceData, List<AudioSource>>();

	private Dictionary<PooledAudioSourceData, int> usedPooledAudioSources = new Dictionary<PooledAudioSourceData, int>();

	private int globalUsedPooledAudioSources;

	public static PooledAudioSourceData DefaultPooledAudioSourceData => TPSingleton<PooledAudioSourcesManager>.Instance.defaultPooledAudioSourceData;

	public static AudioSource GetAvailablePooledAudioSource(PooledAudioSourceData pooledAudioSourceData)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if (!TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources.ContainsKey(pooledAudioSourceData))
		{
			TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources.Add(pooledAudioSourceData, new List<AudioSource>(1));
			GameObject val = new GameObject(((Object)pooledAudioSourceData).name);
			val.transform.parent = ((Component)TPSingleton<PooledAudioSourcesManager>.Instance).transform;
			pooledAudioSourceData.poolParent = val.transform;
		}
		AudioSource val2 = ((IEnumerable<AudioSource>)TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources[pooledAudioSourceData]).FirstOrDefault((Func<AudioSource, bool>)((AudioSource audioSource) => !((Component)audioSource).gameObject.activeSelf));
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = Object.Instantiate<AudioSource>(pooledAudioSourceData.AudioSourcePrefab, pooledAudioSourceData.poolParent);
			TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources[pooledAudioSourceData].Add(val2);
		}
		((Component)val2).gameObject.SetActive(true);
		return val2;
	}

	public void IncreaseUsedPooledAudioSources(PooledAudioSourceData pooledAudioSourceData, int value)
	{
		usedPooledAudioSources.AddValueOrCreateKey(pooledAudioSourceData, value, (int a, int b) => a + b);
		globalUsedPooledAudioSources += value;
	}

	public void DecreaseUsedPooledAudioSources(PooledAudioSourceData pooledAudioSourceData, int value)
	{
		usedPooledAudioSources[pooledAudioSourceData] -= value;
		globalUsedPooledAudioSources -= value;
	}

	public bool IsPoolFull(PooledAudioSourceData pooledAudioSourceData)
	{
		if (globalUsedPooledAudioSources < maxGlobalPooledAudioSources)
		{
			if (usedPooledAudioSources.TryGetValue(pooledAudioSourceData, out var value))
			{
				return value >= maxPooledAudioSources;
			}
			return false;
		}
		return true;
	}
}
