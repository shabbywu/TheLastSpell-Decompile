using System;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.GroundManagement;

public class GroundManagementPanel : SerializedMonoBehaviour
{
	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GameObject fogPanel;

	[SerializeField]
	private TextMeshProUGUI fogPanelDescription;

	[SerializeField]
	private TextMeshProUGUI fogPanelTitle;

	[SerializeField]
	private Image fogPanelTitleIcon;

	[SerializeField]
	private Image fogPanelTitleBG;

	[SerializeField]
	private Sprite fogTitleIcon;

	[SerializeField]
	private Sprite lightFogTitleIcon;

	[SerializeField]
	private Sprite fogTitleBG;

	[SerializeField]
	private Sprite lightFogTitleBG;

	[SerializeField]
	private DataColor fogTitleColor;

	[SerializeField]
	private DataColor lightFogTitleColor;

	[SerializeField]
	private TextMeshProUGUI groundDescriptionText;

	[SerializeField]
	private TextMeshProUGUI groundIdText;

	[SerializeField]
	private Image groundPortraitImage;

	private Tile tile;

	public void Close()
	{
		tile = null;
		((Behaviour)canvas).enabled = false;
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).UnregisterChilds();
		}
	}

	public void Open()
	{
		((Behaviour)canvas).enabled = UIManager.DebugToggleUI != false;
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RegisterChilds();
		}
		Refresh();
	}

	public void Refresh()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if (!TileObjectSelectionManager.HasTileSelected)
		{
			Close();
		}
		else if (TileObjectSelectionManager.SelectedTile != tile)
		{
			tile = TileObjectSelectionManager.SelectedTile;
			groundPortraitImage.sprite = tile.TileView.GetPortraitSprite();
			RefreshLocalizedTexts();
			bool flag = TPSingleton<FogManager>.Exist() && TPSingleton<FogManager>.Instance.Fog.LightFogTiles.ContainsKey(tile) && TPSingleton<FogManager>.Instance.Fog.LightFogTiles[tile].Mode != Fog.LightFogTileInfo.E_LightFogMode.Impeded;
			if (tile.HasFog || flag)
			{
				((Graphic)fogPanelTitle).color = (tile.HasFog ? fogTitleColor._Color : lightFogTitleColor._Color);
				fogPanelTitleBG.sprite = (tile.HasFog ? fogTitleBG : lightFogTitleBG);
				fogPanelTitleIcon.sprite = (tile.HasFog ? fogTitleIcon : lightFogTitleIcon);
				fogPanel.SetActive(true);
			}
			else if (fogPanel.activeSelf)
			{
				fogPanel.SetActive(false);
			}
		}
	}

	private void RefreshLocalizedTexts()
	{
		if (tile != null)
		{
			string text = "_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
			string text2 = "GroundName_" + tile.GroundDefinition.Id;
			string text3 = "GroundDescription_" + tile.GroundDefinition.Id;
			((TMP_Text)groundIdText).text = Localizer.Get(Localizer.ExistsInCurrentLanguage(text2 + text) ? (text2 + text) : text2);
			((TMP_Text)groundDescriptionText).text = Localizer.Get(Localizer.ExistsInCurrentLanguage(text3 + text) ? (text3 + text) : text3);
			bool flag = TPSingleton<FogManager>.Exist() && TPSingleton<FogManager>.Instance.Fog.LightFogTiles.ContainsKey(tile) && TPSingleton<FogManager>.Instance.Fog.LightFogTiles[tile].Mode != Fog.LightFogTileInfo.E_LightFogMode.Impeded;
			if (tile.HasFog || flag)
			{
				string text4 = (tile.HasFog ? "Fog" : "LightFog");
				((TMP_Text)fogPanelTitle).text = Localizer.Get("FogName_" + text4);
				((TMP_Text)fogPanelDescription).text = Localizer.Get("FogDescription_" + text4);
			}
		}
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		Close();
	}

	private void OnLocalize()
	{
		if (((Behaviour)canvas).enabled)
		{
			RefreshLocalizedTexts();
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}
}
