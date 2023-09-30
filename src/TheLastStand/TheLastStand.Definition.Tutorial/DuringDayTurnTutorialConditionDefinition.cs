using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Tutorial;

public class DuringDayTurnTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "DuringDayTurn";
	}

	public Game.E_DayTurn DayTurn { get; private set; }

	public DuringDayTurnTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("DayTurn"));
		if (!Enum.TryParse<Game.E_DayTurn>(val.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse DuringDayTurnTutorialConditionDefinition DayTurn value " + val.Value + " to a valid DayTurn!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			DayTurn = result;
		}
	}
}
