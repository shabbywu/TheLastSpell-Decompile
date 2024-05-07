using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rewired;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.Cursor;

public class CursorView : TPSingleton<CursorView>
{
	[SerializeField]
	private Tilemap cursorFeedbacksTilemap;

	[SerializeField]
	private Tilemap cursorShapeFeedbacksTilemap;

	[SerializeField]
	private TileBase cursorTile;

	[SerializeField]
	private TileBase buildingShapeTile;

	[SerializeField]
	private JoystickCursorView joystickCursor;

	private float joystickNoMovementTimer;

	private float joystickFastSpeedTimer;

	private Tween joystickCursorSnapTween;

	private bool previousNullJoystickInput;

	public static Tilemap CursorFeedbacksTilemap => TPSingleton<CursorView>.Instance.cursorFeedbacksTilemap;

	public JoystickCursorView JoystickCursorView => joystickCursor;

	public Vector3 JoystickCursorPosition => ((Component)joystickCursor).transform.position;

	public bool JoystickCursorMoving => joystickNoMovementTimer == 0f;

	public static void ClearTiles()
	{
		ClearTiles(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
	}

	public static void ClearTiles(Tile tile)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if (tile == null)
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit == null)
		{
			if (tile.Building != null)
			{
				tile.Building.BuildingView.Hovered = false;
				if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Building == null)
				{
					TPSingleton<TileMapView>.Instance.EndHoverOutlineAlphaTilemapTweening();
				}
			}
			if (tile.Unit is EnemyUnit enemyUnit)
			{
				enemyUnit.EnemyUnitView.Hovered = false;
			}
			if (tile.Unit is PlayableUnit playableUnit)
			{
				playableUnit.PlayableUnitView.PortraitPanel.OnPointerExit(null);
			}
			CursorFeedbacksTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), (TileBase)null);
		}
		else
		{
			List<Tile> occupiedTiles = tile.GetOccupiedTiles(TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit.BlueprintModuleDefinition);
			for (int i = 0; i < occupiedTiles.Count; i++)
			{
				CursorFeedbacksTilemap.SetTile((Vector3Int)occupiedTiles[i].Position, (TileBase)null);
			}
		}
		TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, tile);
	}

	public static void DisplayBuildingShape(TheLastStand.Model.Building.Building building, bool show)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			TPSingleton<CursorView>.Instance.cursorShapeFeedbacksTilemap.SetTile((Vector3Int)building.BlueprintModule.OccupiedTiles[i].Position, show ? TPSingleton<CursorView>.Instance.buildingShapeTile : null);
		}
	}

	public static Vector3Int GetPositionInTileMap()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return ((GridLayout)CursorFeedbacksTilemap).WorldToCell(((Component)TPSingleton<CursorView>.Instance.joystickCursor).transform.position);
	}

	public void SnapJoystickViewToCursorTile()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!(ApplicationManager.Application.State.GetName() != "Game") && InputManager.IsLastControllerJoystick && joystickCursorSnapTween == null)
		{
			Vector3 val = Vector2.op_Implicit(TileMapView.GetTileCenter(TPSingleton<GameManager>.Instance.Game.Cursor.Tile));
			if (!(Vector3.SqrMagnitude(((Component)joystickCursor).transform.position - val) < 0.01f))
			{
				joystickCursorSnapTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)joystickCursor).transform, val, InputManager.JoystickConfig.Cursor.TileSnapDuration, false), InputManager.JoystickConfig.Cursor.TileSnapEasing);
			}
		}
	}

	public void SnapJoystickCursorToTile(Tile tile)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!(ApplicationManager.Application.State.GetName() != "Game") && InputManager.IsLastControllerJoystick)
		{
			Tween obj = joystickCursorSnapTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			joystickNoMovementTimer = 0f;
			joystickCursor.SetPosition(Vector2.op_Implicit(TileMapView.GetTileCenter(tile)));
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		if (ApplicationManager.Application.State.GetName() == "Game")
		{
			HUDJoystickNavigationManager.HUDNavigationToggled += OnHUDNavigationToggled;
		}
	}

	private static void ApplyValidationColor(Vector3Int tilePosition)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		switch (TPSingleton<GameManager>.Instance.Game.Cursor.CursorState)
		{
		case TheLastStand.Model.Cursor.E_CursorState.Standard:
			CursorFeedbacksTilemap.SetColor(tilePosition, Color.white);
			break;
		case TheLastStand.Model.Cursor.E_CursorState.Valid:
			CursorFeedbacksTilemap.SetColor(tilePosition, Color.green);
			break;
		case TheLastStand.Model.Cursor.E_CursorState.Invalid:
			CursorFeedbacksTilemap.SetColor(tilePosition, Color.red);
			break;
		case TheLastStand.Model.Cursor.E_CursorState.Undefined:
			break;
		}
	}

	private static Vector3 ClampToScreen(Vector3 position)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!InputManager.JoystickConfig.Cursor.ClampToScreen)
		{
			return position;
		}
		Vector3 val = ACameraView.MainCam.ViewportToWorldPoint(Vector2.op_Implicit(Vector2.zero));
		Vector3 val2 = ACameraView.MainCam.ViewportToWorldPoint(Vector2.op_Implicit(Vector2.one));
		position.x = Mathf.Clamp(position.x, val.x, val2.x);
		position.y = Mathf.Clamp(position.y, val.y, val2.y);
		return position;
	}

	private void FocusCameraOnCursorTile()
	{
		if (!(ApplicationManager.Application.State.GetName() != "Game") && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
		{
			ACameraView.MoveTo(((Component)TPSingleton<GameManager>.Instance.Game.Cursor.Tile.TileView).transform);
		}
	}

	private void FocusCameraOnJoystickCursor()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		ACameraView.MoveTo(((Component)joystickCursor).transform.position, 0f, (Ease)0);
	}

	private void OnDestroy()
	{
		InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		HUDJoystickNavigationManager.HUDNavigationToggled -= OnHUDNavigationToggled;
	}

	private void OnHUDNavigationToggled(bool state)
	{
		RecenterJoystickCursor();
		ClearTiles();
		CursorFeedbacksTilemap.ClearAllTiles();
		if (!state)
		{
			joystickCursor.Enable(isOn: true);
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)controllerType > 1)
		{
			if ((int)controllerType == 2)
			{
				if ((Object)(object)TPSingleton<CursorView>.Instance.joystickCursor != (Object)null)
				{
					joystickCursor.Enable(isOn: true);
				}
				return;
			}
			if ((int)controllerType == 20)
			{
			}
		}
		if ((Object)(object)TPSingleton<CursorView>.Instance.joystickCursor != (Object)null)
		{
			joystickCursor.Enable(isOn: false);
		}
	}

	private void Update()
	{
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		if (ApplicationManager.Application.State.GetName() == "Game" && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Recruitment || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingUpgrade || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.NightReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.HowToPlay || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CutscenePlaying))
		{
			return;
		}
		if (ApplicationManager.Application.State.GetName() == "WorldMap" && (TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null || !WorldMapCityManager.CanSelectAnyCity))
		{
			joystickCursor.Show(show: false);
			return;
		}
		UpdateCursor();
		if (!(ApplicationManager.Application.State.GetName() == "Game"))
		{
			return;
		}
		TheLastStand.Model.Cursor cursor = TPSingleton<GameManager>.Instance.Game.Cursor;
		if (cursor.Tile == null)
		{
			return;
		}
		if (InputManager.IsPointerOverWorld || (InputManager.IsPointerOverAllowingCursorUI && !TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn))
		{
			if (InputManager.IsLastControllerJoystick && cursor.TileHasChanged)
			{
				CursorFeedbacksTilemap.ClearAllTiles();
			}
			if (cursor.BuildingToFit != null || !cursor.TileHasChanged)
			{
				return;
			}
			if (cursor.TileHasChanged && cursor.Tile != null)
			{
				if (cursor.Tile.Building != null)
				{
					cursor.Tile.Building.BuildingView.Hovered = true;
					DisplayBuildingShape(cursor.Tile.Building, show: true);
				}
				else if (cursor.Tile.Unit is EnemyUnit enemyUnit)
				{
					enemyUnit.EnemyUnitView.Hovered = true;
				}
			}
			if (cursor.Tile != null && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingExecutingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Wait || (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction && TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.ChooseBuilding)))
			{
				if (cursor.Tile.Unit is PlayableUnit playableUnit)
				{
					playableUnit.PlayableUnitView.PortraitPanel.OnPointerEnter(null);
				}
				else
				{
					TPSingleton<TileObjectSelectionManager>.Instance.UpdateUnitInfoPanel(cursor.Tile.Unit);
				}
			}
			if (ApplicationManager.Application.State.GetName() == "LevelEditor" && TPSingleton<LevelEditorManager>.Instance.RectFillTileSource != null)
			{
				CursorFeedbacksTilemap.ClearAllTiles();
				Tile[] tilesInRect = TileMapController.GetTilesInRect(TileMapController.GetRectFromTileToTile(TPSingleton<LevelEditorManager>.Instance.RectFillTileSource, cursor.Tile));
				foreach (Tile tile in tilesInRect)
				{
					CursorFeedbacksTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), cursorTile);
				}
			}
			else
			{
				if (InputManager.IsLastControllerJoystick && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.PlaceUnit)
				{
					CursorFeedbacksTilemap.ClearAllTiles();
				}
				CursorFeedbacksTilemap.SetTile(cursor.TilePosition, cursorTile);
				ApplyValidationColor(cursor.TilePosition);
			}
		}
		else
		{
			ClearTiles(cursor.Tile);
			CursorFeedbacksTilemap.ClearAllTiles();
			cursor.BuildingToFit = null;
			cursor.Tile = null;
		}
	}

	private void RecenterJoystickCursor()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 cameraCenterTilePosition = TileMapView.GetCameraCenterTilePosition();
		joystickCursor.SetPosition(cameraCenterTilePosition);
	}

	private void UpdateCursor()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TPSingleton<CursorView>.Instance.joystickCursor == (Object)null)
		{
			return;
		}
		ControllerType lastControllerType = InputManager.GetLastControllerType();
		Vector3 position;
		if ((int)lastControllerType > 1)
		{
			if ((int)lastControllerType == 2)
			{
				if (TPSingleton<HUDJoystickNavigationManager>.Exist() && TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn)
				{
					joystickCursor.Enable(isOn: false);
					return;
				}
				if (InputManager.GetButtonDown(81))
				{
					FocusCameraOnCursorTile();
					SnapJoystickViewToCursorTile();
				}
				if (InputManager.JoystickConfig.Cursor.CanHoldCameraFocusDown && InputManager.GetButton(81))
				{
					FocusCameraOnJoystickCursor();
				}
				if (!joystickCursor.Enabled)
				{
					position = ACameraView.MainCam.ScreenToWorldPoint(InputManager.MousePosition);
					position.z = -9f;
					joystickCursor.Enable(isOn: true);
					joystickCursor.SetPosition(ClampToScreen(position));
				}
				Vector3 zero = default(Vector3);
				((Vector3)(ref zero))._002Ector(InputManager.GetAxis(20), InputManager.GetAxis(21));
				float magnitude = ((Vector3)(ref zero)).magnitude;
				if (magnitude < InputManager.JoystickConfig.DefaultDeadZone)
				{
					zero = Vector3.zero;
				}
				bool flag = zero == Vector3.zero;
				if (flag)
				{
					joystickNoMovementTimer += Time.unscaledDeltaTime;
					joystickCursor.Show(joystickNoMovementTimer < InputManager.JoystickConfig.Cursor.HideDelay);
				}
				else
				{
					joystickNoMovementTimer = 0f;
					joystickCursor.Show(show: true);
					if ((Object)(object)CameraView.CameraUIMasksHandler != (Object)null && ((previousNullJoystickInput && InputManager.JoystickConfig.Cursor.UseCameraUIMask) ? CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(((Component)joystickCursor).transform.position) : CameraView.CameraUIMasksHandler.IsPointOffscreen(((Component)joystickCursor).transform.position)))
					{
						RecenterJoystickCursor();
					}
				}
				if (flag)
				{
					if (joystickNoMovementTimer > InputManager.JoystickConfig.Cursor.TileSnapDelay && ApplicationManager.Application.State.GetName() == "Game" && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
					{
						SnapJoystickViewToCursorTile();
					}
				}
				else
				{
					float num;
					if (magnitude < InputManager.JoystickConfig.Cursor.FastSpeedStartInclination)
					{
						num = InputManager.JoystickConfig.Cursor.SlowSpeed;
						joystickFastSpeedTimer = 0f;
					}
					else
					{
						num = Mathf.Lerp(InputManager.JoystickConfig.Cursor.FastSpeedMinMax.x, InputManager.JoystickConfig.Cursor.FastSpeedMinMax.y, joystickFastSpeedTimer);
						if (InputManager.JoystickConfig.Cursor.FastSpeedTransitionDuration == 0f)
						{
							joystickFastSpeedTimer = 1f;
						}
						else
						{
							joystickFastSpeedTimer += Time.unscaledDeltaTime / InputManager.JoystickConfig.Cursor.FastSpeedTransitionDuration;
						}
					}
					if (ACameraView.IsZoomedIn)
					{
						num *= InputManager.JoystickConfig.Cursor.FreeSpeedZoomedInMultiplier;
					}
					Vector3 position2 = ((Component)joystickCursor).transform.position + (InputManager.JoystickConfig.Cursor.NormalizeInput ? ((Vector3)(ref zero)).normalized : zero) * num * Time.unscaledDeltaTime;
					joystickCursor.SetPosition(ClampToScreen(position2));
					Tween obj = joystickCursorSnapTween;
					if (obj != null)
					{
						TweenExtensions.Kill(obj, false);
					}
					joystickCursorSnapTween = null;
				}
				previousNullJoystickInput = flag;
				return;
			}
			if ((int)lastControllerType == 20)
			{
			}
		}
		position = ACameraView.MainCam.ScreenToWorldPoint(InputManager.MousePosition);
		position.z = 0f;
		joystickCursor.Enable(isOn: false);
		joystickCursor.SetPosition(ClampToScreen(position));
	}
}
