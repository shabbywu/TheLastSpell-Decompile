using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitEquipmentSlotDefinition : TheLastStand.Framework.Serialization.Definition
{
	public ItemSlotDefinition.E_ItemSlotId Id { get; set; }

	public int Min { get; set; }

	public int Max { get; set; }

	public int Base { get; set; }

	public UnitEquipmentSlotDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(val2.Value, out var result))
		{
			CLoggerManager.Log((object)("An UnitEquipmentSlotDefinition has an invalid Id " + val2.Value + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		Id = result;
		XAttribute val3 = val.Attribute(XName.op_Implicit("Min"));
		int result2;
		if (val3.IsNullOrEmpty())
		{
			Debug.LogError((object)"UniEquipmentSlotDefinition must have a Min");
		}
		else if (int.TryParse(val3.Value, out result2))
		{
			Min = result2;
			XAttribute val4 = val.Attribute(XName.op_Implicit("Max"));
			int result3;
			if (val4.IsNullOrEmpty())
			{
				Debug.LogError((object)"UniEquipmentSlotDefinition must have a Max");
			}
			else if (int.TryParse(val4.Value, out result3))
			{
				Max = result3;
				XAttribute val5 = val.Attribute(XName.op_Implicit("Base"));
				int result4;
				if (val5.IsNullOrEmpty())
				{
					Debug.LogError((object)"UniEquipmentSlotDefinition must have a Base");
				}
				else if (int.TryParse(val5.Value, out result4))
				{
					Base = result4;
				}
				else
				{
					Debug.LogError((object)"UniEquipmentSlotDefinition Base must have a valid integer");
				}
			}
			else
			{
				Debug.LogError((object)"UniEquipmentSlotDefinition Max must have a valid integer");
			}
		}
		else
		{
			Debug.LogError((object)"UniEquipmentSlotDefinition Min must have a valid integer");
		}
	}
}
