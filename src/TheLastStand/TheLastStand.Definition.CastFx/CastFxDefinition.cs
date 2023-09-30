using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public class CastFxDefinition : Definition
{
	public class CamShakeDefinition : Definition
	{
		public Node Delay { get; private set; }

		public string Id { get; private set; }

		public CamShakeDefinition(XContainer container)
			: base(container, (Dictionary<string, string>)null)
		{
		}

		public override void Deserialize(XContainer container)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			XElement val = container.Element(XName.op_Implicit("Id"));
			if (val != null)
			{
				Id = val.Value;
				XElement val2 = container.Element(XName.op_Implicit("Delay"));
				Delay = (Node)((val2 != null) ? ((object)Parser.Parse(val2.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(0.0)));
			}
			else
			{
				Debug.LogError((object)"CamShakeDefinition needs to define an Id!");
			}
		}
	}

	public List<CamShakeDefinition> CamShakeDefinitions { get; private set; } = new List<CamShakeDefinition>();


	public Node CastTotalDuration { get; private set; } = (Node)new NodeNumber(0.20000000298023224);


	public List<SoundEffectDefinition> SoundEffectDefinitionsOnCast { get; } = new List<SoundEffectDefinition>();


	public List<SoundEffectDefinition> SoundEffectDefinitionsOnImpact { get; } = new List<SoundEffectDefinition>();


	public List<VisualEffectDefinition> VisualEffectDefinitions { get; } = new List<VisualEffectDefinition>();


	public CastFxDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	//IL_001f: Expected O, but got Unknown


	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		if (val == null)
		{
			return;
		}
		XAttribute val2 = val.Attribute(XName.op_Implicit("TotalDuration"));
		if (!XDocumentExtensions.IsNullOrEmpty(val2))
		{
			CastTotalDuration = Parser.Parse(val2.Value, (Dictionary<string, string>)null);
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("VisualEffect")))
		{
			VisualEffectDefinitions.Add(new StandardVisualEffectDefinition((XContainer)(object)item));
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("SoundEffect")))
		{
			XElement val3 = ((XContainer)item2).Element(XName.op_Implicit("OnImpact"));
			if (val3 != null)
			{
				if (bool.TryParse(val3.Value, out var result))
				{
					if (result)
					{
						SoundEffectDefinitionsOnImpact.Add(new SoundEffectDefinition((XContainer)(object)item2));
					}
					else
					{
						SoundEffectDefinitionsOnCast.Add(new SoundEffectDefinition((XContainer)(object)item2));
					}
				}
			}
			else
			{
				SoundEffectDefinitionsOnCast.Add(new SoundEffectDefinition((XContainer)(object)item2));
			}
		}
		CamShakeDefinitions = new List<CamShakeDefinition>();
		foreach (XElement item3 in ((XContainer)val).Elements(XName.op_Implicit("CamShake")))
		{
			CamShakeDefinitions.Add(new CamShakeDefinition((XContainer)(object)item3));
		}
	}
}
