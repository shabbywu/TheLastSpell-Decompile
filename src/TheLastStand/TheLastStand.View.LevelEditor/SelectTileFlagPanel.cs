using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class SelectTileFlagPanel : MonoBehaviour
{
	[SerializeField]
	private Toggle tileFlagToggle;

	[SerializeField]
	private TextMeshProUGUI selectedTileFlagTilesNbText;

	private Dictionary<TileFlagDefinition.E_TileFlagTag, LevelEditorButton> buttonsByFlag = new Dictionary<TileFlagDefinition.E_TileFlagTag, LevelEditorButton>();

	private RectTransform rectTransform;

	public void ToggleOffFlag(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		buttonsByFlag[tileFlag].ToggleOff();
	}

	public void RefreshText()
	{
		RefreshTileFlagTilesNbText();
	}

	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnTileFlagButtonClick(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		TileMapView.ToggleTilesFlag(tileFlag, true, clearPreviousState: false);
		LevelEditorManager.SelectTileFlag(tileFlag);
		RefreshText();
	}

	private void OnTileFlagToggleValueChanged(TileFlagDefinition.E_TileFlagTag tileFlag, bool state)
	{
		TileMapView.ToggleTilesFlag(tileFlag, state, clearPreviousState: false);
	}

	private void Awake()
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		TPHelpers.DestroyChildren(((Component)this).transform);
		List<TileFlagDefinition> list = new List<TileFlagDefinition>(TileMapManager.TileFlagDefinitions);
		list.Sort((TileFlagDefinition a, TileFlagDefinition b) => a.TileFlagTag.ToString().CompareTo(b.TileFlagTag.ToString()));
		foreach (TileFlagDefinition tileFlagDefinition in list)
		{
			LevelEditorButton levelEditorButton = Object.Instantiate<LevelEditorButton>(LevelEditorManager.LevelEditorButtonTogglePrefab, ((Component)this).transform);
			buttonsByFlag.Add(tileFlagDefinition.TileFlagTag, levelEditorButton);
			levelEditorButton.Init(tileFlagDefinition.TileFlagTag.ToString(), (UnityAction)delegate
			{
				OnTileFlagButtonClick(tileFlagDefinition.TileFlagTag);
			}, delegate(bool value)
			{
				OnTileFlagToggleValueChanged(tileFlagDefinition.TileFlagTag, value);
			});
			BetterButton button = levelEditorButton.Button;
			ColorBlock colors = default(ColorBlock);
			ColorBlock colors2 = ((Selectable)levelEditorButton.Button).colors;
			((ColorBlock)(ref colors)).normalColor = ((ColorBlock)(ref colors2)).normalColor;
			((ColorBlock)(ref colors)).highlightedColor = tileFlagDefinition.DebugColor;
			colors2 = ((Selectable)levelEditorButton.Button).colors;
			((ColorBlock)(ref colors)).pressedColor = ((ColorBlock)(ref colors2)).pressedColor;
			colors2 = ((Selectable)levelEditorButton.Button).colors;
			((ColorBlock)(ref colors)).disabledColor = ((ColorBlock)(ref colors2)).disabledColor;
			colors2 = ((Selectable)levelEditorButton.Button).colors;
			((ColorBlock)(ref colors)).colorMultiplier = ((ColorBlock)(ref colors2)).colorMultiplier;
			colors2 = ((Selectable)levelEditorButton.Button).colors;
			((ColorBlock)(ref colors)).fadeDuration = ((ColorBlock)(ref colors2)).fadeDuration;
			((Selectable)button).colors = colors;
		}
		Object.Instantiate<LevelEditorButton>(LevelEditorManager.LevelEditorButtonPrefab, ((Component)this).transform).Init("BACK (Esc)", new UnityAction(OnBackButtonClick));
	}

	private void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)selectedTileFlagTilesNbText != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			((TMP_Text)selectedTileFlagTilesNbText).rectTransform.anchoredPosition = new Vector2(((TMP_Text)selectedTileFlagTilesNbText).rectTransform.anchoredPosition.x, rectTransform.sizeDelta.y);
		}
	}

	private void OnEnable()
	{
		((Component)tileFlagToggle).gameObject.SetActive(false);
		TileMapView.ToggleTilesFlagAll(false);
		((Component)selectedTileFlagTilesNbText).gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		((Component)tileFlagToggle).gameObject.SetActive(true);
		tileFlagToggle.isOn = false;
		TileMapView.ToggleTilesFlagAll(false);
		((Component)selectedTileFlagTilesNbText).gameObject.SetActive(false);
	}

	private void RefreshTileFlagTilesNbText()
	{
		if ((Object)(object)selectedTileFlagTilesNbText != (Object)null && TPSingleton<TileMapManager>.Exist() && TPSingleton<LevelEditorManager>.Exist())
		{
			((TMP_Text)selectedTileFlagTilesNbText).text = TileMapManager.DebugGetFormattedTileFlagTilesNb(TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag);
		}
	}
}
