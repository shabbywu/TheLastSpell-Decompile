using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.Maths;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class IdsListDefinition : Definition, ITopologicSortItem<IdsListDefinition>
{
	private XElement idsListElement;

	public string Id { get; private set; }

	public List<string> Ids { get; private set; } = new List<string>();


	public List<string> IncludedListsIds { get; private set; } = new List<string>();


	public IdsListDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		idsListElement = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val = idsListElement.Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		foreach (XElement item in ((XContainer)idsListElement).Elements(XName.op_Implicit("IncludeList")))
		{
			IncludedListsIds.Add(item.Value);
		}
	}

	public void DeserializeAfterDependencySorting()
	{
		foreach (XElement item in ((XContainer)idsListElement).Elements(XName.op_Implicit("IncludeList")))
		{
			foreach (string id in GenericDatabase.IdsListDefinitions[item.Value].Ids)
			{
				if (!Ids.Contains(id))
				{
					Ids.Add(id);
				}
			}
		}
		foreach (XElement item2 in ((XContainer)idsListElement).Elements(XName.op_Implicit("Id")))
		{
			if (!Ids.Contains(item2.Value))
			{
				Ids.Add(item2.Value);
			}
		}
	}

	public IEnumerable<IdsListDefinition> GetDependencies()
	{
		return GenericDatabase.IdsListDefinitions.Values.Where((IdsListDefinition o) => IncludedListsIds.Contains(o.Id));
	}

	public override string ToString()
	{
		return ((object)this).GetType().Name + " Id=" + Id;
	}
}
