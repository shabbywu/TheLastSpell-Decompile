using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Framework.Automaton;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Manager;

public sealed class SectorManager : Manager<SectorManager>
{
	public static class Constants
	{
		public static class Assets
		{
			public const string SectorContainerPrefabPathFormat = "Prefab/Sectors/{0}/{0}_Sectors";
		}
	}

	[SerializeField]
	[Range(0f, 10f)]
	private int targetFocusCameraWeight = 2;

	[SerializeField]
	[Range(0f, 10f)]
	private int casterFocusCameraWeight = 1;

	private bool initialized;

	public bool TestCameraSectorOnClick;

	public bool WillMoveCameraNextFrame;

	public SectorContainer SectorContainer { get; private set; }

	public List<CameraAreaOfInterest> Sectors => SectorContainer?.Sectors;

	public int SectorsCount => Sectors?.Count ?? 0;

	public int TargetFocusCameraWeight => targetFocusCameraWeight;

	public int CasterFocusCameraWeight => casterFocusCameraWeight;

	public int GetSectorIndexForTile(Tile tile)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (tile == null || Sectors == null)
		{
			((CLogger<SectorManager>)this).LogError((object)("Error while trying to get sector for tile : " + ((tile == null) ? "Tile is" : "Sectors are") + " null."), (CLogLevel)1, true, true);
			return -1;
		}
		Vector3 tileWorldPos = Vector2.op_Implicit(TileMapView.GetTileCenter(tile));
		IEnumerable<CameraAreaOfInterest> source = from x in Sectors
			where x.AreaCollider.OverlapPoint(Vector2.op_Implicit(tileWorldPos))
			orderby x.AreaWeight descending
			select x;
		CameraAreaOfInterest cameraAreaOfInterest = ((source.Count() == 0) ? Sectors.OrderBy(delegate(CameraAreaOfInterest x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = ((Component)x).transform.position - tileWorldPos;
			return ((Vector3)(ref val)).magnitude - x.AreaWeight;
		}).First() : source.First());
		((CLogger<SectorManager>)this).Log((object)string.Format("Asked for sector index for tile {0}, returning {1}{2}", tile.Position, source.Any() ? string.Empty : "default: ", ((Object)cameraAreaOfInterest).name), (CLogLevel)1, false, false);
		return Sectors.IndexOf(cameraAreaOfInterest);
	}

	public void Init()
	{
		if (!initialized)
		{
			string text = ((((StateMachine)ApplicationManager.Application).State.GetName() == "LevelEditor") ? LevelEditorManager.CityToLoadId : TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.SectorContainerPrefabId);
			string text2 = string.Format("Prefab/Sectors/{0}/{0}_Sectors", text);
			SectorContainer sectorContainer = ResourcePooler.LoadOnce<SectorContainer>(text2, false);
			if ((Object)(object)sectorContainer != (Object)null)
			{
				((CLogger<SectorManager>)this).Log((object)("Prefab has been found for city Id " + text + " at " + text2 + "."), (CLogLevel)2, false, false);
				SectorContainer = Object.Instantiate<SectorContainer>(sectorContainer, ((Component)this).transform);
			}
			else
			{
				((CLogger<SectorManager>)this).LogError((object)("No Sector prefab has been found for city Id " + text + "."), (CLogLevel)2, true, true);
			}
			initialized = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Update()
	{
		DebugUpdate();
	}

	[DevConsoleCommand("TestCameraSectorOnClick")]
	[ContextMenu("TestCameraSectorOnClick")]
	public static void DebugTestCameraSectorOnClick(bool state = true)
	{
		TPSingleton<SectorManager>.Instance.TestCameraSectorOnClick = state;
	}

	private void DebugUpdate()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.GetButton(24) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
		{
			if (!TPSingleton<SectorManager>.Instance.TestCameraSectorOnClick)
			{
				return;
			}
			if (!WillMoveCameraNextFrame)
			{
				WillMoveCameraNextFrame = true;
				return;
			}
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			if (tile != null)
			{
				int sectorIndexForTile = TPSingleton<SectorManager>.Instance.GetSectorIndexForTile(tile);
				if (TPSingleton<SectorManager>.Instance.SectorsCount > sectorIndexForTile)
				{
					CameraAreaOfInterest cameraAreaOfInterest = TPSingleton<SectorManager>.Instance.Sectors[sectorIndexForTile];
					((CLogger<SectorManager>)this).Log((object)$"TestCameraSectorOnClick was triggered for tile {tile.Position}, moving to sector {((Object)cameraAreaOfInterest).name} at position {((Component)cameraAreaOfInterest).transform.position}.", (CLogLevel)1, true, false);
					ACameraView.MoveTo(((Component)cameraAreaOfInterest).transform.position, CameraView.AnimationMoveSpeed, (Ease)0);
				}
			}
			WillMoveCameraNextFrame = false;
		}
		else if (WillMoveCameraNextFrame)
		{
			WillMoveCameraNextFrame = false;
		}
	}
}
