using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseCondition;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossPhaseDefinition : Definition
{
	public string Id { get; }

	public Dictionary<string, ActorDefinition> ActorDefinitions { get; } = new Dictionary<string, ActorDefinition>();


	public List<BossPhaseHandlerDefinition> BossPhaseHandlerDefinitions { get; } = new List<BossPhaseHandlerDefinition>();


	public List<IBossPhaseConditionDefinition> DefeatConditionsDefinitions { get; } = new List<IBossPhaseConditionDefinition>();


	public List<IBossPhaseConditionDefinition> VictoryConditionsDefinitions { get; } = new List<IBossPhaseConditionDefinition>();


	public BossPhaseDefinition(XContainer container, string id)
		: base(container, (Dictionary<string, string>)null)
	{
		Id = id;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("ActorsList"));
		if (val2 != null)
		{
			foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("Actor")))
			{
				ActorDefinition actorDefinition = new ActorDefinition((XContainer)(object)item);
				ActorDefinitions.Add(actorDefinition.ActorId, actorDefinition);
			}
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("VictoryConditions"));
		if (val3 != null)
		{
			foreach (XElement item2 in ((XContainer)val3).Elements())
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionDefinitionFromXElement(item2, out var bossPhaseContentDefinition))
				{
					VictoryConditionsDefinitions.Add(bossPhaseContentDefinition);
				}
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("DefeatConditions"));
		if (val4 != null)
		{
			foreach (XElement item3 in ((XContainer)val4).Elements())
			{
				if (BossPhaseConditionsFactory.BossPhaseConditionDefinitionFromXElement(item3, out var bossPhaseContentDefinition2))
				{
					DefeatConditionsDefinitions.Add(bossPhaseContentDefinition2);
				}
			}
		}
		foreach (XElement item4 in ((XContainer)val).Elements(XName.op_Implicit("PhaseHandler")))
		{
			BossPhaseHandlerDefinitions.Add(new BossPhaseHandlerDefinition((XContainer)(object)item4));
		}
	}
}
