using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class PerkEventDefinition : TheLastStand.Framework.Serialization.Definition
{
	public E_EffectTime EffectTime { get; private set; }

	public List<APerkActionDefinition> PerkActionDefinitions { get; private set; }

	public PerkDataConditionsDefinition PerkDataConditionsDefinition { get; private set; }

	public PerkEventDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("EffectTime"));
		if (Enum.TryParse<E_EffectTime>(val2.Value, out var result))
		{
			EffectTime = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse EffectTime attribute into an E_EffectTime : \"" + val2.Value + "\"."), (LogType)0, (CLogLevel)2, true, "PerkEventDefinition", false);
		}
		PerkDataConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Conditions")), base.TokenVariables);
		PerkActionDefinitions = new List<APerkActionDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("Actions"))).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DecreaseBuffer":
				PerkActionDefinitions.Add(new DecreaseBufferDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "IncreaseBuffer":
				PerkActionDefinitions.Add(new IncreaseBufferDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "SetBufferTo":
				PerkActionDefinitions.Add(new SetBufferDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "TriggerEffects":
				PerkActionDefinitions.Add(new TriggerEffectsDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "TriggerEffectsOnAllAttackData":
				PerkActionDefinitions.Add(new TriggerEffectsOnAllAttackDataDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "InstantiateStatEffectDisplay":
				PerkActionDefinitions.Add(new InstantiateStatEffectDisplayDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "InstantiateRestoreEffectDisplay":
				PerkActionDefinitions.Add(new InstantiateRestoreEffectDisplayDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "InstantiateBuffEffectDisplay":
				PerkActionDefinitions.Add(new InstantiateBuffEffectDisplayDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "RefreshPerkActivationFeedback":
				PerkActionDefinitions.Add(new RefreshPerkActivationFeedbackDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "ForbidSkillUndo":
				PerkActionDefinitions.Add(new ForbidSkillUndoDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "RefillGauge":
				PerkActionDefinitions.Add(new RefillGaugeDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log((object)("Trying to Deserialize an unimplemented PerkAction : " + item.Name.LocalName), (LogType)0, (CLogLevel)2, true, "PerkEventDefinition", false);
				break;
			}
		}
	}
}
