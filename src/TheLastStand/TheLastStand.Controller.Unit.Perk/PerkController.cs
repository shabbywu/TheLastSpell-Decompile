using System;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Serialization.Perk;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.TileMap;
using TheLastStand.View.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk;

public class PerkController
{
	public TheLastStand.Model.Unit.Perk.Perk Perk { get; }

	public PerkController(PerkDefinition perkDefinition, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isNative)
	{
		Perk = new TheLastStand.Model.Unit.Perk.Perk(perkDefinition, this, perkView, owner, perkTier, collectionId, isNative);
	}

	public PerkController(SerializedPerk serializedPerk, PerkDefinition perkDefinition, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isOwnerDead, bool isNative)
	{
		Perk = new TheLastStand.Model.Unit.Perk.Perk(serializedPerk, perkDefinition, this, perkView, owner, perkTier, collectionId, isNative);
		if (Perk.Unlocked && !isOwnerDead)
		{
			Hook(onLoad: true);
		}
	}

	public void ActiveBookmark(bool value)
	{
		Perk.Bookmarked = value;
		UnitPerkDisplay perkView = Perk.PerkView;
		if (!((Object)(object)perkView == (Object)null))
		{
			if (value)
			{
				perkView.DisplayBookmark();
			}
			else
			{
				perkView.HideBookmark();
			}
		}
	}

	public void DisplayRange(bool show)
	{
		if (Perk.PerkDefinition.HoverRanges.Count != 0)
		{
			if (show)
			{
				TPSingleton<TileMapView>.Instance.DisplayPerkHoverRangeTiles(Perk);
			}
			else
			{
				TPSingleton<TileMapView>.Instance.ClearPerkHoverRangeTiles();
			}
		}
	}

	public void UnHook(bool onLoad)
	{
		HandleHook((Action<PerkDataContainer> eventContainer, Action<PerkDataContainer> eventToHook) => (Action<PerkDataContainer>)Delegate.Remove(eventContainer, eventToHook), isHooking: false, onLoad);
	}

	private void Hook(bool onLoad)
	{
		HandleHook((Action<PerkDataContainer> eventContainer, Action<PerkDataContainer> eventToHook) => (Action<PerkDataContainer>)Delegate.Combine(eventContainer, eventToHook), isHooking: true, onLoad);
	}

	public void Lock()
	{
		Perk.PerkController.ActiveBookmark(value: false);
		if (Perk.Unlocked)
		{
			Perk.Unlocked = false;
			Perk.Owner.Log("Perk " + Perk.PerkDefinition.Id + " Locked.", (CLogLevel)0);
			UnHook(onLoad: false);
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.RefreshSkillDisplays();
		}
	}

	public void Unlock()
	{
		if (!Perk.Unlocked)
		{
			Perk.Unlocked = true;
			Perk.PerkController.ActiveBookmark(value: false);
			if ((Object)(object)Perk.PerkView != (Object)null)
			{
				Perk.PerkView.Unlock();
			}
			Perk.Owner.Log("Perk " + Perk.PerkDefinition.Id + " Unlocked.", (CLogLevel)0);
			Hook(onLoad: false);
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxPlayableUnitStatReached(Perk.Owner);
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.RefreshSkillDisplays();
		}
	}

	private void HandleHook(Func<Action<PerkDataContainer>, Action<PerkDataContainer>, Action<PerkDataContainer>> hookOperator, bool isHooking, bool onLoad)
	{
		if (isHooking)
		{
			if (!Perk.Owner.Perks.ContainsKey(Perk.PerkDefinition.Id))
			{
				Perk.Owner.Perks.Add(Perk.PerkDefinition.Id, Perk);
			}
		}
		else if (Perk.Owner.Perks.ContainsKey(Perk.PerkDefinition.Id))
		{
			Perk.Owner.Perks.Remove(Perk.PerkDefinition.Id);
		}
		foreach (APerkModule perkModule in Perk.PerkModules)
		{
			if (isHooking)
			{
				perkModule.OnUnlock(onLoad);
			}
			else
			{
				perkModule.Lock(onLoad);
			}
			foreach (APerkEffect perkEffect in perkModule.PerkEffects)
			{
				if (isHooking)
				{
					perkEffect.APerkEffectController.OnUnlock(onLoad);
				}
				else
				{
					perkEffect.APerkEffectController.Lock(onLoad);
				}
			}
			foreach (PerkEvent perkEvent in perkModule.PerkEvents)
			{
				switch (perkEvent.PerkEventDefinition.EffectTime)
				{
				case E_EffectTime.OnCreation:
					perkEvent.TryTrigger(null);
					break;
				case E_EffectTime.OnDeath:
				case E_EffectTime.OnTileCrossed:
				case E_EffectTime.OnMovementEnd:
				case E_EffectTime.OnHitTaken:
				case E_EffectTime.OnDodge:
				case E_EffectTime.OnTargetHit:
				case E_EffectTime.OnTargetDodge:
				case E_EffectTime.OnTargetKilled:
				case E_EffectTime.OnAttackDataComputed:
				case E_EffectTime.OnSkillNextHit:
				case E_EffectTime.OnSkillCastBegin:
				case E_EffectTime.OnSkillCastEnd:
				case E_EffectTime.OnSkillStatusApplied:
				case E_EffectTime.OnStatusApplied:
				case E_EffectTime.OnEnemyMovementEnd:
				case E_EffectTime.OnSkillUndo:
					if (!Perk.Owner.Events.ContainsKey(perkEvent.PerkEventDefinition.EffectTime))
					{
						CLoggerManager.Log((object)$"Tried to link a perkEvent to an owner event, but it was not present in the event dictionary: {perkEvent.PerkEventDefinition.EffectTime}", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
					}
					else
					{
						Perk.Owner.Events[perkEvent.PerkEventDefinition.EffectTime] = hookOperator(Perk.Owner.Events[perkEvent.PerkEventDefinition.EffectTime], perkEvent.TryTrigger);
					}
					break;
				default:
					if (!TPSingleton<EffectTimeEventManager>.Instance.Events.ContainsKey(perkEvent.PerkEventDefinition.EffectTime))
					{
						CLoggerManager.Log((object)$"Tried to link a perkEvent to the global EffectTimeEventManager, but it was not present in the event dictionary: {perkEvent.PerkEventDefinition.EffectTime}", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
					}
					else
					{
						TPSingleton<EffectTimeEventManager>.Instance.Events[perkEvent.PerkEventDefinition.EffectTime] = hookOperator(TPSingleton<EffectTimeEventManager>.Instance.Events[perkEvent.PerkEventDefinition.EffectTime], perkEvent.TryTrigger);
					}
					break;
				case E_EffectTime.None:
				case E_EffectTime.Permanent:
				case E_EffectTime.OnCreationAfterViewInitialized:
					break;
				}
			}
		}
	}
}
