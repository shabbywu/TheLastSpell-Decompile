using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Brazier;

public class BraziersDefinition : Definition
{
	public class GuardiansGroup
	{
		public string Id;

		public List<Tuple<string, int>> GuardiansPerWeight;
	}

	private static class Constants
	{
		public const string BrazierDefinitionElement = "BrazierDefinition";
	}

	public Dictionary<string, BrazierDefinition> BrazierDefinitions { get; private set; }

	public static Dictionary<string, GuardiansGroup> GuardiansGroups { get; private set; }

	public BraziersDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		GuardiansGroups = new Dictionary<string, GuardiansGroup>();
		foreach (XElement item2 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("GuardiansGroups"))).Elements(XName.op_Implicit("GuardiansGroup")))
		{
			XAttribute val2 = item2.Attribute(XName.op_Implicit("Id"));
			List<Tuple<string, int>> list = new List<Tuple<string, int>>();
			foreach (XElement item3 in ((XContainer)item2).Elements(XName.op_Implicit("Guardian")))
			{
				XAttribute val3 = item3.Attribute(XName.op_Implicit("Id"));
				XAttribute val4 = item3.Attribute(XName.op_Implicit("Weight"));
				int item = ((val4 == null) ? 1 : int.Parse(val4.Value));
				list.Add(new Tuple<string, int>(val3.Value, item));
			}
			GuardiansGroups.Add(val2.Value, new GuardiansGroup
			{
				Id = val2.Value,
				GuardiansPerWeight = list
			});
		}
		BrazierDefinitions = new Dictionary<string, BrazierDefinition>();
		foreach (XElement item4 in ((XContainer)val).Elements(XName.op_Implicit("BrazierDefinition")))
		{
			BrazierDefinition brazierDefinition = new BrazierDefinition((XContainer)(object)item4);
			BrazierDefinitions.Add(brazierDefinition.Id, brazierDefinition);
		}
	}
}
