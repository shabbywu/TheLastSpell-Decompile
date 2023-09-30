using System.Collections;
using System.Reflection;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib.Yield.CustomYieldInstructions;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(PixelPerfectCamera))]
public class OverrideProCam2DOrthoSize : MonoBehaviour
{
	private object pixelPerfectCameraInternal;

	private FieldInfo orthoInfo;

	private float orthosize = -1f;

	private float lastOrthoSize = -1f;

	private bool started;

	private WaitForFrames waitForFrames = new WaitForFrames(2);

	private void LateUpdate()
	{
		orthosize = (float)orthoInfo.GetValue(pixelPerfectCameraInternal);
		if (lastOrthoSize != orthosize && started)
		{
			lastOrthoSize = orthosize;
			((MonoBehaviour)this).StartCoroutine(ResetProCam2D());
		}
	}

	private void Start()
	{
		pixelPerfectCameraInternal = typeof(PixelPerfectCamera).GetField("m_Internal", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(((Component)this).GetComponent<PixelPerfectCamera>());
		orthoInfo = pixelPerfectCameraInternal.GetType().GetField("orthoSize", BindingFlags.Instance | BindingFlags.NonPublic);
		started = true;
	}

	[ContextMenu("ResetProCam2D")]
	private IEnumerator ResetProCam2D()
	{
		if (!((Object)(object)ProCamera2D.Instance == (Object)null))
		{
			yield return waitForFrames;
			ProCamera2D.Instance.CalculateScreenSize();
			ProCamera2D.Instance.ResetStartSize(default(Vector2));
		}
	}
}
