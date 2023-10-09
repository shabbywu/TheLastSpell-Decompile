using System;
using System.Collections;
using TMPro;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Trait;

public class UnitTraitDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI traitTitleText;

	[SerializeField]
	private TextMeshProUGUI traitDescriptionText;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image textImage;

	[SerializeField]
	private Sprite textHoveredSprite;

	private UnitTraitDefinition traitDefinition;

	private TheLastStand.Model.Unit.Unit targetUnit;

	private Sprite textInitSprite;

	public TheLastStand.Model.Unit.Unit TargetUnit
	{
		get
		{
			return targetUnit ?? TileObjectSelectionManager.SelectedUnit;
		}
		set
		{
			if (targetUnit != value)
			{
				targetUnit = value;
			}
		}
	}

	public UnitTraitDefinition UnitTraitDefinition
	{
		get
		{
			return traitDefinition;
		}
		set
		{
			if (traitDefinition != value)
			{
				traitDefinition = value;
			}
		}
	}

	public void OnJoystickSelect()
	{
		textImage.sprite = textHoveredSprite;
	}

	public void OnJoystickDeselect()
	{
		textImage.sprite = textInitSprite;
	}

	public void Refresh()
	{
		((Component)this).gameObject.SetActive(UnitTraitDefinition != null);
		if (UnitTraitDefinition == null)
		{
			return;
		}
		RefreshText();
		if ((Object)(object)iconImage != (Object)null)
		{
			string text = ((!UnitTraitDefinition.IsBackgroundTrait) ? (PlayableUnitDatabase.UnitTraitTiersId.ContainsKey(UnitTraitDefinition.Cost) ? PlayableUnitDatabase.UnitTraitTiersId[UnitTraitDefinition.Cost] : "Default") : ("Background_" + (PlayableUnitDatabase.UnitBackgroundTraitTiersId.ContainsKey(UnitTraitDefinition.Cost) ? PlayableUnitDatabase.UnitBackgroundTraitTiersId[UnitTraitDefinition.Cost] : "Default")));
			iconImage.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Traits/Icons_Traits_" + UnitTraitDefinition.Id, failSilently: true);
			if ((Object)(object)iconImage.sprite == (Object)null)
			{
				iconImage.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Traits/Icons_Traits_" + text, failSilently: false);
			}
		}
	}

	private void Awake()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if ((Object)(object)textImage != (Object)null)
		{
			textInitSprite = textImage.sprite;
		}
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void RefreshText()
	{
		if (UnitTraitDefinition != null)
		{
			if ((Object)(object)traitTitleText != (Object)null)
			{
				((TMP_Text)traitTitleText).text = UnitTraitDefinition.Name;
			}
			if ((Object)(object)traitDescriptionText != (Object)null)
			{
				((TMP_Text)traitDescriptionText).text = UnitTraitDefinition.GetDescription();
			}
		}
	}

	private IEnumerator Start()
	{
		yield return GameManager.WaitForGameInit;
		Refresh();
	}
}
