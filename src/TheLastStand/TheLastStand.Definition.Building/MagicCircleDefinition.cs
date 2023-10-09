using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class MagicCircleDefinition : BuildingDefinition
{
	public int MageCountInit { get; set; }

	public int MageSlotInit { get; set; }

	public int MageSlotMax { get; set; }

	public int OpenSealsInit { get; set; }

	public int SealsToClose { get; set; }

	public MagicCircleDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("MagicCircleSettings"));
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("MageSlotMax"));
		if (val2.IsNullOrEmpty())
		{
			Debug.Log((object)"ConstructionDefinition must have a MageSlotMax");
			return;
		}
		if (!int.TryParse(val2.Value, out var result))
		{
			Debug.Log((object)"MagicCircle MageSlotMax must be a valid int");
			return;
		}
		MageSlotMax = result;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("MageSlotInit"));
		if (val3.IsNullOrEmpty())
		{
			Debug.Log((object)"ConstructionDefinition must have a MageSlotInit");
			return;
		}
		if (!int.TryParse(val3.Value, out var result2))
		{
			Debug.Log((object)"MagicCircle MageSlotInit must be a valid int");
			return;
		}
		MageSlotInit = result2;
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("MageCountInit"));
		if (val4.IsNullOrEmpty())
		{
			Debug.Log((object)"MagicCircle must have a MageCountInit");
			return;
		}
		if (!int.TryParse(val4.Value, out var result3))
		{
			Debug.Log((object)"MagicCircle MageCountInit must be a valid int");
			return;
		}
		MageCountInit = result3;
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("OpenSealsInit"));
		if (val5.IsNullOrEmpty())
		{
			Debug.Log((object)"MagicCircle must have a OpenSealsInit");
			return;
		}
		if (!int.TryParse(val5.Value, out var result4))
		{
			Debug.Log((object)"MagicCircle OpenSealsInit must be a valid int");
			return;
		}
		OpenSealsInit = result4;
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("SealsToClose"));
		int result5;
		if (val6.IsNullOrEmpty())
		{
			Debug.Log((object)"ConstructionDefinition must have a SealsToClose");
		}
		else if (!int.TryParse(val6.Value, out result5))
		{
			Debug.Log((object)"MagicCircle SealsToClose must be a valid int");
		}
		else
		{
			SealsToClose = result5;
		}
	}
}
