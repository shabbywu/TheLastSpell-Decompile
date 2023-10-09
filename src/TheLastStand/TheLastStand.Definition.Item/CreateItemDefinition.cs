using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class CreateItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static int All => -1;

	public Node Count { get; private set; }

	public bool HasID => !string.IsNullOrEmpty(Id);

	public string Id { get; private set; }

	public string LevelModifierListId { get; private set; }

	public ItemsListDefinition ItemsListDefinition { get; private set; }

	public ProbabilityTreeEntriesDefinition ItemRaritiesListDefinition { get; private set; }

	public CreateItemDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute obj = val.Attribute(XName.op_Implicit("Id"));
		Id = ((obj != null) ? obj.Value : null);
		XAttribute val2 = ((XContainer)val).Element(XName.op_Implicit("ItemsList")).Attribute(XName.op_Implicit("Id"));
		if (!ItemDatabase.ItemsListDefinitions.TryGetValue(val2.Value, out var value))
		{
			CLoggerManager.Log((object)(val2.Value + " items list not found!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		ItemsListDefinition = value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("BuildingLevelModifiersList"));
		if (val3 != null)
		{
			XAttribute obj2 = val3.Attribute(XName.op_Implicit("Id"));
			LevelModifierListId = ((obj2 != null) ? obj2.Value : null);
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("ItemRaritiesList"));
		if (val4 == null)
		{
			CLoggerManager.Log((object)"CreateItem levels missing!", (LogType)1, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XAttribute val5 = val4.Attribute(XName.op_Implicit("Id"));
		if (val5.IsNullOrEmpty() || !ItemDatabase.ItemRaritiesListDefinitions.ContainsKey(val5.Value))
		{
			CLoggerManager.Log((object)"CreateItem ItemRarities Id is not valid or does not exist in ItemRaritiesListDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!ItemDatabase.ItemRaritiesListDefinitions.TryGetValue(val5.Value, out var value2))
		{
			CLoggerManager.Log((object)(val5.Value + " items rarities list not found!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		ItemRaritiesListDefinition = value2;
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("Count"));
		Count = (val6.IsNullOrEmpty() ? Parser.Parse("1") : Parser.Parse(val6.Value));
	}
}
