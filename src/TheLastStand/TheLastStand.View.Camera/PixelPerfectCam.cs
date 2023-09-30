using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(Camera))]
public class PixelPerfectCam : MonoBehaviour
{
	[Serializable]
	public class PixelPerfectCamDebug
	{
	}

	[SerializeField]
	[FormerlySerializedAs("_refResolutionHeight")]
	protected int refResolutionHeight = 720;

	[SerializeField]
	[FormerlySerializedAs("_refPixelPerUnit")]
	protected int refPixelPerUnit = 32;

	[SerializeField]
	[FormerlySerializedAs("_refPixelPerUnitScale")]
	protected int refPixelPerUnitScale = 1;

	[Space(10f)]
	public PixelPerfectCamDebug DbgDataPixelPerfectCam;

	protected void Awake()
	{
		SetOrthoSize();
	}

	[ContextMenu("Set Ortho Size")]
	public void SetOrthoSize()
	{
		Camera component = ((Component)this).GetComponent<Camera>();
		if ((Object)(object)component != (Object)null)
		{
			component.orthographicSize = (float)refResolutionHeight / (float)(refPixelPerUnitScale * refPixelPerUnit) * 0.5f;
		}
	}
}
