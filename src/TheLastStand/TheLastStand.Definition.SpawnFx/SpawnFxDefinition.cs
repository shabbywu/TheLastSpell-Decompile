using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.SpawnFx;

public class SpawnFxDefinition : Definition
{
	public List<CastFxDefinition.CamShakeDefinition> CamShakeDefinitions { get; private set; } = new List<CastFxDefinition.CamShakeDefinition>();


	public Node CastTotalDuration { get; private set; } = (Node)new NodeNumber(0.20000000298023224);


	public List<SoundEffectDefinition> SoundEffectDefinitions { get; private set; } = new List<SoundEffectDefinition>();


	public List<SpawnVisualEffectDefinition> SpawnVisualEffectDefinition { get; private set; } = new List<SpawnVisualEffectDefinition>();


	public SpawnFxDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	//IL_001f: Expected O, but got Unknown


	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("TotalDuration"));
		if (!XDocumentExtensions.IsNullOrEmpty(val2))
		{
			CastTotalDuration = Parser.Parse(val2.Value, (Dictionary<string, string>)null);
		}
		SpawnVisualEffectDefinition = new List<SpawnVisualEffectDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("SpawnVisualEffect")))
		{
			if (item != null)
			{
				SpawnVisualEffectDefinition.Add(new SpawnVisualEffectDefinition((XContainer)(object)item));
			}
		}
		SoundEffectDefinitions = new List<SoundEffectDefinition>();
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("SoundEffect")))
		{
			if (item2 != null)
			{
				SoundEffectDefinitions.Add(new SoundEffectDefinition((XContainer)(object)item2));
			}
		}
		CamShakeDefinitions = new List<CastFxDefinition.CamShakeDefinition>();
		foreach (XElement item3 in ((XContainer)val).Elements(XName.op_Implicit("CamShake")))
		{
			if (item3 != null)
			{
				CamShakeDefinitions.Add(new CastFxDefinition.CamShakeDefinition((XContainer)(object)item3));
			}
		}
	}
}
