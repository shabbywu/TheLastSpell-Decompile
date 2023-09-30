using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossPhasesDefinition : Definition
{
	public Dictionary<string, BossPhaseDefinition> BossPhaseDefinitions { get; } = new Dictionary<string, BossPhaseDefinition>();


	public string Id { get; private set; }

	public BossPhasesDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"BossPhasesDefinition has no Id!");
			return;
		}
		Id = val2.Value;
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Phase")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Id"));
			BossPhaseDefinitions[val3.Value] = new BossPhaseDefinition((XContainer)(object)item, val3.Value);
		}
	}
}
