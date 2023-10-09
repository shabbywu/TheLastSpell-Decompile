using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ChooseRewardShelf : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__21_0;

		public static TweenCallback _003C_003E9__23_0;

		internal void _003CAppear_003Eb__21_0()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}

		internal void _003CDisplaySelection_003Eb__23_0()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	[SerializeField]
	[Range(0f, 5f)]
	private float appearTweenDuration = 1f;

	[SerializeField]
	private Ease appearTweenEase = (Ease)27;

	[SerializeField]
	[Range(150f, 250f)]
	private float heightSelected = 200f;

	[SerializeField]
	[Range(0f, 3f)]
	private float selectTweenDuration = 1f;

	[SerializeField]
	private Ease selectTweenEase = (Ease)9;

	[SerializeField]
	private RectTransform shelfRectTransform;

	[SerializeField]
	private Image rarityCloth;

	[SerializeField]
	private DataSpriteTable rarityClothSprites;

	[SerializeField]
	private Image cycleIcon;

	[SerializeField]
	private DataSpriteTable cycleIconSprites;

	[SerializeField]
	private RewardItemSlotView rewardItemSlotView;

	private float heightInit;

	private Tween moveTween;

	public float AppearTweenDuration => appearTweenDuration;

	public int ItemIndex { get; set; }

	public RewardItemSlotView RewardItemSlotView => rewardItemSlotView;

	public void Appear(float delay, AudioClip appearClip)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		Tween obj = moveTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		shelfRectTransform.sizeDelta = new Vector2(shelfRectTransform.sizeDelta.x, -50f);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenerCore<Vector2, Vector2, VectorOptions> obj2 = TweenSettingsExtensions.SetDelay<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(shelfRectTransform, new Vector2(shelfRectTransform.sizeDelta.x, heightInit), appearTweenDuration, false), appearTweenEase), delay).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("RewardShelfAppear", (Component)(object)this);
		object obj3 = _003C_003Ec._003C_003E9__21_0;
		if (obj3 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__21_0 = val;
			obj3 = (object)val;
		}
		moveTween = (Tween)(object)TweenSettingsExtensions.OnStart<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(obj2, (TweenCallback)obj3), (TweenCallback)delegate
		{
			SoundManager.PlayAudioClip(appearClip);
		});
	}

	public void Disappear(float delay, AudioClip disappearClip)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		Tween obj = moveTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		shelfRectTransform.sizeDelta = new Vector2(shelfRectTransform.sizeDelta.x, heightInit);
		moveTween = (Tween)(object)TweenSettingsExtensions.OnStart<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(shelfRectTransform, new Vector2(shelfRectTransform.sizeDelta.x, -50f), appearTweenDuration, false), appearTweenEase), delay).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("RewardShelfDisappear", (Component)(object)this), (TweenCallback)delegate
		{
			SoundManager.PlayAudioClip(disappearClip);
		});
	}

	public void DisplaySelection()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		rewardItemSlotView.DisplaySelectionBG();
		Tween obj = moveTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenerCore<Vector2, Vector2, VectorOptions> obj2 = TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(shelfRectTransform, new Vector2(shelfRectTransform.sizeDelta.x, (TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem == TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.Items[ItemIndex]) ? heightSelected : heightInit), selectTweenDuration, false), selectTweenEase).SetFullId<TweenerCore<Vector2, Vector2, VectorOptions>>("RewardShelfSelection", (Component)(object)this);
		object obj3 = _003C_003Ec._003C_003E9__23_0;
		if (obj3 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__23_0 = val;
			obj3 = (object)val;
		}
		moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(obj2, (TweenCallback)obj3);
	}

	public void Refresh()
	{
		rarityCloth.sprite = rarityClothSprites.GetSpriteAt((int)(TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.Items[ItemIndex].Rarity - 1));
		cycleIcon.sprite = cycleIconSprites.GetSpriteAt(TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.IsNightProduction ? 1 : 0);
		rewardItemSlotView.RewardItemSlot.Item = TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.Items[ItemIndex];
		rewardItemSlotView.Refresh();
	}

	public void Reload()
	{
		RewardItemSlot rewardItemSlot = new RewardItemSlotController(ItemDatabase.ItemSlotDefinitions[ItemSlotDefinition.E_ItemSlotId.RewardItem], rewardItemSlotView).RewardItemSlot;
		rewardItemSlotView.ItemSlot = rewardItemSlot;
	}

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Reload();
		heightInit = shelfRectTransform.sizeDelta.y;
	}
}
