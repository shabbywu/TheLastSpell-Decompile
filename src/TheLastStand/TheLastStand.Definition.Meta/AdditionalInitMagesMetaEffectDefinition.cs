using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class AdditionalInitMagesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "AdditionalInitMages";

	public int Amount { get; private set; }

	public AdditionalInitMagesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (int.TryParse(val.Value, out var result))
		{
			Amount = result;
		}
		else
		{
			Debug.LogError((object)("Could not parse value " + val.Value + " to an integer!"));
		}
	}
}
