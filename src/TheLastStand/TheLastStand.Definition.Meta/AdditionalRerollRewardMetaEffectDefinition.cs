using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class AdditionalRerollRewardMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "AdditionalRerollReward";

	public int RerollReward { get; private set; }

	public AdditionalRerollRewardMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (int.TryParse(val.Value, out var result))
		{
			RerollReward = result;
		}
		else
		{
			Debug.LogError((object)("Could not parse value " + val.Value + " to an integer!"));
		}
	}
}
