using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public abstract class BuildingGaugeEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int FirstGaugeUnits { get; private set; }

	public int GaugeUnitsIncrease { get; private set; }

	public string Id { get; private set; }

	public string ProductionBoxId { get; private set; }

	public bool TriggeredOnConstruction { get; set; }

	public BuildingGaugeEffectDefinition(string id, XContainer container)
		: base(container)
	{
		Id = id;
	}

	public virtual BuildingGaugeEffectDefinition Clone()
	{
		return MemberwiseClone() as BuildingGaugeEffectDefinition;
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement obj2 = obj.Element(XName.op_Implicit("ProductionBoxId"));
		if (obj2 == null)
		{
			Debug.LogError((object)"BuildingGaugeEffectDefinition must have ProductionBoxId");
		}
		XAttribute val = obj2.Attribute(XName.op_Implicit("Value"));
		if (val.IsNullOrEmpty())
		{
			Debug.LogError((object)"ProductionBoxId must have Value");
		}
		ProductionBoxId = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("FirstGauge"));
		if (val2 != null)
		{
			FirstGaugeUnits = int.Parse(val2.Value);
		}
		XElement val3 = obj.Element(XName.op_Implicit("GaugeIncrease"));
		if (val3 != null)
		{
			GaugeUnitsIncrease = int.Parse(val3.Value);
		}
	}
}
