using System;
using System.Xml.Linq;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.SpawnFx;

public class SpawnVisualEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Path { get; private set; }

	public Node Delay { get; private set; }

	public VisualEffectDefinition.E_Depth SortingDepth { get; private set; }

	public SpawnVisualEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Path"));
		if (!val2.IsNullOrEmpty())
		{
			Path = val2.Value;
			XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Delay"));
			Delay = ((val3 != null) ? Parser.Parse(val3.Value) : new NodeNumber(0.0));
			XElement val4 = ((XContainer)val).Element(XName.op_Implicit("SortingDepth"));
			if (val4 != null && Enum.TryParse<VisualEffectDefinition.E_Depth>(val4.Value, out var result))
			{
				SortingDepth = result;
			}
		}
		else
		{
			Debug.LogError((object)"SpawnVisualEffectDefinition must define at least one Path!");
		}
	}
}
