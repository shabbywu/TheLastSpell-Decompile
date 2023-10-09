using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Seer;

public class SeerPreviewDisplay : TPSingleton<SeerPreviewDisplay>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Comparison<EnemyUnitTemplateDefinition> _003C_003E9__35_0;

		public static Func<SeerEnemyPortraitPreview, bool> _003C_003E9__41_0;

		public static TweenCallback _003C_003E9__41_1;

		public static TweenCallback _003C_003E9__42_0;

		internal int _003CDisplayEnemyPortraits_003Eb__35_0(EnemyUnitTemplateDefinition enemyA, EnemyUnitTemplateDefinition enemyB)
		{
			return enemyA.Tier.CompareTo(enemyB.Tier);
		}

		internal bool _003CFold_003Eb__41_0(SeerEnemyPortraitPreview o)
		{
			return o.QuantityDisplayed;
		}

		internal void _003CFold_003Eb__41_1()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}

		internal void _003CUnfold_003Eb__42_0()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	[SerializeField]
	private Canvas previewCanvas;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform titleRectTransform;

	[SerializeField]
	private RectTransform portraitsContainerRectTransform;

	[SerializeField]
	private Selectable foldButton;

	[SerializeField]
	private ScrollRect previewScrollRect;

	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private RectTransform scrollViewRectTransform;

	[SerializeField]
	private Selectable scrollTopButton;

	[SerializeField]
	private Selectable scrollBotButton;

	[SerializeField]
	[Range(0f, 1000f)]
	private float scrollButtonsSensitivity = 100f;

	[SerializeField]
	private RectTransform foldButtonRectTransform;

	[SerializeField]
	private float foldedTitlePosition = 200f;

	[SerializeField]
	[Range(0f, 2f)]
	private float unfoldTitleDuration = 0.4f;

	[SerializeField]
	private Ease unfoldTitleEasing = (Ease)8;

	[SerializeField]
	[Range(0f, 2f)]
	private float foldTitleDuration = 0.4f;

	[SerializeField]
	private Ease foldTitleEasing = (Ease)9;

	[SerializeField]
	private float foldedPosition = 200f;

	[SerializeField]
	private float foldedWithQuantitiesPosition = 250f;

	[SerializeField]
	private float unfoldedPosition = 200f;

	[SerializeField]
	[Range(0f, 2f)]
	private float unfoldDuration = 0.4f;

	[SerializeField]
	private Ease unfoldEasing = (Ease)8;

	[SerializeField]
	[Range(0f, 2f)]
	private float foldDuration = 0.4f;

	[SerializeField]
	private Ease foldEasing = (Ease)9;

	[SerializeField]
	private SeerEnemyPortraitPreview portraitPrefab;

	[SerializeField]
	private List<SeerEnemyPortraitPreview> portraits;

	private bool canScroll;

	private bool displayed;

	private readonly List<EnemyUnitTemplateDefinition> enemyDefinitions = new List<EnemyUnitTemplateDefinition>();

	private bool isFolded;

	private Sequence previewFoldSequence;

	public bool Displayed
	{
		get
		{
			return displayed;
		}
		set
		{
			displayed = value;
			((Behaviour)previewCanvas).enabled = displayed;
			if (displayed)
			{
				SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
				if (obj != null)
				{
					((FontLocalizedParent)obj).RegisterChilds();
				}
			}
			else
			{
				SimpleFontLocalizedParent obj2 = simpleFontLocalizedParent;
				if (obj2 != null)
				{
					((FontLocalizedParent)obj2).UnregisterChilds();
				}
			}
		}
	}

	public void DisplayEnemyPortraits()
	{
		enemyDefinitions.Clear();
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		if (SpawnWaveManager.AliveSeer)
		{
			for (int num = currentSpawnWave.RemainingEnemiesToSpawn.Count - 1; num >= 0; num--)
			{
				_ = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[currentSpawnWave.RemainingEnemiesToSpawn[num].Id];
				if (!enemyDefinitions.Contains(currentSpawnWave.RemainingEnemiesToSpawn[num]))
				{
					enemyDefinitions.Add(currentSpawnWave.RemainingEnemiesToSpawn[num]);
				}
			}
			for (int num2 = currentSpawnWave.RemainingEliteEnemiesToSpawn.Count - 1; num2 >= 0; num2--)
			{
				EnemyUnitTemplateDefinition item = EnemyUnitDatabase.EnemyUnitTemplateDefinitions[currentSpawnWave.RemainingEliteEnemiesToSpawn[num2].Id];
				if (!enemyDefinitions.Contains(item))
				{
					enemyDefinitions.Add(item);
				}
			}
		}
		enemyDefinitions.Sort((EnemyUnitTemplateDefinition enemyA, EnemyUnitTemplateDefinition enemyB) => enemyA.Tier.CompareTo(enemyB.Tier));
		if (currentSpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			enemyDefinitions.Insert(0, BossUnitDatabase.BossUnitTemplateDefinitions[currentSpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.BossWaveSettings.BossUnitTemplateId]);
		}
		int i = 0;
		InstantiatePortraitsIfNeeded(enemyDefinitions.Count);
		for (; i < enemyDefinitions.Count; i++)
		{
			SeerEnemyPortraitPreview seerEnemyPortraitPreview = portraits[i];
			EnemyUnitTemplateDefinition enemyDefinition = enemyDefinitions[i];
			bool flag = enemyDefinition is BossUnitTemplateDefinition;
			bool flag2 = enemyDefinition.Tier == 1 || SpawnWaveManager.DisplayAllEnemyTiers || flag;
			bool display = flag2 && SpawnWaveManager.DisplayQuantities && !flag;
			seerEnemyPortraitPreview.SetEnemyInfo(enemyDefinition, !flag2, flag);
			int enemyQuantity = ((!currentSpawnWave.SpawnWaveDefinition.IsBossWave) ? (currentSpawnWave.RemainingEnemiesToSpawn.Count((EnemyUnitTemplateDefinition o) => o.Id == enemyDefinition.Id) + currentSpawnWave.RemainingEliteEnemiesToSpawn.Count((EliteEnemyUnitTemplateDefinition o) => o.Id == enemyDefinition.Id)) : (-1));
			seerEnemyPortraitPreview.SetEnemyQuantity(enemyQuantity, display);
			seerEnemyPortraitPreview.Display(show: true);
		}
		for (; i < portraits.Count; i++)
		{
			portraits[i].Display(show: false);
		}
		RefreshPanel();
	}

	public void DisplayQuantitiesOnRevealedEnemies()
	{
		for (int num = portraits.Count - 1; num >= 0; num--)
		{
			SeerEnemyPortraitPreview seerEnemyPortraitPreview = portraits[num];
			seerEnemyPortraitPreview.QuantityDisplayed = seerEnemyPortraitPreview.Displayed && !seerEnemyPortraitPreview.IsBossEnemy && !seerEnemyPortraitPreview.HiddenEnemy;
		}
	}

	public void OnBotButtonClick()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		previewScrollRect.verticalScrollbar.value = Mathf.Clamp01(previewScrollRect.verticalScrollbar.value - scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	public void OnFoldButtonClick()
	{
		if (isFolded)
		{
			Unfold();
		}
		else
		{
			Fold();
		}
	}

	public void OnTopButtonClick()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		previewScrollRect.verticalScrollbar.value = Mathf.Clamp01(previewScrollRect.verticalScrollbar.value + scrollButtonsSensitivity / contentRectTransform.sizeDelta.y);
	}

	protected override void Awake()
	{
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChange;
	}

	private void Fold()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		if (isFolded)
		{
			return;
		}
		float num = (portraits.Any((SeerEnemyPortraitPreview o) => o.QuantityDisplayed) ? foldedWithQuantitiesPosition : foldedPosition);
		Sequence obj = previewFoldSequence;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		previewFoldSequence = DOTween.Sequence();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenSettingsExtensions.Join(previewFoldSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(portraitsContainerRectTransform, num, foldDuration, false), foldEasing));
		Sequence obj2 = TweenSettingsExtensions.Join(previewFoldSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(titleRectTransform, foldedTitlePosition, foldTitleDuration, false), foldTitleEasing)).SetFullId<Sequence>("SeerPreviewFoldTween", (Component)(object)this);
		object obj3 = _003C_003Ec._003C_003E9__41_1;
		if (obj3 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__41_1 = val;
			obj3 = (object)val;
		}
		TweenSettingsExtensions.OnComplete<Sequence>(obj2, (TweenCallback)obj3);
		isFolded = true;
		((Transform)foldButtonRectTransform).localScale = new Vector3(1f, 1f, 1f);
	}

	private void Unfold()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		if (!isFolded)
		{
			return;
		}
		Sequence obj = previewFoldSequence;
		if (obj != null)
		{
			TweenExtensions.Kill((Tween)(object)obj, false);
		}
		previewFoldSequence = DOTween.Sequence();
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenSettingsExtensions.Join(previewFoldSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(portraitsContainerRectTransform, unfoldedPosition, unfoldDuration, false), unfoldEasing));
		Sequence obj2 = TweenSettingsExtensions.Join(previewFoldSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(titleRectTransform, 0f, unfoldTitleDuration, false), unfoldTitleEasing)).SetFullId<Sequence>("SeerPreviewFoldTween", (Component)(object)this);
		object obj3 = _003C_003Ec._003C_003E9__42_0;
		if (obj3 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__42_0 = val;
			obj3 = (object)val;
		}
		TweenSettingsExtensions.OnComplete<Sequence>(obj2, (TweenCallback)obj3);
		((Transform)foldButtonRectTransform).localScale = new Vector3(-1f, 1f, 1f);
		isFolded = false;
	}

	private void InstantiatePortraitsIfNeeded(int count)
	{
		int num = count - portraits.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				SeerEnemyPortraitPreview item = Object.Instantiate<SeerEnemyPortraitPreview>(portraitPrefab, (Transform)(object)contentRectTransform);
				portraits.Add(item);
			}
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChange;
		}
	}

	private void OnResolutionChange(Resolution resolution)
	{
		RefreshPanel();
	}

	private void RefreshPanel()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentRectTransform);
		Rect rect = contentRectTransform.rect;
		float height = ((Rect)(ref rect)).height;
		rect = portraitsContainerRectTransform.rect;
		canScroll = height > ((Rect)(ref rect)).height - Mathf.Abs(scrollViewRectTransform.offsetMax.y) - Mathf.Abs(scrollViewRectTransform.offsetMin.y);
		previewScrollRect.vertical = canScroll;
		((Component)scrollTopButton).gameObject.SetActive(canScroll);
		((Component)scrollBotButton).gameObject.SetActive(canScroll);
		if (!canScroll)
		{
			previewScrollRect.verticalScrollbar.value = 1f;
		}
		foldButton.SetSelectOnDown(canScroll ? scrollTopButton : null);
	}
}
