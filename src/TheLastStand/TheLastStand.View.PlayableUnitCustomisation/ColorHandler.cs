using System.Collections.Generic;
using System.Linq;
using PortraitAPI;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class ColorHandler : Handler
{
	[SerializeField]
	private ColorDropdown colorDropdown;

	[SerializeField]
	private E_ColorTypes colorType = (E_ColorTypes)2;

	[SerializeField]
	private Texture2D colorTexture;

	public E_ColorTypes ColorType => colorType;

	public override bool IsDropdownOpen => ((TMP_BetterDropdown)colorDropdown).IsExpanded;

	public override void ChangeCurrentValue()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, ((TMP_BetterDropdown)colorDropdown).value);
	}

	public override void DecreaseCurrentValue()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		int valueToSet = 0;
		E_ColorTypes val = ColorType;
		if ((int)val > 2)
		{
			if ((int)val == 3)
			{
				valueToSet = PlayableUnitDatabase.PortraitBackgroundColors.Count - 1;
				if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index != 0)
				{
					valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index - 1;
				}
			}
		}
		else
		{
			valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count - 1;
			if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index != 0)
			{
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index - 1;
			}
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, valueToSet);
	}

	public override void IncreaseCurrentValue()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		int valueToSet = 0;
		E_ColorTypes val = ColorType;
		if ((int)val > 2)
		{
			if ((int)val == 3 && TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index < PlayableUnitDatabase.PortraitBackgroundColors.Count - 1)
			{
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index + 1;
			}
		}
		else if (TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count - 1)
		{
			valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index + 1;
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, valueToSet);
	}

	public override void RandomizeValue(bool useWeights)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		int valueToSet = 0;
		E_ColorTypes val = ColorType;
		switch ((int)val)
		{
		case 0:
		case 2:
			if (useWeights)
			{
				int num = 0;
				for (int i = 0; i < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count; i++)
				{
					num += TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(i).Value.Weight;
				}
				int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, num);
				int num2 = 0;
				for (int j = 0; j < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count; j++)
				{
					if (j == 0)
					{
						if (randomRange >= 0 && randomRange < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight)
						{
							valueToSet = j;
							break;
						}
						num2 += TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight;
					}
					else
					{
						if (randomRange >= num2 && randomRange < TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight + num2)
						{
							valueToSet = j;
							break;
						}
						num2 += TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].ElementAt(j).Value.Weight;
					}
				}
			}
			else
			{
				valueToSet = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count);
			}
			break;
		case 1:
			if (useWeights)
			{
				string key = TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[(E_ColorTypes)0].ElementAt(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[(E_ColorTypes)0].Index).Key;
				string randomHairColorId = PlayableUnitDatabase.UnitLinkHairSkin.GetRandomHairColorId(key);
				valueToSet = TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[(E_ColorTypes)1].Keys.ToList().IndexOf(randomHairColorId);
			}
			else
			{
				valueToSet = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, TPSingleton<PlayableUnitCustomisationPanel>.Instance.AllPalettesByColorType[ColorType].Count);
			}
			break;
		case 3:
			valueToSet = RandomManager.GetRandomRange(TPSingleton<PlayableUnitCustomisationPanel>.Instance, 0, PlayableUnitDatabase.PortraitBackgroundColors.Count);
			break;
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.ChangeIndexOfColorType(ColorType, valueToSet);
	}

	public void Refresh(List<DataColor> portraitBackgroundColors, bool clearOptions = true)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		((UnityEvent<int>)(object)((TMP_BetterDropdown)colorDropdown).onValueChanged).RemoveListener(onValueChanged);
		if (((TMP_BetterDropdown)colorDropdown).options != null)
		{
			((TMP_BetterDropdown)colorDropdown).ClearOptions();
		}
		List<ColorOptionData> list = new List<ColorOptionData>();
		foreach (DataColor portraitBackgroundColor in portraitBackgroundColors)
		{
			Sprite val = Sprite.Create(colorTexture, new Rect(new Vector2(0f, 0f), new Vector2((float)((Texture)colorTexture).width, (float)((Texture)colorTexture).height)), new Vector2(0.5f, 0.5f));
			E_ColorTypes val2 = ColorType;
			string text = Localizer.Get("HeroCustomization_ColorDropdownItem_" + ((object)(E_ColorTypes)(ref val2)).ToString() + "_" + ((Object)portraitBackgroundColor).name.Replace(" ", string.Empty));
			list.Add(new ColorOptionData(text, val, portraitBackgroundColor._Color));
		}
		colorDropdown.AddOptions(list);
		((TMP_BetterDropdown)colorDropdown).value = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index;
		((UnityEvent<int>)(object)((TMP_BetterDropdown)colorDropdown).onValueChanged).AddListener(onValueChanged);
	}

	public void Refresh()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		((UnityEvent<int>)(object)((TMP_BetterDropdown)colorDropdown).onValueChanged).RemoveListener(onValueChanged);
		if (((TMP_BetterDropdown)colorDropdown).options != null)
		{
			((TMP_BetterDropdown)colorDropdown).ClearOptions();
		}
		List<ColorOptionData> list = new List<ColorOptionData>();
		foreach (KeyValuePair<string, Texture2D> item in TPSingleton<PlayableUnitCustomisationPanel>.Instance.TexturesByColorType[ColorType])
		{
			Sprite val = Sprite.Create(colorTexture, new Rect(Vector2.zero, new Vector2((float)((Texture)colorTexture).width, (float)((Texture)colorTexture).height)), new Vector2(0.5f, 0.5f));
			Material val2 = new Material(TPSingleton<PlayableUnitCustomisationPanel>.Instance.ColorSwapMaterial);
			val2.SetTexture("_SwapTex", (Texture)(object)item.Value);
			string text = Localizer.Get($"HeroCustomization_ColorDropdownItem_{ColorType}_{item.Key}");
			list.Add(new ColorOptionData(text, val, val2));
		}
		colorDropdown.AddOptions(list);
		((TMP_BetterDropdown)colorDropdown).value = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType[ColorType].Index;
		((UnityEvent<int>)(object)((TMP_BetterDropdown)colorDropdown).onValueChanged).AddListener(onValueChanged);
	}

	private void Start()
	{
		onValueChanged = delegate
		{
			ChangeCurrentValue();
		};
	}
}
