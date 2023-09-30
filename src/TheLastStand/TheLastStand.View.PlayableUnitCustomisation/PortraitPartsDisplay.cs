using System;
using System.Collections.Generic;
using PortraitAPI;
using PortraitAPI.Layers;
using Sirenix.OdinInspector;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PortraitPartsDisplay : SerializedMonoBehaviour
{
	[SerializeField]
	private Dictionary<E_LayerKind, Image> parts = new Dictionary<E_LayerKind, Image>();

	public void UpdateValues(Dictionary<E_LayerType, SimpleLayer> currentLayers, E_Gender gender, PlayableUnit playableUnit, Dictionary<E_LayerKind, Material> layerMaterials)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<E_LayerKind, Texture2D> texturesByLayerKind = LayerManagement.GetTexturesByLayerKind(currentLayers);
		for (int i = 0; i < Enum.GetValues(typeof(E_LayerKind)).Length; i++)
		{
			E_LayerKind key = (E_LayerKind)i;
			Image val = parts[key];
			if (parts.ContainsKey(key) && texturesByLayerKind.ContainsKey(key))
			{
				if ((Object)(object)texturesByLayerKind[key] == (Object)null)
				{
					((Behaviour)val).enabled = false;
					continue;
				}
				((Behaviour)val).enabled = true;
				val.sprite = Sprite.Create(texturesByLayerKind[key], new Rect(0f, 0f, (float)PortraitAPIManager.GetTexturesData().Width, (float)PortraitAPIManager.GetTexturesData().Height), PortraitAPIManager.GetTexturesData().PivotPoint, (float)PortraitAPIManager.GetTexturesData().PixelPerUnit);
				((Graphic)val).material = layerMaterials[key];
			}
			else
			{
				((Behaviour)val).enabled = false;
			}
		}
	}
}
