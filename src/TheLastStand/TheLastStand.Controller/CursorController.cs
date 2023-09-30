using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Cursor;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Controller;

public class CursorController
{
	public Cursor Cursor { get; }

	public CursorController()
	{
		Cursor = new Cursor(this);
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		switch (state)
		{
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.PlaceUnit:
		case Game.E_State.Shopping:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.HowToPlay:
			CursorView.ClearTiles();
			break;
		}
	}

	public void SetTile()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		Cursor.PreviousTile = Cursor.Tile;
		Cursor.PreviousTilePosition = Cursor.TilePosition;
		Cursor.TilePosition = CursorView.GetPositionInTileMap();
		if (!(Cursor.TilePosition != Cursor.PreviousTilePosition) && Cursor.PreviousTile != null)
		{
			return;
		}
		Cursor cursor = Cursor;
		TheLastStand.Model.TileMap.TileMap tileMap = TPSingleton<TileMapManager>.Instance.TileMap;
		Vector3Int tilePosition = Cursor.TilePosition;
		int x = ((Vector3Int)(ref tilePosition)).x;
		tilePosition = Cursor.TilePosition;
		cursor.Tile = tileMap.GetTile(x, ((Vector3Int)(ref tilePosition)).y);
		CursorView.ClearTiles(Cursor.PreviousTile);
		TileObjectSelectionManager.UpdateCursorOrientationFromSelection(Cursor.Tile);
		if (!TPSingleton<SkillManager>.Exist() || SkillManager.SelectedSkill == null || (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill))
		{
			return;
		}
		if (TileObjectSelectionManager.CursorOrientationChanged || Cursor.PreviousTile == null)
		{
			if (SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDialsTilesColorsFrom(SkillManager.SelectedSkill.SkillAction.SkillActionExecution.SkillSourceTile, SkillManager.SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDisplayRangeTilesColors(SkillManager.SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
			}
			if (TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit?.UnitController.LookAtDirection(TileObjectSelectionManager.GetDirectionFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
			}
			TileObjectSelectionManager.CursorOrientationChanged = false;
		}
		if ((SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
		{
			TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, Cursor.Tile, "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On");
		}
	}
}
