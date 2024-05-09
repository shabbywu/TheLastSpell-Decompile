using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Item.ItemRestriction;

public class ItemRestrictionCategoriesCollectionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<ItemRestrictionCategoryDefinition> itemCategoryDefinitions { get; private set; }

	public string Id { get; set; }

	public ItemRestrictionCategoriesCollectionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		itemCategoryDefinitions = new List<ItemRestrictionCategoryDefinition>();
		foreach (XElement item2 in obj.Elements(XName.op_Implicit("ItemRestrictionCategoryDefinition")))
		{
			ItemRestrictionCategoryDefinition item = new ItemRestrictionCategoryDefinition((XContainer)(object)item2);
			itemCategoryDefinitions.Add(item);
		}
	}
}
