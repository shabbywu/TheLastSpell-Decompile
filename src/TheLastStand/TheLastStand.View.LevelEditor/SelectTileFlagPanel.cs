using System.Collections.Generic;
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

	private Dictionary<TileFlagDefinition.E_TileFlagTag, LevelEditorButton> buttonsByFlag = new Dictionary<TileFlagDefinition.E_TileFlagTag, LevelEditorButton>();

	public void ToggleOffFlag(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		buttonsByFlag[tileFlag].ToggleOff();
	}

	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnTileFlagButtonClick(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		TileMapView.ToggleTilesFlag(tileFlag, true, clearPreviousState: false);
		LevelEditorManager.SelectTileFlag(tileFlag);
	}

	private void OnTileFlagToggleValueChanged(TileFlagDefinition.E_TileFlagTag tileFlag, bool state)
	{
		TileMapView.ToggleTilesFlag(tileFlag, state, clearPreviousState: false);
	}

	private void Awake()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
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

	private void OnEnable()
	{
		((Component)tileFlagToggle).gameObject.SetActive(false);
		TileMapView.ToggleTilesFlagAll(false);
	}

	private void OnDisable()
	{
		((Component)tileFlagToggle).gameObject.SetActive(true);
		tileFlagToggle.isOn = false;
		TileMapView.ToggleTilesFlagAll(false);
	}
}
