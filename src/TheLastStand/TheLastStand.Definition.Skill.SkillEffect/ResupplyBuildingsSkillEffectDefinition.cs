using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public abstract class ResupplyBuildingsSkillEffectDefinition : SkillEffectDefinition
{
	public int Amount { get; private set; }

	public List<string> TargetIds { get; private set; } = new List<string>();


	public ResupplyBuildingsSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Targets"));
		XElement val2 = obj.Element(XName.op_Implicit("Amount"));
		if (val != null)
		{
			foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("TargetsListId")))
			{
				XAttribute val3 = item.Attribute(XName.op_Implicit("Value"));
				foreach (string id in GenericDatabase.IdsListDefinitions[val3.Value].Ids)
				{
					if (!TargetIds.Contains(id))
					{
						TargetIds.Add(id);
					}
				}
			}
			foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("TargetId")))
			{
				XAttribute val4 = item2.Attribute(XName.op_Implicit("Value"));
				if (!TargetIds.Contains(val4.Value))
				{
					TargetIds.Add(val4.Value);
				}
			}
		}
		if (val2 != null)
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)"An Amount of a SkillEffect ResupplyBuildings(ResupplyOverallUses or ResupplyCharges) isn't a valid integer !", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				Amount = result;
			}
		}
		else
		{
			CLoggerManager.Log((object)"A SkillEffect ResupplyBuildings(ResupplyOverallUses or ResupplyCharges) needs an Element Amount !", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
