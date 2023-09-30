using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.UI;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TheLastStand.View.Camera;

public class CameraView : ACameraView
{
	[SerializeField]
	[Tooltip("Time taken in seconds to execute a camera movement. Only used in animation, like EnemyUnit hitting something.")]
	private float animationMoveSpeed = 0.2f;

	[SerializeField]
	private PostProcessVolume blurPostProcessVolume;

	[SerializeField]
	private float blurWeightStartValue = 0.5f;

	[SerializeField]
	private float blurWeightEndValue = 1f;

	[SerializeField]
	private float blurTransitionDuration = 1f;

	[SerializeField]
	private Ease blurInEasing = (Ease)12;

	[SerializeField]
	private Ease blurOutEasing = (Ease)11;

	[SerializeField]
	[Range(0f, 7f)]
	private float dayNightTransitionDuration = 2f;

	[SerializeField]
	private Ease dayNightTransitionEasing = (Ease)4;

	[SerializeField]
	private PostProcessVolume vignetteNightPostProcessVolume;

	[SerializeField]
	private PostProcessVolume bloomNightPostProcessVolume;

	[SerializeField]
	private CameraLUTView lutView;

	[SerializeField]
	private CameraVisualEffects visualEffects;

	[SerializeField]
	private RippleEffect rippleEffect;

	[SerializeField]
	private CameraPOI cameraPOI;

	[SerializeField]
	private CameraVisionArea cameraVision;

	[SerializeField]
	private CameraUIMasksHandler cameraUIMasksHandler;

	private ProCamera2DNumericBoundaries camBoundaries;

	private Tweener uiBlurTweener;

	private Tweener dayNightPostProcessTweener;

	public static float AnimationMoveSpeed => ((CameraView)TPSingleton<ACameraView>.Instance).animationMoveSpeed;

	public static CameraVisualEffects CameraVisualEffects => ((CameraView)TPSingleton<ACameraView>.Instance).visualEffects;

	public static CameraPOI CameraPOI => ((CameraView)TPSingleton<ACameraView>.Instance).cameraPOI;

	public static CameraLUTView CameraLutView => ((CameraView)TPSingleton<ACameraView>.Instance).lutView;

	public static CameraVisionArea CameraVision => ((CameraView)TPSingleton<ACameraView>.Instance).cameraVision;

	public static CameraUIMasksHandler CameraUIMasksHandler
	{
		get
		{
			CameraView cameraView = TPSingleton<ACameraView>.Instance as CameraView;
			if ((Object)(object)cameraView == (Object)null)
			{
				return null;
			}
			if ((Object)(object)cameraView.cameraUIMasksHandler == (Object)null)
			{
				cameraView.cameraUIMasksHandler = Object.FindObjectOfType<CameraUIMasksHandler>();
			}
			return cameraView.cameraUIMasksHandler;
		}
	}

	public static ProCamera2DNumericBoundaries ProCamera2DNumericBoundaries => ((CameraView)TPSingleton<ACameraView>.Instance).camBoundaries;

	public static RippleEffect RippleEffect => ((CameraView)TPSingleton<ACameraView>.Instance).rippleEffect;

	public static void OnGameStateChange(Game.E_State state)
	{
		switch (state)
		{
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.Shopping:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.Settings:
		case Game.E_State.HowToPlay:
		case Game.E_State.GameOver:
		case Game.E_State.MetaShops:
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		case Game.E_State.CutscenePlaying:
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			break;
		case Game.E_State.UnitPreparingSkill:
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.BuildingPreparingSkill:
		case Game.E_State.BuildingExecutingSkill:
			ACameraView.AllowUserPan = TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.EnemyUnits;
			ACameraView.AllowUserZoom = ACameraView.AllowUserPan;
			break;
		default:
			ACameraView.AllowUserPan = !FogManager.MovingCameraToFog;
			ACameraView.AllowUserZoom = ACameraView.AllowUserPan;
			break;
		case Game.E_State.Wait:
			break;
		}
	}

	public static void AttenuateWorldForPopupFocus(IOverlayUser overlayUser)
	{
		if (TPSingleton<ACameraView>.Exist())
		{
			if (overlayUser != null)
			{
				TPSingleton<TPOverlay>.Instance.Display(overlayUser, -1f);
				((CameraView)TPSingleton<ACameraView>.Instance).BlurWorld(doBlur: true);
			}
			else
			{
				TPSingleton<TPOverlay>.Instance.Hide(-1f);
				((CameraView)TPSingleton<ACameraView>.Instance).BlurWorld(doBlur: false);
			}
		}
	}

	public static void RefreshDayTimeEffects(bool instant = false, bool onLoad = false)
	{
		if (!onLoad)
		{
			CameraLutView.RefreshLut(instant);
		}
		((CameraView)TPSingleton<ACameraView>.Instance).RefreshDayNightPostProcess(instant);
	}

