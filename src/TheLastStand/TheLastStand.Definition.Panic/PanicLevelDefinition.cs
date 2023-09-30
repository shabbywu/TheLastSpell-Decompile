using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Panic;

public class PanicLevelDefinition : Definition
{
	public PanicRewardDefinition PanicRewardDefinition { get; private set; }

	public float PanicValueNeeded { get; private set; }

	public PanicLevelDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = obj.Element(XName.op_Implicit("PanicValueNeeded")).Attribute(XName.op_Implicit("Value"));
		if (!int.TryParse(val.Value, out var result))
		{
			Debug.LogError((object)("Invalid PanicValueNeeded " + val.Value));
		}
		PanicValueNeeded = result;
		XElement container2 = obj.Element(XName.op_Implicit("Reward"));
		PanicRewardDefinition = new PanicRewardDefinition((XContainer)(object)container2);
	}
}
