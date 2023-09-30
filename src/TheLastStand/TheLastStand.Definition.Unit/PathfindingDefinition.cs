using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class PathfindingDefinition : Definition
{
	public enum E_PathfindingStyle
	{
		Undefined,
		Manhattan,
		Hypotenuse,
		Bresenham
	}

	public float EnemyAISpreadFactor { get; private set; }

	public float NodeWeightFogMultiplier { get; private set; }

	public PathfindingDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		if (!float.TryParse(obj.Element(XName.op_Implicit("EnemyAISpreadFactor")).Attribute(XName.op_Implicit("Value")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Debug.LogError((object)"Invalid EnemyAISpreadFactor Value");
		}
		EnemyAISpreadFactor = result;
		if (!float.TryParse(obj.Element(XName.op_Implicit("NodeWeightFogMultiplier")).Attribute(XName.op_Implicit("Value")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
		{
			Debug.LogError((object)"Invalid NodeWeightFogMultiplier Value");
		}
		NodeWeightFogMultiplier = result2;
	}
}
