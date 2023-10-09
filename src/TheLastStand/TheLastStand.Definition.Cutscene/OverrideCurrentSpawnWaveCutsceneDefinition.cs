using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class OverrideCurrentSpawnWaveCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "OverrideCurrentSpawnWave";
	}

	public string WaveId;

	public string DirectionsId;

	public OverrideCurrentSpawnWaveCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("WaveId"));
		WaveId = ((val != null) ? val.Value : null);
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("DirectionsId"));
		DirectionsId = ((val2 != null) ? val2.Value : null);
	}
}
