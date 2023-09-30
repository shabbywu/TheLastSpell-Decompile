using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Camera;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class FocusTileCutsceneController : CutsceneController
{
	public FocusTileCutsceneDefinition FocusTileCutsceneDefinition => base.CutsceneDefinition as FocusTileCutsceneDefinition;

	public FocusTileCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		Tile tile = TileMapManager.GetTile(FocusTileCutsceneDefinition.PosX, FocusTileCutsceneDefinition.PosY);
		Transform val = ((tile != null) ? ((Component)tile.TileView).transform : null);
		if ((Object)(object)val != (Object)null)
		{
			ACameraView.MoveTo(val);
		}
		else
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogError((object)string.Format("{0} : Tile {1}:{2} does not exist!", "FocusTileCutsceneController", FocusTileCutsceneDefinition.PosX, FocusTileCutsceneDefinition.PosY), (CLogLevel)2, true, true);
		}
		yield break;
	}
}
