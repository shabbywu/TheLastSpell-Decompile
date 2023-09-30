using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.SpawnFx;

public class SpawnVisualEffectDefinition : Definition
{
	public string Path { get; private set; }

	public Node Delay { get; private set; }

	public VisualEffectDefinition.E_Depth SortingDepth { get; private set; }

	public SpawnVisualEffectDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Path"));
		if (!XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Path = val2.Value;
			XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Delay"));
			Delay = (Node)((val3 != null) ? ((object)Parser.Parse(val3.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(0.0)));
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
