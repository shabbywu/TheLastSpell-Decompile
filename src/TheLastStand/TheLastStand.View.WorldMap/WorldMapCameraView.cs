using DG.Tweening;
using TPLib;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Camera;
using TheLastStand.View.WorldMap.ItemRestriction;
using UnityEngine;

namespace TheLastStand.View.WorldMap;

public class WorldMapCameraView : ACameraView
{
	public static void OnStateChange()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			ACameraView.Zoom(zoomIn: false);
			CheckIfCanMoveCameraInExplorationState();
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
		{
			Vector3 position = ((Component)TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityView.TargetPos).transform.position;
			position.x -= (float)(190 / ((WorldMapCameraView)TPSingleton<ACameraView>.Instance).pixelPerfectCamOriginalPpu) - 4.392857f;
			position.x += (float)((WorldMapCameraView)TPSingleton<ACameraView>.Instance).pixelPerfectCamOriginalPpu / 100f / 2f;
			ACameraView.Zoom(zoomIn: true);
			ACameraView.MoveTo(position, 0f, (Ease)0);
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		}
		}
	}

	public static void CheckIfCanMoveCameraInExplorationState()
	{
		if (TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.EXPLORATION || TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.DEFAULT)
		{
			if (TPSingleton<WeaponRestrictionsPanel>.Instance.Displayed)
			{
				ACameraView.AllowUserPan = false;
				ACameraView.AllowUserZoom = false;
			}
			else
			{
				ACameraView.AllowUserPan = true;
				ACameraView.AllowUserZoom = true;
			}
		}
	}
}
