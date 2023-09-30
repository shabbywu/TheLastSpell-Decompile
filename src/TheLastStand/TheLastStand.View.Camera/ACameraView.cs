using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.View.Camera.ProCam2D;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

public abstract class ACameraView : Manager<ACameraView>
{
	public delegate void DelZoom(bool zoomed);

	public static DelZoom OnZoomHasChanged;

	private static Tweener moveTweener;

	[SerializeField]
	private Vector3 startPos = new Vector3(0f, 39f, -10f);

	[SerializeField]
	private bool allowUserZoom = true;

	[SerializeField]
	private bool startZoomedIn;

	[SerializeField]
	private float scrollWheelSensitivity = 2f;

	[SerializeField]
	private float scrollWheelTimeframe = 1f;

	[SerializeField]
	private Ease zoomEasing = (Ease)13;

	[SerializeField]
	private float zoomDuration = 0.3f;

	[SerializeField]
	protected Camera mainCam;

	[SerializeField]
	private PixelPerfectCamera pixelPerfectCam;

	private ProCamera2DShake camShake;

	private bool isZoomedIn;

	private bool lastZoom = true;

	private float lastZoomInputTime;

	protected int pixelPerfectCamOriginalPpu;

	private Coroutine zoomCoroutine;

	private Tweener zoomTweener;

	private float zoomInputCumulativeValue;

	[SerializeField]
	private bool debugStopShakesBeforeStartingNewOne;

	public ProCamera2DExtensionPan CamPanner { get; private set; }

	public static bool AllowUserPan
	{
		get
		{
			if ((Object)(object)TPSingleton<ACameraView>.Instance.CamPanner != (Object)null && ((Behaviour)TPSingleton<ACameraView>.Instance.CamPanner).enabled)
			{
				return TPSingleton<ACameraView>.Instance.CamPanner.AllowPan;
			}
			return false;
		}
		set
		{
			if ((Object)(object)TPSingleton<ACameraView>.Instance.CamPanner != (Object)null)
			{
				TPSingleton<ACameraView>.Instance.CamPanner.AllowPan = value;
			}
		}
	}

	public static bool AllowUserZoom
	{
		get
		{
			return TPSingleton<ACameraView>.Instance.allowUserZoom;
		}
		set
		{
			TPSingleton<ACameraView>.Instance.allowUserZoom = value;
		}
	}

	public static bool IsZoomedIn => TPSingleton<ACameraView>.Instance.isZoomedIn;

	public static bool IsZooming => TPSingleton<ACameraView>.Instance.zoomCoroutine != null;

	public static Camera MainCam => TPSingleton<ACameraView>.Instance.mainCam;

