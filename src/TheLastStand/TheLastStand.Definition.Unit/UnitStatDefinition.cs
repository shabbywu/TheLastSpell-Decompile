using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

[Serializable]
public class UnitStatDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_Stat
	{
		Undefined = -1,
		Armor,
		ArmorTotal,
		Health,
		HealthTotal,
		Mana,
		ManaTotal,
		ActionPoints,
		ActionPointsTotal,
		Block,
		Critical,
		Dodge,
		Resistance,
		PhysicalDamage,
		MagicalDamage,
		InjuryDamageMultiplier,
		[Obsolete("Not used anymore", true)]
		Fate,
		HealthRegen,
		ManaRegen,
		MovePoints,
		MovePointsTotal,
		RangedDamage,
		PropagationDamage,
		CriticalPower,
		HealingReceived,
		SkillRangeModifier,
		ExperienceGainMultiplier,
		OverallDamage,
		Reliability,
		Accuracy,
		StunChanceModifier,
		PropagationBouncesModifier,
		ResistanceReduction,
		MomentumAttacks,
		OpportunisticAttacks,
		IsolatedAttacks,
		ArmorShreddingAttacks,
		PoisonDamageModifier,
		Panic,
		MultiHitsCountModifier,
		StunResistance,
		DamnedSoulsEarned,
		ExperienceGain,
		PoisonDurationModifier,
		DebuffDurationModifier,
		BuffDurationModifier,
		PotionRangeModifier,
		MagicDamageReductionModifier,
		PercentageResistanceReduction,
		BonusSkillUses,
		BonusUsableItemsUses,
		StunDurationModifier,
		ContagionDurationModifier,
		EnemyEvolutionDamageMultiplier,
		GauntletRangeModifier
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct StatComparer : IEqualityComparer<E_Stat>
	{
		public bool Equals(E_Stat x, E_Stat y)
		{
			return x == y;
		}

		public int GetHashCode(E_Stat obj)
		{
			return (int)obj;
		}
	}

	public static readonly StatComparer SharedStatComparer = default(StatComparer);

	public static Dictionary<E_Stat, bool> ShownAsPercentageDictionary = new Dictionary<E_Stat, bool>(SharedStatComparer);

	public Dictionary<DamageableType, Vector2> Boundaries { get; private set; } = new Dictionary<DamageableType, Vector2>(3, UnitTemplateDefinition.SharedUnitTypeComparer);


	public E_Stat ChildStatId { get; private set; } = E_Stat.Undefined;


	public HashSet<CompendiumEntryDefinition> CompendiumEntries { get; private set; } = new HashSet<CompendiumEntryDefinition>();


	public string Description => Localizer.Get(string.Format("{0}{1}", "UnitStat_Desc_", Id));

	public E_Stat Id { get; private set; }

	public string Name => Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", Id));

	public E_Stat ParentStatId { get; private set; } = E_Stat.Undefined;


	public string ShortName => Localizer.Get(string.Format("{0}{1}", "UnitStat_ShortName_", Id));

	public UnitStatDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"The unit stat has no Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = (E_Stat)Enum.Parse(typeof(E_Stat), val2.Value);
		XAttribute val3 = val.Attribute(XName.op_Implicit("ShownAsPercentage"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result))
			{
				ShownAsPercentageDictionary.Add(Id, result == 1);
			}
		}
		else
		{
			ShownAsPercentageDictionary.Add(Id, value: false);
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("ParentStat"));
		if (val4 != null)
		{
			if (!Enum.TryParse<E_Stat>(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)$"Could not parse {Id} ParentStat attribute value {val4.Value} to a valid stat!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			ParentStatId = result2;
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("ChildStat"));
		if (val5 != null)
		{
			if (!Enum.TryParse<E_Stat>(val5.Value, out var result3))
			{
				CLoggerManager.Log((object)$"Could not parse {Id} ChildStat attribute value {val5.Value} to a valid stat!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			ChildStatId = result3;
		}
		Vector2 value = default(Vector2);
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Boundaries")))
		{
			XAttribute val6 = item.Attribute(XName.op_Implicit("UnitType"));
			DamageableType key = (DamageableType)Enum.Parse(typeof(DamageableType), val6.Value);
			((Vector2)(ref value))._002Ector((float)int.Parse(((XContainer)item).Element(XName.op_Implicit("Min")).Value), (float)int.Parse(((XContainer)item).Element(XName.op_Implicit("Max")).Value));
			Boundaries.Add(key, value);
		}
		CompendiumEntries = new HashSet<CompendiumEntryDefinition>();
		XElement val7 = ((XContainer)val).Element(XName.op_Implicit("CompendiumEntries"));
		if (val7 == null)
		{
			return;
		}
		foreach (XElement item2 in ((XContainer)val7).Elements(XName.op_Implicit("CompendiumEntry")))
		{
			CompendiumEntries.Add(new CompendiumEntryDefinition((XContainer)(object)item2));
		}
	}
}
