using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public abstract class VisualEffectDefinition : Definition
{
	public enum E_Depth
	{
		Before,
		Behind,
		Dynamic,
		TargetDynamic,
		AboveAll
	}

	protected Dictionary<int, string> paths = new Dictionary<int, string>(4);

	public E_Depth SortingDepth { get; private set; }

	public Node Delay { get; private set; }

	public VisualEffectDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		XElement val = container.Element(XName.op_Implicit("Paths"));
		if (!XDocumentExtensions.IsNullOrEmpty(val))
		{
			foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Path")))
			{
				if (!XDocumentExtensions.IsNullOrEmpty(item))
				{
					XAttribute val2 = item.Attribute(XName.op_Implicit("Orientation"));
					string value = item.Value;
					if (XDocumentExtensions.IsNullOrEmpty(val2))
					{
						if (value.EndsWith("_", StringComparison.OrdinalIgnoreCase))
						{
							paths[2] = value + "E";
							paths[0] = value + "N";
							paths[1] = value + "S";
							paths[3] = value + "W";
						}
						else
						{
							paths[2] = value;
							paths[0] = value;
							paths[1] = value;
							paths[3] = value;
						}
						continue;
					}
					string value2 = val2.Value;
					int i = 0;
					for (int length = value2.Length; i < length; i++)
					{
						switch (value2[i])
						{
						case 'E':
							paths[2] = value;
							break;
						case 'N':
							paths[0] = value;
							break;
						case 'S':
							paths[1] = value;
							break;
						case 'W':
							paths[3] = value;
							break;
						default:
							Debug.LogError((object)$"VisualEffectDefinition (Path='{value}') defines an invalid orientation (char '{value2[i]}')!");
							break;
						}
					}
					continue;
				}
				Debug.LogError((object)"VisualEffectDefinition has an invalid Path entry!");
				return;
			}
			XElement val3 = container.Element(XName.op_Implicit("Delay"));
			Delay = (Node)((val3 != null) ? ((object)Parser.Parse(val3.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(0.0)));
			XElement val4 = container.Element(XName.op_Implicit("SortingDepth"));
			if (val4 != null)
			{
				if (!Enum.TryParse<E_Depth>(val4.Value, out var result))
				{
					Debug.LogError((object)("VisualEffectDefinition (Path='" + paths[0] + "') defines an invalid SortingDepth (" + val4.Value + ")!"));
				}
				else
				{
					SortingDepth = result;
				}
			}
			else
			{
				Debug.LogError((object)("VisualEffectDefinition (Path='" + paths[0] + "') must define a SortingDepth!"));
			}
		}
		else
		{
			Debug.LogError((object)"VisualEffectDefinition must define at least one Path!");
		}
	}

	public string GetPath(GameDefinition.E_Direction direction)
	{
		if (paths.TryGetValue((int)direction, out var value))
		{
			return value;
		}
		return null;
	}
}
