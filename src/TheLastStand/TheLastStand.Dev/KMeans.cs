using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheLastStand.Dev;

public static class KMeans
{
	private static System.Random rnd = new System.Random();

	public static KMeansClustersInfo GetBestClusters(List<Vector2> data, int k, int repetitions = 1, int maxIterations = 50, int[] initialDataIndexCentroids = null, Func<KMeansClustersInfo, bool> clusterValidator = null)
	{
		KMeansClustersInfo kMeansClustersInfo = null;
		bool? flag = null;
		for (int i = 0; i < repetitions; i++)
		{
			KMeansClustersInfo clusters = GetClusters(data, k, maxIterations, initialDataIndexCentroids);
			bool? flag2 = clusterValidator?.Invoke(clusters);
			if ((kMeansClustersInfo == null || flag != true || flag2 == true) && (kMeansClustersInfo == null || (flag2 == true && flag != true) || (flag2 != false && clusters.totalDistance < kMeansClustersInfo.totalDistance)))
			{
				kMeansClustersInfo = clusters;
				flag = flag2;
			}
			if (kMeansClustersInfo.totalDistance == 0f)
			{
				break;
			}
		}
		return kMeansClustersInfo;
	}

	public static KMeansClustersInfo GetClusters(List<Vector2> data, int k, int maxIterations, int[] initialDataIndexCentroids = null)
	{
		KMeansClustersInfo kMeansClustersInfo = new KMeansClustersInfo(data.Count, k);
		bool flag = true;
		if (initialDataIndexCentroids != null && initialDataIndexCentroids.Length == k)
		{
			kMeansClustersInfo.dataIndexCentroids = initialDataIndexCentroids;
			AssignClusters(data, kMeansClustersInfo, k);
		}
		else
		{
			kMeansClustersInfo.clusterIdByData = RandomlyAssignClusters(data.Count, k);
		}
		while (flag && ++kMeansClustersInfo.iterations < maxIterations)
		{
			UpdateClustersInfo(data, kMeansClustersInfo, k);
			flag = AssignClusters(data, kMeansClustersInfo, k);
		}
		return kMeansClustersInfo;
	}

	private static bool AssignClusters(List<Vector2> data, KMeansClustersInfo kMeansClustersInfo, int clusterCount)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		for (int i = 0; i < data.Count; i++)
		{
			float num = float.MaxValue;
			int num2 = -1;
			for (int j = 0; j < clusterCount; j++)
			{
				Vector2 val = data[i] - kMeansClustersInfo.means[j];
				float magnitude = ((Vector2)(ref val)).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					num2 = j;
				}
			}
			if (num2 != -1 && kMeansClustersInfo.clusterIdByData[i] != num2)
			{
				result = true;
				kMeansClustersInfo.clusterIdByData[i] = num2;
			}
		}
		return result;
	}

	private static void UpdateClustersInfo(List<Vector2> data, KMeansClustersInfo kMeansClustersInfo, int clusterCount)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		kMeansClustersInfo.clusterSizes = new int[clusterCount];
		kMeansClustersInfo.maxDistanceByCluster = (Vector2[])(object)new Vector2[clusterCount];
		kMeansClustersInfo.minDistanceByCluster = ((IEnumerable<Vector2>)(object)new Vector2[clusterCount]).Select((Vector2 x) => Vector2.one * float.MaxValue).ToArray();
		kMeansClustersInfo.means = (Vector2[])(object)new Vector2[clusterCount];
		kMeansClustersInfo.totalDistance = 0f;
		for (int i = 0; i < data.Count; i++)
		{
			Vector2 val = data[i];
			int num = kMeansClustersInfo.clusterIdByData[i];
			kMeansClustersInfo.clusterSizes[num]++;
			ref Vector2 reference = ref kMeansClustersInfo.means[num];
			reference += val;
		}
		for (int j = 0; j < clusterCount; j++)
		{
			int num2 = kMeansClustersInfo.clusterSizes[j];
			ref Vector2 reference2 = ref kMeansClustersInfo.means[j];
			reference2 /= (float)((num2 <= 0) ? 1 : num2);
		}
		for (int k = 0; k < data.Count; k++)
		{
			int num3 = kMeansClustersInfo.clusterIdByData[k];
			Vector2 val2 = data[k] - kMeansClustersInfo.means[num3];
			kMeansClustersInfo.totalDistance += ((Vector2)(ref val2)).magnitude;
			if (((Vector2)(ref val2)).magnitude < ((Vector2)(ref kMeansClustersInfo.minDistanceByCluster[num3])).magnitude)
			{
				kMeansClustersInfo.minDistanceByCluster[num3] = val2;
				kMeansClustersInfo.dataIndexCentroids[num3] = k;
			}
			if (Mathf.Abs(val2.x) > kMeansClustersInfo.maxDistanceByCluster[num3].x)
			{
				kMeansClustersInfo.maxDistanceByCluster[num3].x = Mathf.Abs(val2.x);
			}
			if (Mathf.Abs(val2.y) > kMeansClustersInfo.maxDistanceByCluster[num3].y)
			{
				kMeansClustersInfo.maxDistanceByCluster[num3].y = Mathf.Abs(val2.y);
			}
		}
	}

	private static int[] RandomlyAssignClusters(int dataCount, int clusterCount)
	{
		int[] array = new int[dataCount];
		for (int i = 0; i < dataCount; i++)
		{
			array[i] = rnd.Next(0, clusterCount);
		}
		return array;
	}
}
