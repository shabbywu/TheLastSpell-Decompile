using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class InstantiateParticlesCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "InstantiateParticles";
	}

	public string ParticlesId { get; private set; }

	public string ParticlesFolder { get; private set; }

	public string ParticlesPath { get; private set; }

	public InstantiateParticlesCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Folder"));
		ParticlesFolder = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("Id"));
		ParticlesId = val2.Value;
		ParticlesPath = Path.Combine(ParticlesFolder, ParticlesId);
	}
}
