using System;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class RegenStatSkillEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "RegenStat";
	}

	public float Bonus { get; private set; }

	public override string Id => "RegenStat";

	public string StatDescription => UnitDatabase.UnitStatDefinitions[Stat].Description;

	public string StatName => UnitDatabase.UnitStatDefinitions[Stat].Name;

	public UnitStatDefinition.E_Stat Stat { get; private set; } = UnitStatDefinition.E_Stat.Undefined;


	public RegenStatSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Bonus = float.Parse(((XContainer)val).Element(XName.op_Implicit("Bonus")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		if (Enum.TryParse<UnitStatDefinition.E_Stat>(((XContainer)val).Element(XName.op_Implicit("Stat")).Attribute(XName.op_Implicit("Id")).Value, out var result))
		{
			Stat = result;
		}
		else
		{
			CLoggerManager.Log((object)$"Unknown stat {result}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
