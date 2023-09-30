using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeDefinition : Definition
{
	public List<EasyModeModifierDefinition> ModifiersDefinitions { get; private set; } = new List<EasyModeModifierDefinition>();


	public EasyModeDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((XContainer)((container is XElement) ? container : null).Element(XName.op_Implicit("EasyModeModifiers"))).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DecreasePrices":
				ModifiersDefinitions.Add(new EasyModeDecreasePricesDefinition((XContainer)(object)item));
				break;
			case "DecreaseEnemiesCount":
				ModifiersDefinitions.Add(new EasyModeDecreaseEnemiesCountDefinition((XContainer)(object)item));
				break;
			case "IncreaseMagicCircleHealth":
				ModifiersDefinitions.Add(new EasyModeIncreaseMagicCircleHealthDefinition((XContainer)(object)item));
				break;
			case "InitResources":
				ModifiersDefinitions.Add(new EasyModeInitResourcesDefinition((XContainer)(object)item));
				break;
			}
		}
	}
}
