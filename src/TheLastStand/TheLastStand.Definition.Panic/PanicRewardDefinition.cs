using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Panic;

public class PanicRewardDefinition : Definition
{
	public class DayGenerationDatas
	{
		public int BaseGenerationLevel { get; set; }

		public string ItemGenerationModifiersListId { get; set; }

		public string ItemsListId { get; set; }

		public string ItemRaritiesListId { get; set; }
	}

	public Node Gold { get; private set; }

	public Dictionary<int, DayGenerationDatas> ItemsListsPerDay { get; private set; }

	public Dictionary<int, int> ItemsListsTotalWeightPerDay { get; private set; }

	public Node Materials { get; private set; }

	public PanicRewardDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = obj.Element(XName.op_Implicit("Gold")).Attribute(XName.op_Implicit("Value"));
		Gold = Parser.Parse(val.Value, (Dictionary<string, string>)null);
		XAttribute val2 = obj.Element(XName.op_Implicit("Materials")).Attribute(XName.op_Implicit("Value"));
		Materials = Parser.Parse(val2.Value, (Dictionary<string, string>)null);
		XElement val3 = obj.Element(XName.op_Implicit("ItemsLists"));
		if (val3 == null)
		{
			return;
		}
		ItemsListsPerDay = new Dictionary<int, DayGenerationDatas>();
		ItemsListsTotalWeightPerDay = new Dictionary<int, int>();
		foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("Reward")))
		{
			XAttribute val4 = item.Attribute(XName.op_Implicit("Index"));
			if (!int.TryParse(val4.Value, out var result))
			{
				Debug.LogError((object)("Invalid StartingDay " + val4.Value));
			}
			XElement val5 = ((XContainer)item).Element(XName.op_Implicit("BaseLevel"));
			if (!int.TryParse(val5.Value, out var result2))
			{
				Debug.LogError((object)("Invalid BaseGenerationLevel " + val5.Value));
			}
			string value = ((XContainer)item).Element(XName.op_Implicit("ItemRaritiesList")).Attribute(XName.op_Implicit("Id")).Value;
			string value2 = ((XContainer)item).Element(XName.op_Implicit("ItemGenerationModifiersList")).Attribute(XName.op_Implicit("Id")).Value;
			string value3 = ((XContainer)item).Element(XName.op_Implicit("ItemsList")).Attribute(XName.op_Implicit("Id")).Value;
			DayGenerationDatas value4 = new DayGenerationDatas
			{
				BaseGenerationLevel = result2,
				ItemsListId = value3,
				ItemGenerationModifiersListId = value2,
				ItemRaritiesListId = value
			};
			ItemsListsPerDay.Add(result, value4);
		}
	}
}
