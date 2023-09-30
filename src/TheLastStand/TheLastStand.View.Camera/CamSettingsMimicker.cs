using TPLib;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(Camera))]
public class CamSettingsMimicker : MonoBehaviour
{
	[SerializeField]
	private Camera target;

	[SerializeField]
	private bool mimickOrthoSize;

	[SerializeField]
	private bool mimickPixelPerfect;

	private Camera cam;

	private PixelPerfectCamera pixelPerfectCam;

	private PixelPerfectCamera targetPixelPerfectCam;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if ((Object)(object)target == (Object)null)
		{
			if (TPSingleton<GameView>.Exist())
			{
				target = ACameraView.MainCam;
			}
			else
			{
				target = Camera.main;
			}
		}
		if ((Object)(object)cam == (Object)null)
		{
			cam = ((Component)this).GetComponent<Camera>();
		}
	}

	private void InitPixelPerfect()
	{
		if ((Object)(object)target == (Object)null)
		{
			Init();
		}
		if ((Object)(object)target == (Object)null)
		{
			TPDebug.LogError((object)"Need to have a target! Disabling self...", (Object)(object)this);
			((Behaviour)this).enabled = false;
			return;
		}
		pixelPerfectCam = ((Component)cam).GetComponent<PixelPerfectCamera>();
		targetPixelPerfectCam = ((Component)target).GetComponent<PixelPerfectCamera>();
		if ((Object)(object)pixelPerfectCam == (Object)null || (Object)(object)targetPixelPerfectCam == (Object)null)
		{
			TPDebug.LogError((object)"Either this or the target doesn't have a PixelPerfectCam! Disabling self...", (Object)(object)this);
			((Behaviour)this).enabled = false;
		}
	}

	private void Update()
	{
		if ((Object)(object)target == (Object)null)
		{
			Init();
			if ((Object)(object)target == (Object)null)
			{
				TPDebug.LogError((object)"Need to have a target! Disabling self...", (Object)(object)this);
				((Behaviour)this).enabled = false;
				return;
			}
		}
		if (mimickPixelPerfect)
		{
			if ((Object)(object)pixelPerfectCam == (Object)null || (Object)(object)targetPixelPerfectCam == (Object)null)
			{
				InitPixelPerfect();
				if ((Object)(object)pixelPerfectCam == (Object)null || (Object)(object)targetPixelPerfectCam == (Object)null)
				{
					return;
				}
			}
			if (pixelPerfectCam.assetsPPU != targetPixelPerfectCam.assetsPPU)
			{
				pixelPerfectCam.assetsPPU = targetPixelPerfectCam.assetsPPU;
			}
			if (pixelPerfectCam.refResolutionX != targetPixelPerfectCam.refResolutionX || pixelPerfectCam.refResolutionY != targetPixelPerfectCam.refResolutionY)
			{
				pixelPerfectCam.refResolutionX = targetPixelPerfectCam.refResolutionX;
				pixelPerfectCam.refResolutionY = targetPixelPerfectCam.refResolutionY;
			}
		}
		else if (mimickOrthoSize)
		{
			cam.orthographicSize = target.orthographicSize;
		}
	}
}
