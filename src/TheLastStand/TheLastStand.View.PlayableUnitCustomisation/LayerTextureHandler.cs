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
	public static class Constants
	{
		public const string DropDownTitleMaleFormatting = "HeroCustomization_TextureDropdownTitle_Male_Formatting";

		public const string DropDownTitleFemaleFormatting = "HeroCustomization_TextureDropdownTitle_Female_Formatting";
	}

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
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Invalid comparison between Unknown and I4
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
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
				num = (((int)val != 6) ? IEnumerableExtension.RandomWeightedIndex<SimpleLayer>(LayerManagement.GetTheseLayers<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, layerType, true), RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitCustomisationPanel>.Instance).GetType().Name)) : IEnumerableExtension.RandomWeightedIndex<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards, RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitCustomisationPanel>.Instance).GetType().Name)));
			}
			else
			{
				string faceId = GetRandomFaceId(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender);
				List<HairLayer> theseLayers = LayerManagement.GetTheseLayers<HairLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, (E_LayerType)1, true);
				if (theseLayers.Find((HairLayer x) => x.FaceId == faceId) != null)
				{
					num = theseLayers.IndexOf(theseLayers.Find((HairLayer x) => x.FaceId == faceId));
				}
			}
		}
		else
		{
			num = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, ((int)LayerType == 6) ? TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Count : LayerManagement.GetTheseLayers<SimpleLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender, layerType, true).Count);
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
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected I4, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Invalid comparison between Unknown and I4
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Invalid comparison between Unknown and I4
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Invalid comparison between Unknown and I4
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Invalid comparison between Unknown and I4
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Expected O, but got Unknown
		E_Gender val = (((int)LayerType != 6 || !TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType) ? TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender : TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[(E_LayerType)1].Gender);
		E_Gender val2 = val;
		List<SimpleLayer> list = new List<SimpleLayer>();
		List<SimpleLayer> list2 = new List<SimpleLayer>();
		switch ((int)val2)
		{
		case 0:
			list.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>(val2, layerType, true));
			break;
		case 1:
			list2.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>(val2, layerType, true));
			break;
		case 2:
			list.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)0, layerType, true));
			if ((int)layerType != 9)
			{
				list2.AddRange(LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)1, layerType, true));
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
			int layerIndex = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[(E_LayerType)1].LayerIndex;
			List<HairLayer> theseLayers = LayerManagement.GetTheseLayers<HairLayer>(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByLayerType[(E_LayerType)1].Gender, (E_LayerType)1, true);
			int count = theseLayers.Count;
			HairLayer hairLayer = null;
			if (layerIndex != -1 && layerIndex < count)
			{
				hairLayer = theseLayers[layerIndex];
			}
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.BeardIsLockToHairType)
			{
				AddBeardsToCurrentBeards(list, list3, text, "HeroCustomization_TextureDropdownTitle_Male_Formatting", isBeardLockedToHair: true, hairLayer);
				AddBeardsToCurrentBeards(list2, list3, text, "HeroCustomization_TextureDropdownTitle_Female_Formatting", isBeardLockedToHair: true, hairLayer);
			}
			else
			{
				AddBeardsToCurrentBeards(list, list3, text, "HeroCustomization_TextureDropdownTitle_Male_Formatting", isBeardLockedToHair: false);
				AddBeardsToCurrentBeards(list2, list3, text, "HeroCustomization_TextureDropdownTitle_Female_Formatting", isBeardLockedToHair: false);
			}
		}
		else
		{
			int num = 0;
			foreach (SimpleLayer item in list)
			{
				_ = item;
				string text2 = (((int)layerType != 9) ? Localizer.Format("HeroCustomization_TextureDropdownTitle_Male_Formatting", new object[2]
				{
					text,
					(num + 1).ToString("00")
				}) : (text + " " + (num + 1).ToString("00")));
				list3.Add(new OptionData(text2));
				num++;
			}
			num = 0;
			foreach (SimpleLayer item2 in list2)
			{
				_ = item2;
				string text3 = (((int)layerType != 9) ? Localizer.Format("HeroCustomization_TextureDropdownTitle_Female_Formatting", new object[2]
				{
					text,
					(num + 1).ToString("00")
				}) : (text + " " + (num + 1).ToString("00")));
				list3.Add(new OptionData(text3));
				num++;
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

	private void AddBeardsToCurrentBeards(List<SimpleLayer> genderedLayers, List<OptionData> optionsData, string localizedLayerType, string localizeFinalTextKey, bool isBeardLockedToHair, HairLayer hairLayer = null)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (SimpleLayer genderedLayer in genderedLayers)
		{
			bool flag = true;
			if (isBeardLockedToHair && hairLayer != null)
			{
				flag = ((BeardLayers)genderedLayer).FaceIds.Contains(hairLayer.FaceId);
			}
			if (flag)
			{
				TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentBeards.Add(genderedLayer);
				string text = Localizer.Format(localizeFinalTextKey, new object[2]
				{
					localizedLayerType,
					(num + 1).ToString("00")
				});
				optionsData.Add(new OptionData(text));
				num++;
			}
		}
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