	public static void MoveTo(Vector3 targetPosition, float time = 0f, Ease ease = 0)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Vector3 previousTargetPosition = ProCamera2D.Instance.CameraTargets[0].TargetTransform.position;
		if (moveTweener != null)
		{
			TweenExtensions.Kill((Tween)(object)moveTweener, false);
		}
		moveTweener = (Tweener)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(DOTween.To((DOGetter<Vector3>)(() => previousTargetPosition), (DOSetter<Vector3>)delegate(Vector3 x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			previousTargetPosition = x;
			ProCamera2D.Instance.CameraTargets[0].TargetTransform.position = x;
		}, targetPosition, time), ease);
	}

	public static void MoveTo(Transform targetTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		MoveTo(targetTransform.position, 0f, (Ease)0);
	}

	public static void Shake(string shakeId = "CamShake - Attack", float delay = 0f)
	{
		if (!TPSingleton<SettingsManager>.Exist() || TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue != 0f)
		{
			((MonoBehaviour)TPSingleton<ACameraView>.Instance).StartCoroutine(TPSingleton<ACameraView>.Instance.ShakeCoroutine(shakeId, delay));
		}
	}

	public static void Shake(float duration, Vector2 strength, int vibrato, float randomness, float initialAngle, Vector3 rotation, float smoothness, float delay = 0f)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!TPSingleton<SettingsManager>.Exist() || TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue != 0f)
		{
			((MonoBehaviour)TPSingleton<ACameraView>.Instance).StartCoroutine(TPSingleton<ACameraView>.Instance.ShakeCoroutine(duration, strength, vibrato, randomness, initialAngle, rotation, smoothness, delay));
		}
	}

	public static void StopShaking()
	{
		if ((Object)(object)TPSingleton<ACameraView>.Instance.camShake == (Object)null)
		{
			TPSingleton<ACameraView>.Instance.InitShake();
		}
		TPSingleton<ACameraView>.Instance.camShake.StopShaking();
	}

	public static Coroutine Zoom(bool zoomIn, bool instant = false)
	{
		if (TPSingleton<ACameraView>.Instance.zoomCoroutine != null)
		{
			return TPSingleton<ACameraView>.Instance.zoomCoroutine;
		}
		if (zoomIn == IsZoomedIn)
		{
			return null;
		}
		OnZoomHasChanged?.Invoke(zoomIn);
		TPSingleton<ACameraView>.Instance.zoomCoroutine = ((MonoBehaviour)TPSingleton<ACameraView>.Instance).StartCoroutine(TPSingleton<ACameraView>.Instance.ZoomCoroutine(zoomIn, instant));
		return TPSingleton<ACameraView>.Instance.zoomCoroutine;
	}

	public virtual void Init()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		((Component)MainCam).transform.position = startPos;
		if (ProCamera2D.Exists)
		{
			ProCamera2D.Instance.MoveCameraInstantlyToPosition(Vector2.op_Implicit(startPos));
		}
		InitShake();
	}

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChanged;
		pixelPerfectCamOriginalPpu = pixelPerfectCam.assetsPPU;
		zoomTweener = null;
		lastZoomInputTime = -1f;
		zoomInputCumulativeValue = -1f;
		isZoomedIn = !startZoomedIn;
		ProCamera2D instance = ProCamera2D.Instance;
		CamPanner = ((instance != null) ? ((Component)instance).GetComponent<ProCamera2DExtensionPan>() : null);
		Init();
	}

	protected virtual int ComputeTargetPixelsPerUnit(bool zoomedIn)
	{
		int num = pixelPerfectCamOriginalPpu;
		if (zoomedIn)
		{
			num *= 2;
		}
		return num;
	}

	private void OnResolutionChanged(Resolution resolution)
	{
		pixelPerfectCam.assetsPPU = ComputeTargetPixelsPerUnit(isZoomedIn);
	}

	private void InitShake()
	{
		if (ProCamera2D.Exists)
		{
			camShake = ((Component)ProCamera2D.Instance).GetComponent<ProCamera2DShake>();
			ProCamera2DShake obj = camShake;
			obj.OnShakeCompleted = (Action)Delegate.Combine(obj.OnShakeCompleted, (Action)delegate
			{
				((CLogger<ACameraView>)this).Log((object)$"[{Time.time}] Shake completed", (Object)(object)this, (CLogLevel)0, false, false);
			});
		}
	}

	protected void Start()
	{
		if ((Object)(object)CamPanner != (Object)null)
		{
			CamPanner.UsePanByMoveToEdges = TPSingleton<SettingsManager>.Instance.Settings.EdgePan;
			CamPanner.IgnoreUIOnEdges = TPSingleton<SettingsManager>.Instance.Settings.EdgePanOverUI;
		}
		Zoom(startZoomedIn, instant: true);
	}

	protected virtual void Update()
	{
		if (!allowUserZoom)
		{
			return;
		}
		float axis = InputManager.GetAxis(14);
		bool buttonDown = InputManager.GetButtonDown(14);
		if (Mathf.Abs(axis) > 0f)
		{
			if (zoomInputCumulativeValue != 0f && Mathf.Sign(axis) != Mathf.Sign(zoomInputCumulativeValue))
			{
				ResetZoomTimeFrame();
				((CLogger<ACameraView>)this).Log((object)"Zoom Reset: input direction changed", (Object)(object)this, (CLogLevel)0, false, false);
			}
			if (lastZoomInputTime < 0f)
			{
				lastZoomInputTime = Time.time;
			}
			zoomInputCumulativeValue += axis;
			float num = Mathf.Abs(zoomInputCumulativeValue);
			if (num > 0f)
			{
				((CLogger<ACameraView>)this).Log((object)$"Cumulative zoom value: {zoomInputCumulativeValue}", (Object)(object)this, (CLogLevel)0, false, false);
			}
			if (num >= scrollWheelSensitivity)
			{
				lastZoom = axis > 0f;
				Zoom(axis > 0f);
				ResetZoomTimeFrame();
				((CLogger<ACameraView>)this).Log((object)"Zoom Triggered", (Object)(object)this, (CLogLevel)0, false, false);
			}
		}
		else if (buttonDown)
		{
			if (lastZoom)
			{
				lastZoom = false;
				Zoom(zoomIn: false);
			}
			else
			{
				lastZoom = true;
				Zoom(zoomIn: true);
			}
		}
		if (lastZoomInputTime > 0f && lastZoomInputTime + scrollWheelTimeframe <= Time.time)
		{
			ResetZoomTimeFrame();
			((CLogger<ACameraView>)this).Log((object)"Zoom Timeout", (Object)(object)this, (CLogLevel)0, false, false);
		}
	}

	protected override void OnDestroy()
	{
		((CLogger<ACameraView>)this).OnDestroy();
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChanged;
		}
	}

	private void ResetZoomTimeFrame()
	{
		lastZoomInputTime = -1f;
		zoomInputCumulativeValue = 0f;
	}

	private IEnumerator ShakeCoroutine(string shakeId, float delay)
	{
		if ((Object)(object)camShake == (Object)null)
		{
			InitShake();
		}
		((CLogger<ACameraView>)this).Log((object)$"[{Time.time}] Starting shake with id '{shakeId}'", (Object)(object)this, (CLogLevel)0, false, false);
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (debugStopShakesBeforeStartingNewOne)
		{
			camShake.StopShaking();
		}
		((CLogger<ACameraView>)this).Log((object)$"[{Time.time}] Playing shake with id '{shakeId}'", (Object)(object)this, (CLogLevel)0, false, false);
		ShakePreset val = null;
		foreach (ShakePreset shakePreset in camShake.ShakePresets)
		{
			if (((Object)shakePreset).name == shakeId)
			{
				val = ScriptableObject.CreateInstance<ShakePreset>();
				val.Duration = shakePreset.Duration;
				val.Strength = shakePreset.Strength;
				val.Vibrato = shakePreset.Vibrato;
				val.Randomness = shakePreset.Randomness;
				val.UseRandomInitialAngle = shakePreset.UseRandomInitialAngle;
				val.Rotation = shakePreset.Rotation;
				val.Smoothness = shakePreset.Smoothness;
				val.IgnoreTimeScale = shakePreset.IgnoreTimeScale;
				break;
			}
		}
		if ((Object)(object)val != (Object)null)
		{
			if (TPSingleton<SettingsManager>.Exist())
			{
				ShakePreset obj = val;
				obj.Strength *= TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue;
			}
			camShake.Shake(val);
		}
		else
		{
			((CLogger<ACameraView>)this).LogWarning((object)("Shake preset with id " + shakeId + " hasn't been found. Skipping shake."), (CLogLevel)1, true, false);
		}
	}

	private IEnumerator ShakeCoroutine(float duration, Vector2 strength, int vibrato, float randomness, float initialAngle, Vector3 rotation, float smoothness, float delay = 0f)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)camShake == (Object)null)
		{
			InitShake();
		}
		((CLogger<ACameraView>)this).Log((object)$"[{Time.time}] Starting custom shake'", (Object)(object)this, (CLogLevel)0, false, false);
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (debugStopShakesBeforeStartingNewOne)
		{
			camShake.StopShaking();
		}
		((CLogger<ACameraView>)this).Log((object)$"[{Time.time}] Playing custom shake", (Object)(object)this, (CLogLevel)0, false, false);
		if (TPSingleton<SettingsManager>.Exist())
		{
			strength *= TPSingleton<SettingsManager>.Instance.Settings.ScreenShakesValue;
		}
		camShake.Shake(duration, strength, vibrato, randomness, initialAngle, rotation, smoothness, false);
	}

	private IEnumerator ZoomCoroutine(bool zoomIn, bool instant = false)
	{
		if (zoomTweener != null && TweenExtensions.IsPlaying((Tween)(object)zoomTweener))
		{
			yield return TweenExtensions.WaitForCompletion((Tween)(object)zoomTweener);
			yield break;
		}
		int num = ComputeTargetPixelsPerUnit(zoomIn);
		if (instant)
		{
			pixelPerfectCam.assetsPPU = num;
			isZoomedIn = zoomIn;
			yield break;
		}
		zoomTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<int, int, NoOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<int, int, NoOptions>>(TweenSettingsExtensions.SetId<TweenerCore<int, int, NoOptions>>(DOTween.To((DOGetter<int>)(() => pixelPerfectCam.assetsPPU), (DOSetter<int>)delegate(int v)
		{
			pixelPerfectCam.assetsPPU = v;
		}, num, zoomDuration * (TPSingleton<SettingsManager>.Instance.IsTimeScaleAccelerated ? TPSingleton<SettingsManager>.Instance.Settings.SpeedScale : 1f)), "CameraZoom"), zoomEasing), (TweenCallback)delegate
		{
			zoomTweener = null;
		});
		yield return TweenExtensions.WaitForCompletion((Tween)(object)zoomTweener);
		isZoomedIn = zoomIn;
		zoomCoroutine = null;
	}

	[ContextMenu("Test Zoom Sequence")]
	private void DebugTestZoomSequence()
	{
		((MonoBehaviour)this).StartCoroutine(DebugTestZoomSequenceCoroutine());
	}

	private IEnumerator DebugTestZoomSequenceCoroutine()
	{
		yield return Zoom(zoomIn: false);
		yield return SharedYields.WaitForSeconds(0.5f);
		yield return Zoom(zoomIn: true, instant: true);
		yield return SharedYields.WaitForSeconds(0.5f);
		yield return Zoom(zoomIn: false);
	}

	[DevConsoleCommand(Name = "CameraMoveTo")]
	public static void Debug_CameraMoveTo(float x, float y)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		MoveTo(new Vector3(x, y), 0f, (Ease)0);
	}
}
