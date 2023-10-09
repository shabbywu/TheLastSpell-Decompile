using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BattleModuleDefinition : BuildingModuleDefinition
{
	public BehaviorDefinition Behavior { get; private set; }

	public int MaximumTrapCharges { get; private set; }

	public Dictionary<string, int> Skills { get; } = new Dictionary<string, int>();


	public List<SkillProgression> SkillProgressions { get; protected set; } = new List<SkillProgression>();


	public BattleModuleDefinition(BuildingDefinition buildingDefinition, XContainer battleDefinition)
		: base(buildingDefinition, battleDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Skills"));
		if (val2 != null)
		{
			foreach (XElement item in ((XContainer)val2).Elements())
			{
				int result = -1;
				XAttribute val3 = item.Attribute(XName.op_Implicit("NightUsesCount"));
				if (val3 != null)
				{
					if (!int.TryParse(val3.Value, out result))
					{
						TPDebug.LogError((object)("The skill " + item.Value + " " + HasAnInvalidInt(val3.Value)), (Object)null);
						continue;
					}
					if (result == -1)
					{
						CLoggerManager.Log((object)("Night uses count has a value of -1 for skill " + item.Value + "!"), (LogType)2, (CLogLevel)2, true, "StaticLog", false);
					}
				}
				Skills.Add(item.Value, result);
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Behavior"));
		if (val4 == null)
		{
			Behavior = null;
		}
		else
		{
			Behavior = new BehaviorDefinition((XContainer)(object)val4);
		}
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("MaximumTrapUsage"));
		string text = ((obj != null) ? obj.Value : null);
		if (text != null)
		{
			if (int.TryParse(text, out var result2))
			{
				MaximumTrapCharges = result2;
			}
			else
			{
				CLoggerManager.Log((object)("A Trap named : " + BuildingDefinition.Id + " has an invalid value for Element : MaximumTrapUsage. The value should be of type integer ! (Current value : " + text + ")"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XElement obj2 = ((XContainer)val).Element(XName.op_Implicit("SkillProgressions"));
		IEnumerable<XElement> enumerable = ((obj2 != null) ? ((XContainer)obj2).Elements(XName.op_Implicit("SkillProgression")) : null);
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item2 in enumerable)
		{
			SkillProgressions.Add(SkillProgression.Deserialize(item2));
		}
	}
}
