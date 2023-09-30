using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class UnitPerkDisplay : MonoBehaviour
{
	public static class Constants
	{
		public const string DefaultPerkIcon = "Default";

		public const string PerkIconPath = "View/Sprites/UI/Perks/";

		public const string PerkUnlockAnimationName = "PerkUnlock_Misc";
	}

	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private TextMeshProUGUI perkName;

	[SerializeField]
	private TextMeshProUGUI perkDescription;

	[SerializeField]
	private TextMeshProUGUI descriptionAdditionalValues;

	[SerializeField]
	private GameObject separator;

	[SerializeField]
	private Image perkBorder;

	[SerializeField]
	private RectTransform perkSelectorPos;

	[SerializeField]
	private Image perkUnlockImage;

	[SerializeField]
	private Animator perkUnlockAnimator;

	[SerializeField]
	private Canvas bookmarkCanvas;

	[SerializeField]
	private Animator bookmarkAnimator;

	[SerializeField]
	private Image bookmarkImage;

	[SerializeField]
	private AudioClip[] bookmarkAudioClips;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private Selectable defaultSelectOnLeft;

	[SerializeField]
	private bool IsOnBottomLine;

	private Tween bookmarkFadeTween;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform PerkSelectorPos => perkSelectorPos;

	public TheLastStand.Model.Unit.Perk.Perk Perk { get; private set; }

	public PerkDefinition PerkDefinition { get; private set; }

	public Selectable DefaultSelectOnLeft => defaultSelectOnLeft;

	public void DisplayBookmark(bool triggerAnimation = true)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = bookmarkFadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Color color = ((Graphic)bookmarkImage).color;
		color.a = 1f;
		((Graphic)bookmarkImage).color = color;
		((Behaviour)bookmarkCanvas).enabled = true;
		if (triggerAnimation)
		{
			bookmarkAnimator.SetTrigger("Appear");
			SoundManager.PlayAudioClip(bookmarkAudioClips[Random.Range(0, bookmarkAudioClips.Length)]);
		}
	}

	public void HideBookmark()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		bookmarkFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade(bookmarkImage, 0f, 0.3f), (TweenCallback)delegate
		{
			((Behaviour)bookmarkCanvas).enabled = false;
		});
	}

	public void Init()
	{
		if (PerkDefinition == null)
		{
			if ((Object)(object)perkIcon != (Object)null)
			{
				((Behaviour)perkIcon).enabled = false;
			}
			return;
		}
		if ((Object)(object)perkIcon != (Object)null)
		{
			((Behaviour)perkIcon).enabled = true;
			perkIcon.sprite = PerkDefinition.PerkSprite;
		}
		if ((Object)(object)perkName != (Object)null)
		{
			((TMP_Text)perkName).text = PerkDefinition.Name;
		}
		if ((Object)(object)perkDescription != (Object)null)
		{
			((TMP_Text)perkDescription).text = PerkDefinition.GetDescription((InterpreterContext)(object)Perk);
		}
		if ((Object)(object)descriptionAdditionalValues != (Object)null)
		{
			if (Perk != null && PerkDefinition.PerkEffectsInformationsExist && (Perk.Unlocked || PerkDefinition.DisplayBonusBeforePurchase))
			{
				((Component)descriptionAdditionalValues).gameObject.SetActive(true);
				separator.SetActive(true);
				((TMP_Text)descriptionAdditionalValues).text = PerkDefinition.GetAdditionDescription((InterpreterContext)(object)Perk);
			}
			else if (((Component)descriptionAdditionalValues).gameObject.activeInHierarchy || separator.activeInHierarchy)
			{
				((Component)descriptionAdditionalValues).gameObject.SetActive(false);
				separator.SetActive(false);
			}
		}
		if ((Object)(object)bookmarkCanvas != (Object)null && Perk.Bookmarked)
		{
			((Behaviour)bookmarkCanvas).enabled = true;
			DisplayBookmark(triggerAnimation: false);
		}
	}

	public void OnPerkButtonClick()
	{
		TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.SelectPerk(this);
	}

	public void OnJoystickSelect()
	{
		if (TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree == null)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.Refresh();
		}
		OnPerkButtonClick();
	}

	public void OnJoystickDeselect()
	{
		TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeController.SelectPerk(null);
		JoystickSelectable.TooltipDisplayer.HideTooltip();
	}

	public void Refresh()
	{
		if (Perk == null)
		{
			perkBorder.sprite = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>(IsOnBottomLine ? "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_Off" : "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_Off", "Misc");
			return;
		}
		perkBorder.sprite = (Perk.Unlocked ? UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>(IsOnBottomLine ? "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_On" : "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_On", Perk.CollectionId) : UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>(IsOnBottomLine ? "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_Off" : "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_Off", Perk.CollectionId));
		if (InputManager.IsLastControllerJoystick && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)this).gameObject && TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			JoystickSelectable.TooltipDisplayer.HideTooltip();
			JoystickSelectable.TooltipDisplayer.DisplayTooltip();
		}
		if ((Object)(object)bookmarkCanvas != (Object)null)
		{
			((Behaviour)bookmarkCanvas).enabled = Perk.Bookmarked;
		}
	}

	public void SetAvailabilityMaterial(Material perkMaterial, Material backgroundMaterial)
	{
		((Graphic)perkBorder).material = backgroundMaterial;
		((Graphic)perkIcon).material = perkMaterial;
	}

	public void SetContent(TheLastStand.Model.Unit.Perk.Perk perk, PerkDefinition perkDefinition = null)
	{
		Perk = perk;
		PerkDefinition = perk?.PerkDefinition ?? perkDefinition;
	}

	public void Unlock()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsPerksPanelOpened)
		{
			AnimationClip collectionAssetOrDefault = UnitPerkTreeView.GetCollectionAssetOrDefault<AnimationClip>("Animation/PerkUnlock/PerkUnlock_{0}", Perk.CollectionId);
			AnimatorOverrideController val = new AnimatorOverrideController(perkUnlockAnimator.runtimeAnimatorController);
			val["PerkUnlock_Misc"] = collectionAssetOrDefault;
			perkUnlockAnimator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val;
			((MonoBehaviour)this).StartCoroutine(UnlockCoroutine(collectionAssetOrDefault.length));
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.PlayPerkSelectionSound();
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)perkUnlockImage != (Object)null)
		{
			((Behaviour)perkUnlockImage).enabled = false;
		}
	}

	private IEnumerator UnlockCoroutine(float unlockDuration)
	{
		((Behaviour)perkUnlockAnimator).enabled = true;
		perkUnlockAnimator.SetTrigger("unlock");
		yield return SharedYields.WaitForSeconds(unlockDuration);
		((Behaviour)perkUnlockAnimator).enabled = false;
		((Behaviour)perkUnlockImage).enabled = false;
	}
}
