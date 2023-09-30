using System.Collections;
using System.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ReplacePerkEffectController : APerkEffectController
{
	public ReplacePerkEffect ReplacePerkEffect => base.PerkEffect as ReplacePerkEffect;

	public ReplacePerkEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ReplacePerkEffect(aPerkEffectDefinition as ReplacePerkEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		if (!onLoad)
		{
			ReplacePerk(ReplacePerkEffect.ReplacePerkEffectDefinition.PerkToReplaceId, ReplacePerkEffect.ReplacePerkEffectDefinition.PerkReplacementId);
		}
	}

	public override void Lock(bool onLoad)
	{
		if (!onLoad)
		{
			ReplacePerk(ReplacePerkEffect.ReplacePerkEffectDefinition.PerkReplacementId, ReplacePerkEffect.ReplacePerkEffectDefinition.PerkToReplaceId);
		}
	}

	private bool ReplacePerk(string perkToReplaceId, string perkReplacementId)
	{
		if (!base.PerkEffect.APerkModule.Perk.Owner.Perks.ContainsKey(perkReplacementId))
		{
			TheLastStand.Model.Unit.Perk.Perk perk = null;
			UnitPerkTier unitPerkTier = null;
			foreach (UnitPerkTier unitPerkTier2 in base.PerkEffect.APerkModule.Perk.Owner.PerkTree.UnitPerkTiers)
			{
				perk = (unitPerkTier = unitPerkTier2).Perks.FirstOrDefault((TheLastStand.Model.Unit.Perk.Perk p) => p.PerkDefinition.Id == perkToReplaceId);
				if (perk != null)
				{
					break;
				}
			}
			if (perk == null)
			{
				return false;
			}
			((MonoBehaviour)base.PerkEffect.APerkModule.Perk.Owner.UnitView).StartCoroutine(ReplacePerkCoroutine(perk, unitPerkTier, perkReplacementId));
			return true;
		}
		return false;
	}

	private IEnumerator ReplacePerkCoroutine(TheLastStand.Model.Unit.Perk.Perk perkToReplace, UnitPerkTier unitPerkTier, string perkReplacementId)
	{
		yield return (object)new WaitForSeconds(Random.Range(PlayableUnitManager.PerkReplacementRandomDelay.x, PlayableUnitManager.PerkReplacementRandomDelay.y));
		bool unlocked = perkToReplace.Unlocked;
		bool bookmarked = perkToReplace.Bookmarked;
		perkToReplace.PerkController.Lock();
		PerkController perkController = new PerkController(PlayableUnitDatabase.PerkDefinitions[perkReplacementId], perkToReplace.PerkView, base.PerkEffect.APerkModule.Perk.Owner, perkToReplace.PerkTier, perkToReplace.CollectionId, isNative: false);
		unitPerkTier.Perks[unitPerkTier.Perks.IndexOf(perkToReplace)] = perkController.Perk;
		perkController.Perk.PerkView.SetContent(perkController.Perk);
		perkController.Perk.PerkView.Init();
		if (unlocked)
		{
			base.PerkEffect.APerkModule.Perk.Owner.PerkTree.UnitPerkTreeController.UnlockPerk(perkReplacementId);
		}
		if (bookmarked)
		{
			perkController.Perk.PerkController.ActiveBookmark(value: true);
		}
	}
}
