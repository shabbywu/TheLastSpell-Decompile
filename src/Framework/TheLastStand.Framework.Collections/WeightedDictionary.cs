using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace TheLastStand.Framework.Collections;

[Serializable]
public class WeightedDictionary<T>
{
	[Serializable]
	public class Weight
	{
		[SerializeField]
		private float odd;

		public float Odd => odd;

		public float Proba { get; set; }

		private string Percent => $"{Mathf.RoundToInt(Proba * 100f)}%";
	}

	[OdinSerialize]
	private Dictionary<T, Weight> weights = new Dictionary<T, Weight>();

	public Dictionary<T, Weight> Weights => weights;

	public float ComputeTotalOdd()
	{
		float num = 0f;
		foreach (KeyValuePair<T, Weight> weight in weights)
		{
			if (weight.Value != null)
			{
				num += weight.Value.Odd;
			}
		}
		return num;
	}

	public void OnInspectorGUI()
	{
		float num = ComputeTotalOdd();
		foreach (KeyValuePair<T, Weight> weight in weights)
		{
			if (weight.Value != null)
			{
				weight.Value.Proba = weight.Value.Odd / num;
			}
		}
	}
}
