using System.Collections.Generic;
using PortraitAPI;
using PortraitAPI.Layers;
using PortraitAPI.Misc;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class LayerTextureHandler : Handler
{
	private const string IndexFormat = "00";

	private const string Space = " ";

	[SerializeField]
	private E_LayerType layerType;

	[SerializeField]
	private TMP_Dropdown dropDown;

	private int currentValue;

	public override bool IsDropdownOpen => DropDown.IsExpanded;

	public TMP_Dropdown DropDown => dropDown;

	public E_LayerType LayerType => layerType;

	public override void ChangeCurrentValue()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		currentValue = dropDown.value;
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfLayerType(LayerType, dropDown.value);
		if ((int)LayerType == 1)
		{
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshBeardHandler = true;
		}
		dropDown.RefreshShownValue();
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshPortraitParts = true;
	}

	public override void DecreaseCurrentValue()
	{
		currentValue = dropDown.value;
		int valueWithoutNotify = dropDown.options.Count - 1;
		if (currentValue != 0)
		{
			valueWithoutNotify = currentValue - 1;
		}
		dropDown.SetValueWithoutNotify(valueWithoutNotify);
		ChangeCurrentValue();
	}

	public override void IncreaseCurrentValue()
	{
		currentValue = dropDown.value;
		int valueWithoutNotify = 0;
		if (currentValue < dropDown.options.Count - 1)
		{
			valueWithoutNotify = currentValue + 1;
		}
		dropDown.SetValueWithoutNotify(valueWithoutNotify);
		ChangeCurrentValue();
	}

	public override void RandomizeValue(bool useWeights)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Invalid comparison between Unknown and I4
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		if ((int)LayerType == 6)
		{
			Refresh();
		}
		if (dropDown.options.Count <= 0)
		{
			return;
		}
		if (useWeights)
		{
			E_LayerType val = LayerType;
			if ((int)val != 1)
			{
				num = (((int)val != 6) ? IEnumerableExtension.RandomWeightedIndex<SimpleLayer>(LayerManagement.GetTheseLayers<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, layerType), RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitCustomisationPanel>.Instance).GetType().Name)) : IEnumerableExtension.RandomWeightedIndex<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards, RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitCustomisationPanel>.Instance).GetType().Name)));
			}
			else
			{
				string faceId = GetRandomFaceId(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender);
				List<HairLayer> theseLayers = LayerManagement.GetTheseLayers<HairLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, (E_LayerType)1);
				if (theseLayers.Find((HairLayer x) => x.FaceId == faceId) != null)
				{
					num = theseLayers.IndexOf(theseLayers.Find((HairLayer x) => x.FaceId == faceId));
				}
			}
		}
		else
		{
			num = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, ((int)LayerType == 6) ? TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Count : LayerManagement.GetTheseLayers<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, layerType).Count);
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfLayerType(LayerType, num);
		if ((int)LayerType == 1)
		{
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshBeardHandler = true;
		}
		dropDown.SetValueWithoutNotify(num);
	}

	public void Refresh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected I4, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Invalid comparison between Unknown and I4
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Invalid comparison between Unknown and I4
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Expected O, but got Unknown
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Invalid comparison between Unknown and I4
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Expected O, but got Unknown
		if ((int)LayerType != 6)
		{
			goto IL_003f;
		}
		E_Gender val;
		if (!TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType)
		{
			val = (E_Gender)2;
		}
		else
		{
			if (!TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType)
			{
				goto IL_003f;
			}
			val = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[(E_LayerType)1].Gender;
		}
		goto IL_004b;
		IL_003f:
		val = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender;
		goto IL_004b;
		IL_004b:
		E_Gender val2 = val;
		List<SimpleLayer> list = new List<SimpleLayer>();
		List<SimpleLayer> list2 = new List<SimpleLayer>();
		switch ((int)val2)
		{
		case 0:
			list.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>(val2, layerType));
			break;
		case 1:
			list2.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>(val2, layerType));
			break;
		case 2:
			list.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)0, layerType));
			if ((int)layerType != 9)
			{
				list2.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)1, layerType));
			}
			break;
		}
		E_LayerType val3 = LayerType;
		string text = Localizer.Get("HeroCustomization_TextureDropdownTitle_" + ((object)(E_LayerType)(ref val3)).ToString());
		((UnityEvent<int>)(object)dropDown.onValueChanged).RemoveListener(onValueChanged);
		if (dropDown.options != null)
		{
			dropDown.ClearOptions();
		}
		List<OptionData> list3 = new List<OptionData>();
		if ((int)LayerType == 6)
		{
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Clear();
			HairLayer val4 = LayerManagement.GetTheseLayers<HairLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[(E_LayerType)1].Gender, (E_LayerType)1)[TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[(E_LayerType)1].LayerIndex];
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType)
			{
				foreach (SimpleLayer item in list)
				{
					if (((BeardLayers)item).FaceIds.Contains(val4.FaceId))
					{
						TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Add(item);
						list3.Add(new OptionData(text + " " + (list.IndexOf(item) + 1).ToString("00")));
					}
				}
			}
			else
			{
				int num = 0;
				foreach (SimpleLayer item2 in list)
				{
					TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Add(item2);
					list3.Add(new OptionData(text + " " + (num + 1).ToString("00")));
					num++;
				}
			}
		}
		else
		{
			int num2 = 0;
			foreach (SimpleLayer item3 in list)
			{
				_ = item3;
				string text2 = (((int)layerType != 9) ? Localizer.Format("HeroCustomization_TextureDropdownTitle_Male_Formatting", new object[2]
				{
					text,
					(num2 + 1).ToString("00")
				}) : (text + " " + (num2 + 1).ToString("00")));
				list3.Add(new OptionData(text2));
				num2++;
			}
			foreach (SimpleLayer item4 in list2)
			{
				_ = item4;
				string text3 = (((int)layerType != 9) ? Localizer.Format("HeroCustomization_TextureDropdownTitle_Female_Formatting", new object[2]
				{
					text,
					(num2 + 1).ToString("00")
				}) : (text + " " + (num2 + 1).ToString("00")));
				list3.Add(new OptionData(text3));
				num2++;
			}
		}
		if (list3.Count > 0)
		{
			SwitchHandlerLockState(state: true);
			dropDown.AddOptions(list3);
			((UnityEvent<int>)(object)dropDown.onValueChanged).AddListener(onValueChanged);
		}
		else
		{
			SwitchHandlerLockState(state: false);
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[LayerType].SetIndex(-1);
		}
	}

	public override void SwitchHandlerLockState(bool state)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.SwitchHandlerLockState(state);
		((Selectable)dropDown).interactable = state;
		((Graphic)((Selectable)dropDown).image).CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, false, true);
	}

	private string GetRandomFaceId(E_Gender gender)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		UnitFaceIdDefinitions[] array = new UnitFaceIdDefinitions[2];
		switch ((int)gender)
		{
		case 1:
			array[1] = PlayableUnitDatabase.PlayableFemaleUnitFaceIds;
			break;
		case 0:
			array[0] = PlayableUnitDatabase.PlayableMaleUnitFaceIds;
			break;
		case 2:
			array[0] = PlayableUnitDatabase.PlayableMaleUnitFaceIds;
			array[1] = PlayableUnitDatabase.PlayableFemaleUnitFaceIds;
			break;
		}
		string result = string.Empty;
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				for (int j = 0; j < array[i].Count; j++)
				{
					num += array[i][j].Weight;
				}
			}
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, num);
		int num2 = 0;
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k] == null)
			{
				continue;
			}
			int num3 = 0;
			while (num3 < array[k].Count)
			{
				if (randomRange < num2 || randomRange >= array[k][num3].Weight + num2)
				{
					num2 += array[k][num3].Weight;
					num3++;
					continue;
				}
				goto IL_00c5;
			}
			continue;
			IL_00c5:
			result = array[k][num3].FaceId;
			break;
		}
		return result;
	}

	private void Start()
	{
		onValueChanged = delegate
		{
			ChangeCurrentValue();
		};
	}
}
