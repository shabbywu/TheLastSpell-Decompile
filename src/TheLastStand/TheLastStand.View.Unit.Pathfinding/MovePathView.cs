using System.Collections.Generic;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.TileMap;
using TheLastStand.View.UnitManagement.UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Pathfinding;

public class MovePathView : MonoBehaviour
{
	[SerializeField]
	private DataColor movePathValidColor;

	[SerializeField]
	private DataColor movePathInvalidColor;

	[SerializeField]
	private GameObject teleportIndicator;

	[SerializeField]
	[Range(0f, 1f)]
	private float movePathAwayAlpha = 0.4f;

	private Dictionary<string, TileBase> tileAssets = new Dictionary<string, TileBase>();

	public MovePath MovePath { get; set; }

	public void Clear(bool removeTeleportationMarker = true)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < MovePath.Path.Count; i++)
		{
			Tile tile = MovePath.Path[i];
			TileMapView.MovePathTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), (TileBase)null);
		}
		TPSingleton<MovePathCounterHUD>.Instance.DisplayMovePathCount(isDisplayed: false);
		teleportIndicator.SetActive(!removeTeleportationMarker);
	}

	public void DisplayMovePath()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (MovePath.Path.Count <= 1 || MovePath.State == MovePath.E_State.Hidden)
		{
			return;
		}
		if (MovePath.State == MovePath.E_State.Teleport)
		{
			Tile tile = MovePath.Path[^1];
			teleportIndicator.transform.position = ((GridLayout)TileMapView.BuildingTilemap).CellToWorld(new Vector3Int(tile.X, tile.Y, 0));
			if (!teleportIndicator.activeSelf)
			{
				teleportIndicator.SetActive(true);
			}
		}
		else if (!InputManager.IsLastControllerJoystick || !TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.IsOnSkillBarSelection() || PlayableUnitManager.SelectedSkill != null)
		{
			for (int i = 0; i < MovePath.Path.Count; i++)
			{
				Tile tile2 = MovePath.Path[i];
				Tile previousTile = ((i == 0) ? null : MovePath.Path[i - 1]);
				Tile nextTile = ((i == MovePath.Path.Count - 1) ? null : MovePath.Path[i + 1]);
				SetTileAsset(tile2, previousTile, nextTile);
			}
			TPSingleton<MovePathCounterHUD>.Instance.DisplayMovePathCount(TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill);
			RefreshMovePathState();
		}
	}

	private void Awake()
	{
		LoadTileAssets();
	}

	private void LoadTileAssets()
	{
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Left", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Left", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Right", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Right", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Down", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Down", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Up", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Up", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Left", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Left", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Right", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Right", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Down", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Down", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Up", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Up", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Horizontal", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Horizontal", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Vertical", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Vertical", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Left", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Left", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Right", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Right", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Left", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Left", false));
		tileAssets.Add("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Right", ResourcePooler.LoadOnce<TileBase>("View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Right", false));
	}

	private void RefreshMovePathState()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		switch (MovePath.State)
		{
		case MovePath.E_State.Valid:
		case MovePath.E_State.Max:
			TileMapView.MovePathTilemap.color = movePathValidColor._Color;
			TPSingleton<MovePathCounterHUD>.Instance.SetMovePathCountValidation(isValid: true);
			break;
		case MovePath.E_State.Invalid:
			TileMapView.MovePathTilemap.color = movePathInvalidColor._Color;
			TPSingleton<MovePathCounterHUD>.Instance.SetMovePathCountValidation(isValid: false);
			break;
		case MovePath.E_State.Away:
		{
			Color color = TileMapView.MovePathTilemap.color;
			color.a = movePathAwayAlpha;
			TileMapView.MovePathTilemap.color = color;
			color = ((Graphic)TPSingleton<MovePathCounterHUD>.Instance.MovePathCountText).color;
			color.a = movePathAwayAlpha;
			((Graphic)TPSingleton<MovePathCounterHUD>.Instance.MovePathCountText).color = color;
			TPSingleton<MovePathCounterHUD>.Instance.ChangeCounterColor(color);
			break;
		}
		}
	}

	private void SetTileAsset(Tile tile, Tile previousTile, Tile nextTile)
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		string text = null;
		text = ((previousTile == null) ? ((nextTile.X > tile.X) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Right" : ((nextTile.X < tile.X) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Left" : ((nextTile.Y <= tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Down" : "View/Tiles/Feedbacks/Movement/MovePath/Move Path Start Up"))) : ((nextTile == null) ? ((previousTile.X < tile.X) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Right" : ((previousTile.X > tile.X) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Left" : ((previousTile.Y >= tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Down" : "View/Tiles/Feedbacks/Movement/MovePath/Move Path Arrow Up"))) : ((previousTile.X != tile.X && nextTile.X != tile.X) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Horizontal" : ((previousTile.Y != tile.Y && nextTile.Y != tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Vertical" : ((previousTile.X < tile.X) ? ((nextTile.Y < tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Left" : "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Left") : ((previousTile.X > tile.X) ? ((nextTile.Y < tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Right" : "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Right") : ((nextTile.X >= tile.X) ? ((previousTile.Y < tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Right" : "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Right") : ((previousTile.Y < tile.Y) ? "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Up Left" : "View/Tiles/Feedbacks/Movement/MovePath/Move Path Angle Down Left"))))))));
		TileMapView.MovePathTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), tileAssets[text]);
	}
}
