using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingSlotEffect;

public abstract class BuildingSlotEffectDefinition : Definition
{
	public string Id { get; private set; }

	public int ManaCost { get; set; }

	public BuildingSlotEffectDefinition(string id, XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
		Id = id;
	}

	public virtual BuildingSlotEffectDefinition Clone()
	{
		return ((object)this).MemberwiseClone() as BuildingSlotEffectDefinition;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
