using DG.Tweening;
using TPLib;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.WorldMap;

public class WorldMapCameraView : ACameraView
{
	public static void OnStateChange()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			ACameraView.Zoom(zoomIn: false);
			ACameraView.AllowUserPan = true;
			ACameraView.AllowUserZoom = true;
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
		{
			Vector3 position = ((Component)TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityView.TargetPos).transform.position;
			position.x -= (float)(190 / ((WorldMapCameraView)TPSingleton<ACameraView>.Instance).pixelPerfectCamOriginalPpu) - ((Component)TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityView).GetComponent<SpriteRenderer>().size.x / 2f;
			position.x += (float)((WorldMapCameraView)TPSingleton<ACameraView>.Instance).pixelPerfectCamOriginalPpu / 100f / 2f;
			ACameraView.Zoom(zoomIn: true);
			ACameraView.MoveTo(position, 0f, (Ease)0);
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		}
		}
	}
}