	public void BlurWorld(bool doBlur, bool instant = false)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		float num = (doBlur ? blurWeightEndValue : blurWeightStartValue);
		if (instant)
		{
			blurPostProcessVolume.weight = num;
			((Behaviour)blurPostProcessVolume).enabled = doBlur;
			return;
		}
		if (uiBlurTweener != null)
		{
			TweenExtensions.Kill((Tween)(object)uiBlurTweener, false);
		}
		uiBlurTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => blurPostProcessVolume.weight), (DOSetter<float>)delegate(float v)
		{
			blurPostProcessVolume.weight = v;
		}, num, blurTransitionDuration), "CameraBlur", (Component)(object)this), doBlur ? blurInEasing : blurOutEasing), (TweenCallback)delegate
		{
			uiBlurTweener = null;
		});
		if (doBlur)
		{
			TweenSettingsExtensions.OnStart<Tweener>(uiBlurTweener, (TweenCallback)delegate
			{
				((Behaviour)blurPostProcessVolume).enabled = true;
			});
		}
		else
		{
			TweenSettingsExtensions.OnComplete<Tweener>(uiBlurTweener, (TweenCallback)delegate
			{
				((Behaviour)blurPostProcessVolume).enabled = false;
			});
		}
	}

	public void ChangeBoundaries(Vector4 boundaries)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		camBoundaries.TopBoundary = boundaries.x;
		camBoundaries.BottomBoundary = boundaries.y;
		camBoundaries.LeftBoundary = boundaries.z;
		camBoundaries.RightBoundary = boundaries.w;
	}

	public override void Init()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		if (TPSingleton<GameManager>.Exist())
		{
			ChangeBoundaries(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.CameraBoundaries);
			CameraLutView.UpdateLutTextures();
		}
	}

	protected override void Awake()
	{
		ProCamera2D instance = ProCamera2D.Instance;
		camBoundaries = ((instance != null) ? ((Component)instance).GetComponent<ProCamera2DNumericBoundaries>() : null);
		base.Awake();
	}

	protected override int ComputeTargetPixelsPerUnit(bool zoomedIn)
	{
		int num = base.ComputeTargetPixelsPerUnit(zoomedIn);
		if (Screen.height >= 1440)
		{
			num /= 2;
		}
		return num;
	}

	protected override void Update()
	{
		if (TPSingleton<GameManager>.Exist())
		{
			switch (TPSingleton<GameManager>.Instance.Game.State)
			{
			case Game.E_State.CharacterSheet:
			case Game.E_State.Recruitment:
			case Game.E_State.Shopping:
			case Game.E_State.BuildingUpgrade:
			case Game.E_State.NightReport:
			case Game.E_State.ProductionReport:
			case Game.E_State.Settings:
			case Game.E_State.HowToPlay:
			case Game.E_State.MetaShops:
				return;
			}
		}
		base.Update();
	}

	private void RefreshDayNightPostProcess(bool instant = false)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
		float num = (flag ? 1 : 0);
		if (instant)
		{
			vignetteNightPostProcessVolume.weight = num;
			((Behaviour)vignetteNightPostProcessVolume).enabled = flag;
			bloomNightPostProcessVolume.weight = num;
			((Behaviour)bloomNightPostProcessVolume).enabled = flag;
			return;
		}
		Tweener obj = dayNightPostProcessTweener;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		dayNightPostProcessTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => vignetteNightPostProcessVolume.weight), (DOSetter<float>)delegate(float v)
		{
			vignetteNightPostProcessVolume.weight = v;
			bloomNightPostProcessVolume.weight = v;
		}, num, dayNightTransitionDuration), "NightPostProcess", (Component)(object)this), dayNightTransitionEasing), (TweenCallback)delegate
		{
			dayNightPostProcessTweener = null;
		});
		if (flag)
		{
			TweenSettingsExtensions.OnStart<Tweener>(dayNightPostProcessTweener, (TweenCallback)delegate
			{
				((Behaviour)vignetteNightPostProcessVolume).enabled = true;
				((Behaviour)bloomNightPostProcessVolume).enabled = true;
			});
		}
		else
		{
			TweenSettingsExtensions.OnComplete<Tweener>(dayNightPostProcessTweener, (TweenCallback)delegate
			{
				((Behaviour)vignetteNightPostProcessVolume).enabled = false;
				((Behaviour)bloomNightPostProcessVolume).enabled = false;
			});
		}
	}

	[ContextMenu("Blur OFF")]
	private void DebugDisableBlur()
	{
		BlurWorld(doBlur: false);
	}

	[ContextMenu("Blur ON")]
	private void DebugEnableBlur()
	{
		BlurWorld(doBlur: true);
	}
}
