using System;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Maths;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class GoddessSeal : MonoBehaviour
{
	[SerializeField]
	private RectTransform pivotRectTransform;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private bool isDarkShop = true;

	[SerializeField]
	private bool clickAllowed = true;

	[SerializeField]
	private bool mouseDetectionEnabled = true;

	[SerializeField]
	[Range(0f, 1f)]
	private float clickMinPercentage = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float percentage = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	private float percentageMaxSnap = 0.95f;

	[SerializeField]
	private AnimationCurve positionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private Vector2 positionAmplitudeRange = new Vector2(0f, 50f);

	[SerializeField]
	private Vector2 positionFrequencyRange = new Vector2(0f, 20f);

	[SerializeField]
	private Image sealImage;

	[SerializeField]
	private Image glowImage;

	[SerializeField]
	private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private Vector2 alphaRange = new Vector2(0f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float clickableAlpha = 1f;

	[SerializeField]
	[Min(0.1f)]
	private float glowAlphaTweenSpeed = 2f;

	[SerializeField]
	private Image vignette;

	[SerializeField]
	[Range(0f, 1f)]
	private float vignetteMinPercentage = 0.75f;

	[SerializeField]
	[Range(0f, 1f)]
	private float vignetteMaxAlpha = 1f;

	[SerializeField]
	private AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private float vignetteSmoothTime = 0.3f;

	[SerializeField]
	private ParticleSystem circlesParticleSystem;

	[SerializeField]
	private AnimationCurve circlesRateOverTimeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Range(0f, 1f)]
	private float circlesMinPercentage = 0.75f;

	[SerializeField]
	private Vector2 circlesRateOverTimeRange = new Vector2(1f, 5f);

	[SerializeField]
	private ParticleSystem lightRaysParticleSystem;

	[SerializeField]
	private ParticleSystemRenderer lightRaysParticleSystemRenderer;

	[SerializeField]
	private Vector2 lightRaysLengthScaleRange = new Vector2(6f, 10f);

	[SerializeField]
	private Vector2 lightRaysRateOverTimeRange = new Vector2(25f, 50f);

	[SerializeField]
	private AnimationCurve lightRaysCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private ParticleSystem dustParticleSystem;

	[SerializeField]
	private Vector2 dustRateOverTimeRange = new Vector2(25f, 50f);

	[SerializeField]
	private Vector2 dustStartSpeedRange = new Vector2(5f, 15f);

	[SerializeField]
	private AnimationCurve dustCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private ParticleSystem smokeParticleSystem;

	[SerializeField]
	private Vector2 smokeSpeedRange = new Vector2(1f, 3f);

	[SerializeField]
	private AudioSource hoverAudioSource;

	[SerializeField]
	[Min(0.1f)]
	private float joystickDisabledVolumeDecreaseSpeed = 2f;

	private float currentPositionFrequency;

	private float positionPhase;

	private float refVignetteAlpha;

	public bool DisablePercentage => true;

	private void Awake()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		currentPositionFrequency = VectorExtensions.Lerp(positionFrequencyRange, 0f, positionCurve);
		((Graphic)glowImage).color = ColorExtensions.WithA(((Graphic)glowImage).color, 0f);
	}

	private void Start()
	{
		if (mainCamera == null)
		{
			mainCamera = ACameraView.MainCam;
		}
	}

	private void ComputeNewPositionFrequency()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float num = VectorExtensions.Lerp(positionFrequencyRange, percentage, positionCurve);
		float num2 = (Time.time * currentPositionFrequency + positionPhase) % ((float)Math.PI * 2f);
		float num3 = Time.time * num % ((float)Math.PI * 2f);
		positionPhase = num2 - num3;
		currentPositionFrequency = num;
	}

	private void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		if (mouseDetectionEnabled && (!InputManager.IsLastControllerJoystick || TPSingleton<OraculumView>.Instance.CursorView.Enabled))
		{
			Vector3 val = mainCamera.ScreenToWorldPoint(InputManager.IsLastControllerJoystick ? ((Component)TPSingleton<OraculumView>.Instance.CursorView).transform.position : Input.mousePosition);
			Vector3 val2 = mainCamera.ScreenToWorldPoint(((Transform)pivotRectTransform).position);
			float num = Vector3.Distance(val, val2);
			float num2 = 1080f / (float)Screen.height;
			float num3 = mainCamera.orthographicSize * num2;
			float num4 = num * num2;
			percentage = Mathf.Clamp01(1f - num4 / num3);
			if (percentage > percentageMaxSnap)
			{
				percentage = 1f;
			}
		}
		else
		{
			percentage = (InputManager.IsLastControllerJoystick ? Mathf.Max(0f, percentage - Time.unscaledDeltaTime * joystickDisabledVolumeDecreaseSpeed) : 0f);
		}
		int num5;
		if (clickAllowed)
		{
			num5 = ((percentage > clickMinPercentage) ? 1 : 0);
			if (num5 != 0 && InputManager.GetSubmitButtonDown() && !TPSingleton<OraculumView>.Instance.IsInAnyShop && !TPSingleton<OraculumView>.Instance.OpeningOrClosing)
			{
				((MonoBehaviour)TPSingleton<OraculumView>.Instance).StartCoroutine(TPSingleton<OraculumView>.Instance.TransitionToShop(isDarkShop));
			}
		}
		else
		{
			num5 = 0;
		}
		if (Mathf.Abs(VectorExtensions.Lerp(positionFrequencyRange, percentage, positionCurve) - currentPositionFrequency) > 0.01f)
		{
			ComputeNewPositionFrequency();
		}
		Vector3 position = ((Transform)pivotRectTransform).position;
		position.y += Mathf.Sin(Time.time * currentPositionFrequency + positionPhase) * VectorExtensions.Lerp(positionAmplitudeRange, percentage, positionCurve);
		((Transform)rectTransform).position = position;
		Color color = ((Graphic)glowImage).color;
		float num6 = ((num5 != 0) ? 1f : 0f);
		if (color.a < num6)
		{
			float num7 = color.a + Time.deltaTime * glowAlphaTweenSpeed;
			color.a = Mathf.Clamp01(num7);
		}
		else if (color.a > num6)
		{
			float num8 = color.a - Time.deltaTime * glowAlphaTweenSpeed;
			color.a = Mathf.Clamp01(num8);
		}
		((Graphic)glowImage).color = color;
		Color color2 = ((Graphic)sealImage).color;
		float a = VectorExtensions.Lerp(alphaRange, percentage, alphaCurve);
		if (num5 != 0 && ((Graphic)glowImage).color.a >= 1f)
		{
			a = clickableAlpha;
		}
		color2.a = a;
		((Graphic)sealImage).color = color2;
		if (((Behaviour)vignette).enabled)
		{
			float num9 = Maths.Normalize01(percentage, vignetteMinPercentage, 1f);
			Color color3 = ((Graphic)vignette).color;
			float num10 = VectorExtensions.Lerp(new Vector2(0f, vignetteMaxAlpha), num9, vignetteCurve);
			color3.a = Mathf.SmoothDamp(((Graphic)vignette).color.a, num10, ref refVignetteAlpha, vignetteSmoothTime);
			((Graphic)vignette).color = color3;
		}
		if (percentage >= circlesMinPercentage)
		{
			if (!circlesParticleSystem.isPlaying)
			{
				circlesParticleSystem.Play();
			}
			EmissionModule emission = circlesParticleSystem.emission;
			float num11 = Maths.Normalize01(percentage, circlesMinPercentage, 1f);
			((EmissionModule)(ref emission)).rateOverTime = MinMaxCurve.op_Implicit(VectorExtensions.Lerp(circlesRateOverTimeRange, num11, circlesRateOverTimeCurve));
		}
		else if (circlesParticleSystem.isPlaying)
		{
			circlesParticleSystem.Stop();
		}
		lightRaysParticleSystemRenderer.lengthScale = VectorExtensions.Lerp(lightRaysLengthScaleRange, percentage);
		EmissionModule emission2 = lightRaysParticleSystem.emission;
		((EmissionModule)(ref emission2)).rateOverTimeMultiplier = VectorExtensions.Lerp(lightRaysLengthScaleRange, percentage);
		EmissionModule emission3 = dustParticleSystem.emission;
		MainModule main = dustParticleSystem.main;
		((EmissionModule)(ref emission3)).rateOverTimeMultiplier = VectorExtensions.Lerp(dustRateOverTimeRange, percentage, dustCurve);
		((MainModule)(ref main)).startSpeed = MinMaxCurve.op_Implicit(VectorExtensions.Lerp(dustStartSpeedRange, percentage, dustCurve));
		MainModule main2 = smokeParticleSystem.main;
		((MainModule)(ref main2)).simulationSpeed = VectorExtensions.Lerp(smokeSpeedRange, percentage);
		hoverAudioSource.volume = percentage;
	}
}
