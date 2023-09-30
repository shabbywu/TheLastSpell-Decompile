using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class TargetingMethodsContainerDefinition : Definition
{
	public bool AvoidOverkill { get; private set; }

	public List<TargetingMethodDefinition> TargetingMethods { get; private set; } = new List<TargetingMethodDefinition>();


	public TargetingMethodsContainerDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((container is XElement) ? container : null).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "Closest":
				TargetingMethods.Add(new ClosestTargetingMethodDefinition((XContainer)(object)item));
				break;
			case "Farthest":
				TargetingMethods.Add(new FarthestTargetingMethodDefinition((XContainer)(object)item));
				break;
			case "FirstTarget":
				TargetingMethods.Add(new FirstTargetTargetingMethodDefinition((XContainer)(object)item));
				break;
			case "Optimal":
				TargetingMethods.Add(new OptimalTargetingMethodDefinition((XContainer)(object)item));
				break;
			case "Score":
				TargetingMethods.Add(new ScoreTargetingMethodDefinition((XContainer)(object)item));
				break;
			case "Random":
				TargetingMethods.Add(new RandomTargetingMethodDefinition((XContainer)(object)item));
				break;
			case "AvoidOverkill":
				AvoidOverkill = true;
				break;
			default:
				CLoggerManager.Log((object)("Error, " + item.Name.LocalName + " is not a correct Targeting Method!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
		}
		if (TargetingMethods.Count == 0)
		{
			CLoggerManager.Log((object)"No Targeting Method found!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
