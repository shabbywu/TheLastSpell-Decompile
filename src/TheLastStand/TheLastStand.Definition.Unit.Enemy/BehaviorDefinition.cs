using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class BehaviorDefinition : Definition
{
	public GoalDefinition[] GoalDefinitions { get; private set; }

	public int GoalsComputingOrder { get; private set; }

	public PathfindingDefinition.E_PathfindingStyle PathfindingStyle { get; private set; }

	public int TurnsToSkipOnSpawn { get; private set; }

	public int ThinkingScope { get; private set; }

	public int NumberOfGoalsToExecute { get; private set; }

	public BehaviorDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("PathfindingStyle"));
		PathfindingDefinition.E_PathfindingStyle result = PathfindingDefinition.E_PathfindingStyle.Hypotenuse;
		if (val2 != null && !Enum.TryParse<PathfindingDefinition.E_PathfindingStyle>(val2.Value, out result))
		{
			Debug.LogError((object)"Invalid PathfindingStyle");
		}
		PathfindingStyle = result;
		ThinkingScope = int.Parse(((XContainer)val).Element(XName.op_Implicit("ThinkingScope")).Value);
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("GoalsComputingOrder"));
		if (val3 != null)
		{
			XAttribute val4 = val3.Attribute(XName.op_Implicit("Value"));
			GoalsComputingOrder = int.Parse(val4.Value);
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Goals"));
		int num = 0;
		foreach (XElement item in ((XContainer)val5).Elements(XName.op_Implicit("Goal")))
		{
			_ = item;
			num++;
		}
		GoalDefinitions = new GoalDefinition[num];
		int num2 = 0;
		foreach (XElement item2 in ((XContainer)val5).Elements(XName.op_Implicit("Goal")))
		{
			GoalDefinition goalDefinition = new GoalDefinition((XContainer)(object)item2);
			GoalDefinitions[num2++] = goalDefinition;
		}
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("NumberOfGoalsToExecute"));
		if (val6 != null)
		{
			if (!int.TryParse(val6.Value, out var result2))
			{
				Debug.LogError((object)"NumberOfGoalsToExecute should have a value of type int !");
			}
			NumberOfGoalsToExecute = result2;
		}
		XElement val7 = ((XContainer)val).Element(XName.op_Implicit("TurnsToSkipOnSpawn"));
		if (val7 != null)
		{
			if (!int.TryParse(val7.Attribute(XName.op_Implicit("Value")).Value, out var result3))
			{
				CLoggerManager.Log((object)"Could not parse Value attribute in TurnsToSkipOnSpawn element in BehaviorDefinition into an int.", (LogType)0, (CLogLevel)2, true, "BehaviorDefinition", false);
			}
			TurnsToSkipOnSpawn = result3;
		}
	}
}
