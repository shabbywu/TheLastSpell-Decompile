using System;
using System.Collections.Generic;
using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View;

public class GaugeMarkersDisplayer
{
	private struct GaugeMarkersData
	{
		public int Threshold;

		public int MinIndex;

		public int MaxIndex;
	}

	private readonly RectTransform markersContainer;

	private int lastEnabledMarkerIndex;

	private float lastNormalizedValue;

	private readonly List<Tuple<Image, float>> markers = new List<Tuple<Image, float>>();

	public GaugeMarkersDisplayer(RectTransform markersContainer)
	{
		this.markersContainer = markersContainer;
	}

	public void RefreshEnabledMarkers(float normalizedValue)
	{
		if (lastNormalizedValue < normalizedValue)
		{
			while (lastEnabledMarkerIndex + 1 < markers.Count && markers[lastEnabledMarkerIndex + 1].Item2 < normalizedValue)
			{
				lastEnabledMarkerIndex++;
				((Behaviour)markers[lastEnabledMarkerIndex].Item1).enabled = true;
			}
		}
		else
		{
			while (lastEnabledMarkerIndex >= 0 && markers[lastEnabledMarkerIndex].Item2 >= normalizedValue)
			{
				((Behaviour)markers[lastEnabledMarkerIndex--].Item1).enabled = false;
			}
		}
		lastNormalizedValue = normalizedValue;
	}

	public void RefreshMarkers(float gaugeMaxValue, float gaugeValue, int markerWidth = 1)
	{
		DeactivatesMarkers();
		GaugeMarkersData gaugeMarkersData = FetchGaugeMarkersData(gaugeMaxValue, markerWidth);
		InstantiateGaugeMarkers(gaugeMarkersData, gaugeMaxValue, gaugeValue, markerWidth);
		lastNormalizedValue = gaugeValue / gaugeMaxValue;
	}

	private void DeactivatesMarkers()
	{
		for (int num = markers.Count - 1; num >= 0; num--)
		{
			((Component)markers[num].Item1).gameObject.SetActive(false);
		}
		markers.Clear();
		lastEnabledMarkerIndex = -1;
	}

	private GaugeMarkersData FetchGaugeMarkersData(float gaugeMaxValue, int markerWidth)
	{
		GaugeMarkersData gaugeMarkersData = default(GaugeMarkersData);
		gaugeMarkersData.MinIndex = 0;
		gaugeMarkersData.MaxIndex = -1;
		GaugeMarkersData result = gaugeMarkersData;
		int num = 0;
		while (num < UIManager.GaugeMarkersData.Count && IsThresholdTooSmall(UIManager.GaugeMarkersData[num], gaugeMaxValue, markerWidth))
		{
			num++;
			result.MinIndex++;
			result.MaxIndex++;
		}
		while (num < UIManager.GaugeMarkersData.Count && (float)UIManager.GaugeMarkersData[num].Threshold < gaugeMaxValue)
		{
			num++;
			result.MaxIndex++;
		}
		if (result.MinIndex < UIManager.GaugeMarkersData.Count)
		{
			result.Threshold = UIManager.GaugeMarkersData[result.MinIndex].Threshold;
		}
		return result;
	}

	private void InstantiateGaugeMarkers(GaugeMarkersData gaugeMarkersData, float gaugeMaxValue, float gaugeValue, int markerWidth)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (gaugeMarkersData.MaxIndex < gaugeMarkersData.MinIndex)
		{
			return;
		}
		float num = gaugeMaxValue / (float)gaugeMarkersData.Threshold;
		int num2 = (int)num;
		if (num2 <= 0)
		{
			return;
		}
		Rect rect = markersContainer.rect;
		float width = ((Rect)(ref rect)).width;
		float num3 = width / num;
		float num4 = gaugeValue / gaugeMaxValue;
		for (int i = 0; i < num2; i++)
		{
			float num5 = (float)(i + 1) * num3 / width;
			for (int num6 = gaugeMarkersData.MaxIndex; num6 >= gaugeMarkersData.MinIndex; num6--)
			{
				if ((i + 1) % (UIManager.GaugeMarkersData[num6].Threshold / UIManager.GaugeMarkersData[gaugeMarkersData.MinIndex].Threshold) == 0)
				{
					InstantiateGaugeMarker(UIManager.GaugeMarkersData[num6], width, markerWidth, num5, num5 < num4);
					break;
				}
			}
		}
	}

	private void InstantiateGaugeMarker(GaugeMarkerData gaugeMarkerData, float containerWidth, int markerWidth, float markerNormalizedPosition, bool enabled)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ObjectPooler.GetPooledGameObject("GaugeMarker", UIManager.GaugeMarker, (Transform)(object)markersContainer, dontSetParent: true).transform;
		if (enabled)
		{
			lastEnabledMarkerIndex++;
		}
		Image component = ((Component)transform).GetComponent<Image>();
		((Behaviour)component).enabled = enabled;
		((Graphic)component).color = gaugeMarkerData.Color;
		transform.SetParent((Transform)(object)markersContainer, false);
		Transform transform2 = ((Component)transform).transform;
		Vector3 localPosition = transform2.localPosition;
		transform.localPosition = new Vector3(markerNormalizedPosition * containerWidth - (float)markerWidth / 2f, localPosition.y, localPosition.z);
		RectTransform val = (RectTransform)transform2;
		float num = markerWidth;
		Rect rect = markersContainer.rect;
		val.sizeDelta = new Vector2(num, ((Rect)(ref rect)).height);
		markers.Add(new Tuple<Image, float>(component, markerNormalizedPosition));
	}

	private bool IsThresholdTooSmall(GaugeMarkerData gaugeMarkerData, float gaugeMaxValue, int markerWidth)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		float num = gaugeMaxValue / (float)gaugeMarkerData.Threshold;
		Rect rect = markersContainer.rect;
		float num2 = ((Rect)(ref rect)).width / num;
		if (gaugeMarkerData.MaxValueToUse <= 0 || !((float)gaugeMarkerData.MaxValueToUse < gaugeMaxValue))
		{
			return num2 < (float)markerWidth * UIManager.GaugeMarkerSectionSizeRatio;
		}
		return true;
	}
}
