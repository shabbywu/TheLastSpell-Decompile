using System.Xml.Linq;
using TPLib;
using UnityEngine;

namespace TheLastStand.Definition.Skill;

public class OnlyDuringPhaseConditionDefinition : SkillConditionDefinition
{
	public const string OnlyDuringPhaseName = "OnlyDuringPhase";

	public bool DuringDeployment { get; set; }

	public bool DuringNight { get; set; }

	public bool DuringProduction { get; set; }

	public override string Name => "OnlyDuringPhase";

	public OnlyDuringPhaseConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((container is XElement) ? container : null).Elements())
		{
			if (item.Name == XName.op_Implicit("Night"))
			{
				DuringNight = true;
			}
			else if (item.Name == XName.op_Implicit("Production"))
			{
				DuringProduction = true;
			}
			else if (item.Name == XName.op_Implicit("Deployment"))
			{
				DuringDeployment = true;
			}
			else
			{
				TPDebug.LogError((object)$"{item.Name} is not a valid phase name", (Object)null);
			}
		}
	}
}
