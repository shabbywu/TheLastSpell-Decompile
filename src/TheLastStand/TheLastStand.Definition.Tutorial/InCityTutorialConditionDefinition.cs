using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Tutorial;

public class InCityTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "InCity";
	}

	public string CityId { get; private set; }

	public InCityTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("CityId"));
		CityId = val.Value;
	}
}
