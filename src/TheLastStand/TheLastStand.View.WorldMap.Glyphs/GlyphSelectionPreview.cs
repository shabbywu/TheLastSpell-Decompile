using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphSelectionPreview : MonoBehaviour
{
	[SerializeField]
	private PreviewedGlyphDisplay previewedGlyphDisplayPrefab;

	[SerializeField]
	private RectTransform glyphDisplayContainer;

	[SerializeField]
	private GlyphsHeader glyphsHeader;

	[SerializeField]
	private Button editButton;

	[SerializeField]
	private LayoutNavigationInitializer layoutNavigationInitializer;

	[SerializeField]
	private TextMeshProUGUI lockedCityText;

	private readonly List<PreviewedGlyphDisplay> glyphDisplays = new List<PreviewedGlyphDisplay>();

	public PreviewedGlyphDisplay FirstPreviewedGlyphDisplay
	{
		get
		{
			if (glyphDisplays.Count <= 0)
			{
				return null;
			}
			return glyphDisplays[0];
		}
	}

	public Button EditButton => editButton;

	public void Refresh()
	{
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		glyphsHeader.InitCityPointImages();
		glyphsHeader.RefreshCityPoints();
		SetGlyphDisplayContainerVisible(isVisible: true);
		List<GlyphDefinition> selectedGlyphs = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs;
		while (glyphDisplays.Count > selectedGlyphs.Count)
		{
			Object.DestroyImmediate((Object)(object)((Component)glyphDisplays[0]).gameObject);
			glyphDisplays.RemoveAt(0);
		}
		while (glyphDisplays.Count < selectedGlyphs.Count)
		{
			glyphDisplays.Add(Object.Instantiate<PreviewedGlyphDisplay>(previewedGlyphDisplayPrefab, (Transform)(object)glyphDisplayContainer));
		}
		for (int num = selectedGlyphs.Count - 1; num >= 0; num--)
		{
			glyphDisplays[num].Init(selectedGlyphs[num]);
		}
		layoutNavigationInitializer.InitNavigation(reset: true);
		foreach (PreviewedGlyphDisplay glyphDisplay in glyphDisplays)
		{
			Navigation navigation = ((Selectable)glyphDisplay.JoystickSelectable).navigation;
			if ((Object)(object)((Navigation)(ref navigation)).selectOnUp == (Object)null)
			{
				((Selectable)(object)glyphDisplay.JoystickSelectable).SetSelectOnUp((Selectable)(object)EditButton);
			}
			navigation = ((Selectable)glyphDisplay.JoystickSelectable).navigation;
			if ((Object)(object)((Navigation)(ref navigation)).selectOnDown == (Object)null)
			{
				((Selectable)(object)glyphDisplay.JoystickSelectable).SetSelectOnDown((Selectable)(object)((TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines.Count > 0) ? TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines[0].JoystickSelectable : null));
			}
		}
	}

	public void RefreshLockedUI(WorldMapCity city)
	{
		((Component)lockedCityText).gameObject.SetActive(false);
		if (city.IsUnlocked)
		{
			SetEditButtonVisible(isVisible: true);
			SetGlyphDisplayContainerVisible(isVisible: true);
			return;
		}
		SetEditButtonVisible(isVisible: false);
		SetGlyphDisplayContainerVisible(isVisible: false);
		if (city.CityDefinition.HasLinkedDLC && !city.IsLinkedDLCOwned)
		{
			SetLockedCityText(city.GetMissingDLCText());
		}
		else
		{
			SetLockedCityText(city.GetLockedCityText());
		}
	}

	private void SetEditButtonVisible(bool isVisible)
	{
		((Component)editButton).gameObject.SetActive(isVisible);
	}

	private void SetGlyphDisplayContainerVisible(bool isVisible)
	{
		((Component)glyphDisplayContainer).gameObject.SetActive(isVisible);
	}

	private void SetLockedCityText(string lockedCityTextContent)
	{
		((Component)lockedCityText).gameObject.SetActive(true);
		((TMP_Text)lockedCityText).text = lockedCityTextContent;
	}
}
