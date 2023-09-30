using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.WorldMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphsHeader : MonoBehaviour
{
	[SerializeField]
	private Image cityPointPrefab;

	[SerializeField]
	private Sprite cityPointOn;

	[SerializeField]
	private Sprite cityPointOff;

	[SerializeField]
	private Sprite headerBackgroundDefault;

	[SerializeField]
	private Sprite headerBackgroundCustomMode;

	[SerializeField]
	private Image headerBackground;

	[SerializeField]
	private TextMeshProUGUI cityName;

	[SerializeField]
	private TextMeshProUGUI cityPointsText;

	[SerializeField]
	private RectTransform cityPointsContainer;

	[SerializeField]
	private Image customModeIcon;

	[SerializeField]
	private TextMeshProUGUI customModePoints;

	private List<Image> cityPointImages = new List<Image>();

	private bool updateCityName;

	public void InitCityPointImages()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		while (cityPointImages.Count > selectedCity.CityDefinition.MaxGlyphPoints)
		{
			Object.Destroy((Object)(object)((Component)cityPointImages[^1]).gameObject);
			cityPointImages.RemoveAt(cityPointImages.Count - 1);
		}
		while (cityPointImages.Count < selectedCity.CityDefinition.MaxGlyphPoints)
		{
			cityPointImages.Add(Object.Instantiate<Image>(cityPointPrefab, (Transform)(object)cityPointsContainer));
		}
	}

	public void RefreshCityName()
	{
		if (updateCityName)
		{
			((TMP_Text)cityName).text = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Name;
		}
	}

	public void RefreshCityPoints()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		for (int i = 0; i < selectedCity.CurrentGlyphPoints && i < selectedCity.CityDefinition.MaxGlyphPoints; i++)
		{
			cityPointImages[i].sprite = cityPointOn;
		}
		for (int j = selectedCity.CurrentGlyphPoints; j < selectedCity.CityDefinition.MaxGlyphPoints; j++)
		{
			cityPointImages[j].sprite = cityPointOff;
		}
		((TMP_Text)cityPointsText).text = $"{selectedCity.CurrentGlyphPoints}/{selectedCity.CityDefinition.MaxGlyphPoints}";
		headerBackground.sprite = (selectedCity.GlyphsConfig.CustomModeEnabled ? headerBackgroundCustomMode : headerBackgroundDefault);
		((Behaviour)customModeIcon).enabled = selectedCity.GlyphsConfig.CustomModeEnabled;
		((Behaviour)customModePoints).enabled = selectedCity.GlyphsConfig.CustomModeEnabled;
		((TMP_Text)customModePoints).text = $"+{selectedCity.GetCustomModeBonusPoints()}";
	}

	private void Awake()
	{
		updateCityName = (Object)(object)cityName != (Object)null;
	}
}
