using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class InjuryDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_ValueMultiplier
	{
		None,
		ThreeQuarter,
		TwoThird,
		Half,
		Third,
		Quarter,
		Zero
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ValueMultiplierComparer : IEqualityComparer<E_ValueMultiplier>
	{
		public bool Equals(E_ValueMultiplier x, E_ValueMultiplier y)
		{
			return x == y;
		}

		public int GetHashCode(E_ValueMultiplier obj)
		{
			return (int)obj;
		}
	}

	public static readonly Dictionary<E_ValueMultiplier, float> Multipliers = new Dictionary<E_ValueMultiplier, float>(default(ValueMultiplierComparer))
	{
		{
			E_ValueMultiplier.None,
			1f
		},
		{
			E_ValueMultiplier.ThreeQuarter,
			0.75f
		},
		{
			E_ValueMultiplier.TwoThird,
			0.66f
		},
		{
			E_ValueMultiplier.Half,
			0.5f
		},
		{
			E_ValueMultiplier.Third,
			0.33f
		},
		{
			E_ValueMultiplier.Quarter,
			0.25f
		},
		{
			E_ValueMultiplier.Zero,
			0f
		}
	};

	public float BaseHealth { get; private set; }

	public float BaseRatio { get; private set; }

	public float BaseThreshold => BaseHealth * BaseRatio;

	public List<string> PreventedSkillsIds { get; private set; } = new List<string>();


	public float RatioMultiplier { get; private set; }

	public List<RemoveStatusDefinition> RemoveStatusDefinitions { get; private set; } = new List<RemoveStatusDefinition>();


	public Dictionary<UnitStatDefinition.E_Stat, float> StatModifiers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, float>(UnitStatDefinition.SharedStatComparer);


	public Dictionary<UnitStatDefinition.E_Stat, E_ValueMultiplier> StatMultipliers { get; private set; } = new Dictionary<UnitStatDefinition.E_Stat, E_ValueMultiplier>(UnitStatDefinition.SharedStatComparer);


	public List<StatusEffectDefinition> Statuses { get; private set; } = new List<StatusEffectDefinition>();


	public InjuryDefinition(XContainer container, float baseHealth)
		: base(container)
	{
		BaseHealth = baseHealth;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("BaseRatio"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"Injury definition must have a BaseRatio element.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log((object)("Could not parse Injury definition BaseRatio element value " + val2.Value + " to a float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		BaseRatio = result;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("RatioMultiplier"));
		if (val3.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"Injury definition must have a RatioMultiplier element.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			CLoggerManager.Log((object)("Could not parse Injury definition RatioMultiplier element value " + val3.Value + " to a float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RatioMultiplier = result2;
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("StatModifiers"));
		if (val4 != null)
		{
			foreach (XElement item9 in ((XContainer)val4).Elements(XName.op_Implicit("StatModifier")))
			{
				XAttribute val5 = item9.Attribute(XName.op_Implicit("Id"));
				if (val5.IsNullOrEmpty())
				{
					CLoggerManager.Log((object)"StatModifier must have an Id attribute.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val5.Value, out var result3))
				{
					CLoggerManager.Log((object)$"StatModifier attribute Id {result3} must be a valid UnitStatDefinition.E_Stat value.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				XAttribute val6 = item9.Attribute(XName.op_Implicit("Offset"));
				XAttribute val7 = item9.Attribute(XName.op_Implicit("Multiplier"));
				E_ValueMultiplier result5;
				if (val6 != null)
				{
					float result4;
					if (val6.IsNullOrEmpty())
					{
						CLoggerManager.Log((object)"StatModifier must have an Offset attribute.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else if (!float.TryParse(val6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result4))
					{
						CLoggerManager.Log((object)("Could not parse Injury definition StatModifier Offset attribute value " + val6.Value + " to a float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else
					{
						StatModifiers.Add(result3, result4);
					}
				}
				else if (val7.IsNullOrEmpty())
				{
					CLoggerManager.Log((object)"StatModifier must have a Multiplier attribute.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else if (!Enum.TryParse<E_ValueMultiplier>(val7.Value, out result5))
				{
					CLoggerManager.Log((object)("Could not parse Injury definition StatModifier Multiplier attribute value " + val7.Value + " to a " + typeof(E_ValueMultiplier).Name + " value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					StatMultipliers.Add(result3, result5);
				}
			}
		}
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("PreventSkills"));
		if (val8 != null)
		{
			foreach (XElement item10 in ((XContainer)val8).Elements(XName.op_Implicit("SkillId")))
			{
				PreventedSkillsIds.Add(item10.Value);
			}
		}
		foreach (XElement item11 in ((XContainer)val).Elements(XName.op_Implicit("Buff")))
		{
			BuffEffectDefinition item = new BuffEffectDefinition((XContainer)(object)item11);
			Statuses.Add(item);
		}
		foreach (XElement item12 in ((XContainer)val).Elements(XName.op_Implicit("Debuff")))
		{
			DebuffEffectDefinition item2 = new DebuffEffectDefinition((XContainer)(object)item12);
			Statuses.Add(item2);
		}
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("Stun"));
		if (val9 != null)
		{
			StunEffectDefinition item3 = new StunEffectDefinition((XContainer)(object)val9);
			Statuses.Add(item3);
		}
		XElement val10 = ((XContainer)val).Element(XName.op_Implicit("Poison"));
		if (val10 != null)
		{
			PoisonEffectDefinition item4 = new PoisonEffectDefinition((XContainer)(object)val10);
			Statuses.Add(item4);
		}
		XElement val11 = ((XContainer)val).Element(XName.op_Implicit("Charged"));
		if (val11 != null)
		{
			ChargedEffectDefinition item5 = new ChargedEffectDefinition((XContainer)(object)val11);
			Statuses.Add(item5);
		}
		XElement val12 = ((XContainer)val).Element(XName.op_Implicit("NegativeStatusImmunity"));
		if (val12 != null)
		{
			ImmuneToNegativeStatusEffectDefinition item6 = new ImmuneToNegativeStatusEffectDefinition((XContainer)(object)val12);
			Statuses.Add(item6);
		}
		XElement val13 = ((XContainer)val).Element(XName.op_Implicit("Contagion"));
		if (val13 != null)
		{
			ContagionEffectDefinition item7 = new ContagionEffectDefinition((XContainer)(object)val13);
			Statuses.Add(item7);
		}
		foreach (XElement item13 in ((XContainer)val).Elements(XName.op_Implicit("RemoveStatus")))
		{
			RemoveStatusDefinition item8 = new RemoveStatusDefinition((XContainer)(object)item13);
			RemoveStatusDefinitions.Add(item8);
		}
	}

	public static string GetFormatedStatModifierInjury(float value, UnitStatDefinition.E_Stat stat, bool statIsPercentage)
	{
		string text = $"{value}";
		if (value >= 0f)
		{
			text = "+" + text;
		}
		text += (statIsPercentage ? "<size=110%>%</size>" : string.Empty);
		text = "<style=" + ((value >= 0f) ? "GoodNb" : "BadNb") + ">" + text + "</style>";
		string arg = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
		UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
		return $"{text} <style={stat2.GetChildStatIfExists()}>{arg}</style>";
	}

	public static string GetFormatedStatMultiplierInjury(E_ValueMultiplier multiplier, UnitStatDefinition.E_Stat stat, float currentLoss, bool statIsPercentage)
	{
		string arg = ((currentLoss != 0f) ? string.Format("(<style=BadNb>{0}{1}</style>)", currentLoss, statIsPercentage ? "<size=110%>%</size>" : string.Empty) : string.Empty);
		string arg2 = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
		UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
		return string.Format(Localizer.Get(string.Format("{0}{1}", "Injury_Multiplier_", multiplier)), $"<style={stat2.GetChildStatIfExists()}>{arg2}</style>", arg);
	}

	public static string GetFormatedPreventedSkillInjury(string skillId)
	{
		return string.Format(Localizer.Get("Injury_PreventSkill_InjuryTooltip"), "<style=Skill>" + Localizer.Get("SkillName_" + skillId) + "</style>");
	}

	public static string GetFormatedStatusInjury(StatusEffectDefinition statusDefinition, bool canShowAsPercentage)
	{
		string result = string.Empty;
		if (statusDefinition is StatModifierEffectDefinition statModifierEffectDefinition)
		{
			UnitStatDefinition.E_Stat stat = statModifierEffectDefinition.Stat;
			result = GetFormatedBuffStatusInjury(statModifierEffectDefinition.ModifierValue, stat, statusDefinition.TurnsCount, statModifierEffectDefinition is BuffEffectDefinition, canShowAsPercentage);
		}
		else if (statusDefinition is PoisonEffectDefinition poisonEffectDefinition)
		{
			result = GetFormatedPoisonStatusInjury(poisonEffectDefinition.TurnsCount, poisonEffectDefinition.DamagePerTurn);
		}
		else if (statusDefinition is StunEffectDefinition)
		{
			result = GetFormatedStunStatusInjury(statusDefinition.TurnsCount);
		}
		else if (statusDefinition is ContagionEffectDefinition)
		{
			result = GetFormatedContagionStatusInjury(statusDefinition.TurnsCount);
		}
		return result;
	}

	public static string GetFormatedBuffStatusInjury(float value, UnitStatDefinition.E_Stat stat, int turnsCount, bool isBuff, bool canShowAsPercentage)
	{
		string empty = string.Empty;
		empty = $"{value}";
		if (value > 0f && isBuff)
		{
			empty = "+" + empty;
		}
		else if (value > 0f)
		{
			empty = "-" + empty;
		}
		empty += ((stat.ShownAsPercentage() && canShowAsPercentage) ? "<size=80%>%</size>" : string.Empty);
		empty = "<style=" + (isBuff ? "GoodNb" : "BadNb") + ">" + empty + "</style>";
		string text = ((stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat)) : string.Empty);
		UnitStatDefinition stat2 = UnitDatabase.UnitStatDefinitions[stat];
		if (turnsCount == -1)
		{
			return $"{empty} <style={stat2.GetChildStatIfExists()}>{text}</style> ({AtlasIcons.TimeIcon} {AtlasIcons.InfiniteIcon})";
		}
		return $"{empty} <style={stat2.GetChildStatIfExists()}>{text}</style> ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	public static string GetFormatedContagionStatusInjury(int turnsCount)
	{
		string text = "<style=Contagion>" + Localizer.Get("SkillEffectName_Contagion") + "</style>";
		if (turnsCount == -1)
		{
			return text + " (" + AtlasIcons.TimeIcon + " " + AtlasIcons.InfiniteIcon + ")";
		}
		return $"{text} ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	public static string GetFormatedPoisonStatusInjury(int turnsCount, float damagePerTurn)
	{
		string text = "<style=Poison>" + Localizer.Get("SkillEffectName_Poison") + "</style>";
		if (turnsCount == -1)
		{
			return $"{text} <color=red>({damagePerTurn})</color> ({AtlasIcons.TimeIcon} {AtlasIcons.InfiniteIcon})";
		}
		return $"{text} <color=red>({damagePerTurn})</color> ({AtlasIcons.TimeIcon} {turnsCount})";
	}

	public static string GetFormatedStunStatusInjury(int turnsCount)
	{
		string text = "<style=Stun>" + Localizer.Get("SkillEffectName_Stun") + "</style>";
		if (turnsCount == -1)
		{
			return text + " (" + AtlasIcons.TimeIcon + " " + AtlasIcons.InfiniteIcon + ")";
		}
		return $"{text} ({AtlasIcons.TimeIcon} {turnsCount})";
	}
}
