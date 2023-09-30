using System;
using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Model.Unit.Perk;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class UnitPerkTierView : MonoBehaviour
{
	[SerializeField]
	private float unavailableOpacity;

	[SerializeField]
	private TextMeshProUGUI requiredPerksText;

	[SerializeField]
	private Image perksTierIndex;

	[SerializeField]
	private Image separator;

	[SerializeField]
	private List<UnitPerkDisplay> perkDisplays = new List<UnitPerkDisplay>();

	[SerializeField]
	private Material unlockedPerkMaterial;

	[SerializeField]
	private Material availablePerkMaterial;

	[SerializeField]
	private Material unavailablePerkMaterial;

	[SerializeField]
	private Material availableBackgroundMaterial;

	[SerializeField]
	private Material unavailableBackgroundMaterial;

	[SerializeField]
	private Material availablePerkTierIndexMaterial;

	[SerializeField]
	private Material unavailablePerkTierIndexMaterial;

	[SerializeField]
	private Sprite thresholdOn;

	[SerializeField]
	private Sprite thresholdOff;

	public RectTransform Separator => ((Graphic)separator).rectTransform;

	public UnitPerkTier UnitPerkTier { get; set; }

	public List<UnitPerkDisplay> PerkDisplays => perkDisplays;

	public int Tier { get; set; }

	public void RefreshAvailability(bool isFirstUnavailable)
	{
		((Graphic)perksTierIndex).material = (UnitPerkTier.Available ? availablePerkTierIndexMaterial : unavailablePerkTierIndexMaterial);
		foreach (UnitPerkDisplay perkDisplay in PerkDisplays)
		{
			perkDisplay.SetAvailabilityMaterial((perkDisplay.Perk != null && perkDisplay.Perk.Unlocked) ? unlockedPerkMaterial : (UnitPerkTier.Available ? availablePerkMaterial : unavailablePerkMaterial), UnitPerkTier.Available ? availableBackgroundMaterial : unavailableBackgroundMaterial);
		}
		for (int i = 0; i < PerkDisplays.Count; i++)
		{
			PerkDisplays[i].Refresh();
		}
		separator.sprite = (isFirstUnavailable ? thresholdOn : thresholdOff);
		((Component)requiredPerksText).gameObject.SetActive(isFirstUnavailable);
		if (isFirstUnavailable)
		{
			RefreshText();
		}
	}

	public void RefreshText()
	{
		if (UnitPerkTier != null)
		{
			((TMP_Text)requiredPerksText).text = Localizer.Format("CharacterSheet_RequiredPerks", new object[1] { PlayableUnitDatabase.UnitPerkTemplateDefinition.RequiredPerksCountPerTier[Tier] - UnitPerkTier.UnitPerkTree.PlayableUnit.UnlockedPerksCount });
		}
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
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
}
