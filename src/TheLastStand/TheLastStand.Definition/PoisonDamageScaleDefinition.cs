using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Skill;
using UnityEngine;

namespace TheLastStand.Definition;

public class PoisonDamageScaleDefinition : Definition
{
	public Dictionary<int, float> MultipliersPerLevel { get; private set; }

	public PoisonDamageScaleDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container.Element(XName.op_Implicit("PoisonDamageScaleDefinition"));
		MultipliersPerLevel = new Dictionary<int, float>();
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("Multiplier")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Level"));
			if (!int.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Invalid int value " + val.Value + " for a PoisonDamageMultiplier element."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			if (!float.TryParse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				CLoggerManager.Log((object)("Invalid float value " + item.Value + " for a PoisonDamageMultiplier element."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			MultipliersPerLevel.Add(result, result2);
		}
	}

	public float GetMultiplierAtLevel(int level)
	{
		if (MultipliersPerLevel.TryGetValue(level, out var value))
		{
			return value;
		}
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogWarning((object)$"PoisonDamageScaleDefinition does not define {level} multiplier. Getting the largest multiplier below level {level} instead.", (CLogLevel)1, true, false);
		for (int num = level - 1; num > -1; num--)
		{
			if (MultipliersPerLevel.TryGetValue(num, out value))
			{
				return value;
			}
		}
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogWarning((object)"PoisonDamageScaleDefinition seem to define no multiplier, returning 1.", (CLogLevel)1, true, false);
		return 1f;
	}
}
