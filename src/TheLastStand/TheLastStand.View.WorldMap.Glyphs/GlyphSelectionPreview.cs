using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.WorldMap;
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
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		glyphsHeader.InitCityPointImages();
		glyphsHeader.RefreshCityPoints();
		List<GlyphDefinition> selectedGlyphs = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs;
		while (glyphDisplays.Count > selectedGlyphs.Count)
		{
			Object.Destroy((Object)(object)((Component)glyphDisplays[0]).gameObject);
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
				SelectableExtensions.SetSelectOnUp((Selectable)(object)glyphDisplay.JoystickSelectable, (Selectable)(object)EditButton);
			}
			navigation = ((Selectable)glyphDisplay.JoystickSelectable).navigation;
			if ((Object)(object)((Navigation)(ref navigation)).selectOnDown == (Object)null)
			{
				SelectableExtensions.SetSelectOnDown((Selectable)(object)glyphDisplay.JoystickSelectable, (Selectable)(object)((TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines.Count > 0) ? TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines[0].JoystickSelectable : null));
			}
		}
	}
}
