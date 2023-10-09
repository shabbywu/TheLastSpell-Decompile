using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Trophy;

public class TrophyDefinition : TheLastStand.Framework.Serialization.Definition
{
	private uint damnedSoulsEarnedBase;

	public string Id { get; private set; }

	public bool IgnoreGem { get; private set; }

	public bool IsLostOnDefeat { get; private set; }

	public uint DamnedSoulsEarned
	{
		get
		{
			uint num = damnedSoulsEarnedBase;
			uint num2 = TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
			if (TPSingleton<GlyphManager>.Exist())
			{
				num2 += TPSingleton<GlyphManager>.Instance.DamnedSoulsPercentageModifier;
			}
			float num3 = 1f + (float)num2 / 100f;
			return (uint)((float)num * num3);
		}
	}

	public string TrophyToOverride { get; private set; }

	public TrophyConditionDefinition Condition { get; private set; }

	public TrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Id = val.Attribute(XName.op_Implicit("Id")).Value;
		XAttribute val2 = val.Attribute(XName.op_Implicit("LostOnDefeat"));
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("DamnedSoulsEarned"));
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("OverrideTrophy"));
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("IgnoreGem"));
		if (val2 != null)
		{
			IsLostOnDefeat = bool.Parse(val2.Value);
		}
		if (obj != null)
		{
			IgnoreGem = true;
		}
		if (val3 != null)
		{
			if (!uint.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Trophy Definition " + Id + "'s DamnedSoulsEarned " + HasAnInvalid("uint", val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			damnedSoulsEarnedBase = result;
		}
		else
		{
			CLoggerManager.Log((object)("A TrophyDefinition should have a DamnedSoulsEarned Element please add it to : " + Id), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			damnedSoulsEarnedBase = 0u;
		}
		if (val4 != null)
		{
			TrophyToOverride = val4.Value;
		}
		foreach (XElement item in ((XContainer)val).Elements())
		{
			Condition = item.Name.LocalName switch
			{
				"HealthLost" => new HealthLostTrophyDefinition((XContainer)(object)item), 
				"EnemiesKilled" => new EnemiesKilledTrophyDefinition((XContainer)(object)item), 
				"DefensesLost" => new DefensesLostTrophyDefinition((XContainer)(object)item), 
				"UsableUsed" => new UsableUsedTrophyDefinition((XContainer)(object)item), 
				"StatusInflicted" => new StatusInflictedTrophyDefinition((XContainer)(object)item), 
				"OpportunisticTriggered" => new OpportunisticTriggeredTrophyDefinition((XContainer)(object)item), 
				"OpportunismDamageInflicted" => new OpportunismDamageInflictedTrophyDefinition((XContainer)(object)item), 
				"NoHealthLost" => new NoHealthLostTrophyDefinition((XContainer)(object)item), 
				"BloodyKilledAfterEatingAllies" => new BloodyKilledAfterEatingAlliesTrophyDefinition((XContainer)(object)item), 
				"HeroSurroundedByEnemies" => new HeroSurroundedByEnemiesTrophyDefinition((XContainer)(object)item), 
				"NightCompleted" => new NightCompletedTrophyDefinition((XContainer)(object)item), 
				"NightCompletedXTurnsAfterSpawnEnd" => new NightCompletedXTurnsAfterSpawnEndTrophyDefinition((XContainer)(object)item), 
				"PerfectPanic" => new PerfectPanicTrophyDefinition((XContainer)(object)item), 
				"PunchUsed" => new PunchUsedTrophyDefinition((XContainer)(object)item), 
				"JumpOverWallUsed" => new JumpOverWallUsedTrophyDefinition((XContainer)(object)item), 
				"HealthRemainingAtMost" => new HealthRemainingAtMostTrophyDefinition((XContainer)(object)item), 
				"TilesMovedUsingSkills" => new TilesMovedUsingSkillsTrophyDefinition((XContainer)(object)item), 
				"TilesMovedBeforeMomentum" => new TilesMovedBeforeMomentumTrophyDefinition((XContainer)(object)item), 
				"EnemiesDamagedByBoomer" => new EnemiesDamagedByBoomerTrophyDefinition((XContainer)(object)item), 
				"ManaSpent" => new ManaSpentTrophyDefinition((XContainer)(object)item), 
				"HeroDead" => new HeroDeadTrophyDefinition((XContainer)(object)item), 
				"NoDodgeTriggered" => new NoDodgeTriggeredTrophyDefinition((XContainer)(object)item), 
				"DodgesPerformed" => new DodgesPerformedTrophyDefinition((XContainer)(object)item), 
				"SurviveWithFewWalls" => new SurviveWithFewWallsTrophyDefinition((XContainer)(object)item), 
				"EnemiesDamaged" => new EnemiesDamagedTrophyDefinition((XContainer)(object)item), 
				"EnemiesKilledFromWatchtower" => new EnemiesKilledFromWatchtowerTrophyDefinition((XContainer)(object)item), 
				"EnemiesKilledWithoutAttack" => new EnemiesKilledWithoutAttackTrophyDefinition((XContainer)(object)item), 
				"SpeedyKilledWithoutDodging" => new SpeedyKilledWithoutDodgingTrophyDefinition((XContainer)(object)item), 
				"EnemiesDebuffedSeveralTimesSingleTurn" => new EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition((XContainer)(object)item), 
				"BodyArmorBuffUsed" => new BodyArmorBuffUsedTrophyDefinition((XContainer)(object)item), 
				"DamageInflicted" => new DamageInflictedTrophyDefinition((XContainer)(object)item), 
				"EnemiesKilledSingleAttack" => new EnemiesKilledSingleAttackTrophyDefinition((XContainer)(object)item), 
				"CatapultUsed" => new CatapultUsedTrophyDefinition((XContainer)(object)item), 
				"BuildingsLost" => new BuildingsLostTrophyDefinition((XContainer)(object)item), 
				"EnemiesKilledByPropagation" => new EnemiesKilledByPropagationTrophyDefinition((XContainer)(object)item), 
				"EnemiesKilledByIsolated" => new EnemiesKilledByIsolatedTrophyDefinition((XContainer)(object)item), 
				"ArmoredEnemiesDamagedByArmorShredding" => new ArmoredEnemiesDamagedByArmorShreddingTrophyDefinition((XContainer)(object)item), 
				"DamageInflictedSingleAttack" => new DamageInflictedSingleAttackTrophyDefinition((XContainer)(object)item), 
				"GhostKilledWithoutDebuffing" => new GhostKilledWithoutDebuffingTrophyDefinition((XContainer)(object)item), 
				"CriticalsInflictedSingleTurn" => new CriticalsInflictedSingleTurnTrophyDefinition((XContainer)(object)item), 
				"FriendlyFire" => new FriendlyFireTrophyDefinition((XContainer)(object)item), 
				_ => Condition, 
			};
		}
	}
}
