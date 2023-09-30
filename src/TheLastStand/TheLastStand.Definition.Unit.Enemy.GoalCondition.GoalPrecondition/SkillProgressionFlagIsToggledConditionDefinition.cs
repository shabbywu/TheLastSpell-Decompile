using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class SkillProgressionFlagIsToggledConditionDefinition : GoalConditionDefinition
{
	public const string Name = "SkillProgressionFlagIsToggled";

	public GlyphManager.E_SkillProgressionFlag SkillProgressionFlag { get; private set; }

	public SkillProgressionFlagIsToggledConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Flag"));
		if (!Enum.TryParse<GlyphManager.E_SkillProgressionFlag>(val.Value, out var result))
		{
			CLoggerManager.Log((object)("SkillProgressionFlagIsToggled Unable to parse " + val.Value + " (" + val.Value + ") into a E_SkillProgressionFlag"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		SkillProgressionFlag = result;
	}
}
