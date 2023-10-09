using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Database;
using TheLastStand.Definition.Panic;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model.Panic;
using UnityEngine;

namespace TheLastStand.View.Panic;

public class PanicPanel : MonoBehaviour
{
	[SerializeField]
	private float closedPanelPosY;

	[SerializeField]
	private float openedPanelPosY = -100f;

	[SerializeField]
	[Range(0f, 5f)]
	private float openPanelTweenDuration = 0.7f;

	[SerializeField]
	[Range(0f, 5f)]
	private float closePanelTweenDuration = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float panicGaugeTweenDuration = 0.2f;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private BetterSlider panicGaugeSlider;

	[SerializeField]
	private BetterSlider panicPreviewGaugeSlider;

	[SerializeField]
	private Transform panicLevelsPanelTransform;

	[SerializeField]
	private PanicLevel panicLevelPrefab;

	private PanicLevel[] panicLevels;

	private Sequence movePanelSequence;

	private Sequence refreshPanicGaugeSequence;

	private float targetPanicGaugeSliderValue;

	private Sequence refreshPanicPreviewGaugeSequence;

	private float targetPanicPreviewGaugeSliderValue;

	private float panicLevelsPanelWidth;

	public float ClosePanelTweenDuration => closePanelTweenDuration;

	public void Close()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((Component)panicLevelsPanelTransform).gameObject.SetActive(false);
			if (movePanelSequence == null)
			{
				movePanelSequence = TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), "MovePanel");
			}
			TweenSettingsExtensions.Append(movePanelSequence, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, closedPanelPosY, closePanelTweenDuration, true).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("ClosePanicPanel", (Component)(object)this), (Ease)26), (TweenCallback)delegate
			{
				((Component)this).gameObject.SetActive(false);
			}));
		}
	}

	public void InstantiatePanicLevels()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		panicLevels = new PanicLevel[PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length];
		for (int num = PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length - 1; num > 0; num--)
		{
			PanicLevelDefinition panicLevelDefinition = PanicDatabase.PanicDefinition.PanicLevelDefinitions[num];
			PanicLevel panicLevel = Object.Instantiate<PanicLevel>(panicLevelPrefab, panicLevelsPanelTransform);
			((Component)panicLevel).GetComponent<RectTransform>().anchoredPosition = new Vector2(panicLevelDefinition.PanicValueNeeded / PanicDatabase.PanicDefinition.ValueMax * panicLevelsPanelWidth, 0f);
			panicLevel.InstantiateIndicators(num);
			panicLevels[num] = panicLevel;
		}
	}

	public void Open()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		if (!((Component)this).gameObject.activeInHierarchy && UIManager.DebugToggleUI != false)
		{
			((Component)this).gameObject.SetActive(true);
			if (movePanelSequence == null)
			{
				movePanelSequence = TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), "MovePanel");
			}
			TweenSettingsExtensions.Append(movePanelSequence, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, openedPanelPosY, openPanelTweenDuration, true).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("OpenPanicPanel", (Component)(object)this), (Ease)30), (TweenCallback)delegate
			{
				((Component)panicLevelsPanelTransform).gameObject.SetActive(true);
			}));
		}
	}

	public void Refresh(TheLastStand.Model.Panic.Panic panic)
	{
		RefreshPanicValue(panic);
		RefreshPanicExpectedValue(panic);
	}

	public void RefreshPanicExpectedValue(TheLastStand.Model.Panic.Panic panic)
	{
		float num = panic.ExpectedValue / panic.PanicDefinition.ValueMax;
		if (num != targetPanicPreviewGaugeSliderValue)
		{
			targetPanicPreviewGaugeSliderValue = num;
			if (refreshPanicPreviewGaugeSequence == null)
			{
				refreshPanicPreviewGaugeSequence = TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), "RefreshPanicPreviewGauge");
			}
			TweenSettingsExtensions.Append(refreshPanicPreviewGaugeSequence, (Tween)(object)DOTween.To((DOGetter<float>)(() => panicPreviewGaugeSlider.value), (DOSetter<float>)delegate(float x)
			{
				panicPreviewGaugeSlider.value = x;
			}, targetPanicPreviewGaugeSliderValue, panicGaugeTweenDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("RefreshPanicPreviewGauge", (Component)(object)this));
		}
		RefreshPanicLevels(panic);
	}

	public void RefreshPanicLevels(TheLastStand.Model.Panic.Panic panic)
	{
		if (UIManager.DebugToggleUI == false)
		{
			return;
		}
		for (int num = PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length - 1; num > 0; num--)
		{
			if (num <= panic.Level)
			{
				panicLevels[num].Activate();
				panicLevels[num].DisactivateThresholdMaterial();
			}
			else
			{
				panicLevels[num].Disactivate();
				if (num <= panic.ExpectedLevel)
				{
					panicLevels[num].ActivateThresholdMaterial();
				}
				else
				{
					panicLevels[num].DisactivateThresholdMaterial();
				}
			}
		}
	}

	public void RefreshPanicValue(TheLastStand.Model.Panic.Panic panic)
	{
		float num = panic.Value / panic.PanicDefinition.ValueMax;
		if (num != targetPanicGaugeSliderValue)
		{
			targetPanicGaugeSliderValue = num;
			if (refreshPanicGaugeSequence == null)
			{
				refreshPanicGaugeSequence = TweenSettingsExtensions.SetId<Sequence>(DOTween.Sequence(), "RefreshPanicGauge");
			}
			TweenSettingsExtensions.Append(refreshPanicGaugeSequence, (Tween)(object)DOTween.To((DOGetter<float>)(() => panicGaugeSlider.value), (DOSetter<float>)delegate(float value)
			{
				panicGaugeSlider.value = value;
			}, targetPanicGaugeSliderValue, panicGaugeTweenDuration).SetFullId<TweenerCore<float, float, FloatOptions>>("RefreshPanicGauge", (Component)(object)this));
			RefreshPanicLevels(panic);
		}
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		panicLevelsPanelWidth = ((Component)panicLevelsPanelTransform).GetComponent<RectTransform>().sizeDelta.x;
		InstantiatePanicLevels();
	}
}
