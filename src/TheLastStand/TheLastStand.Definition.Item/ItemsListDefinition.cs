using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.DLC;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class ItemsListDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string DLCId { get; private set; }

	public string Id { get; private set; }

	public bool IsEmpty => ItemsWithOdd.Count == 0;

	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	public Dictionary<string, int> ItemsWithOdd { get; } = new Dictionary<string, int>();


	public ItemsListDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"xItemCategoriesListDefinition must have a valid Id", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("DLCId"));
		if (!val3.IsNullOrEmpty())
		{
			DLCId = val3.Value;
		}
		if (IsLinkedToDLC && !TPSingleton<DLCManager>.Instance.IsDLCOwned(DLCId))
		{
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Item")))
		{
			XAttribute val4 = item.Attribute(XName.op_Implicit("Odd"));
			if (val4.IsNullOrEmpty() || !int.TryParse(val4.Value, out var result))
			{
				CLoggerManager.Log((object)(Id + " Invalid odd!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			XAttribute val5 = item.Attribute(XName.op_Implicit("Id"));
			if (val5.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)(Id + " Invalid category!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				ItemsWithOdd.Add(val5.Value, result);
			}
		}
	}
}
