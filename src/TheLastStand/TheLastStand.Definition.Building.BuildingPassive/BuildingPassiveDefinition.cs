using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class BuildingPassiveDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public bool HasOnDeathEffect { get; private set; }

	public List<BuildingPassiveEffectDefinition> PassiveEffectDefinitions { get; private set; }

	public List<PassiveTriggerDefinition> TriggerDefinitions { get; private set; }

	public BuildingPassiveDefinition(XContainer container)
		: base(container)
	{
	}

	public virtual BuildingPassiveDefinition Clone()
	{
		return MemberwiseClone() as BuildingPassiveDefinition;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		PassiveEffectDefinitions = new List<BuildingPassiveEffectDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("UpdateShopLevel")))
		{
			PassiveEffectDefinitions.Add(new UpdateShopLevelDefinition((XContainer)(object)item));
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("UpdateBonePileLevel")))
		{
			PassiveEffectDefinitions.Add(new UpdateBonePileLevelDefinition((XContainer)(object)item2));
		}
		foreach (XElement item3 in ((XContainer)val).Elements(XName.op_Implicit("FillEffectGauge")))
		{
			PassiveEffectDefinitions.Add(new FillEffectGaugeDefinition((XContainer)(object)item3));
		}
		foreach (XElement item4 in ((XContainer)val).Elements(XName.op_Implicit("GenerateNewItemsRoster")))
		{
			PassiveEffectDefinitions.Add(new GenerateNewItemsRosterDefinition((XContainer)(object)item4));
		}
		foreach (XElement item5 in ((XContainer)val).Elements(XName.op_Implicit("ImproveSpawnWaveInfo")))
		{
			PassiveEffectDefinitions.Add(new ImproveSpawnWaveInfoDefinition((XContainer)(object)item5));
		}
		foreach (XElement item6 in ((XContainer)val).Elements(XName.op_Implicit("IncreaseWorkers")))
		{
			PassiveEffectDefinitions.Add(new IncreaseWorkersDefinition((XContainer)(object)item6));
		}
		foreach (XElement item7 in ((XContainer)val).Elements(XName.op_Implicit("DestroyBuilding")))
		{
			PassiveEffectDefinitions.Add(new DestroyBuildingDefinition((XContainer)(object)item7));
		}
		foreach (XElement item8 in ((XContainer)val).Elements(XName.op_Implicit("TransformBuilding")))
		{
			PassiveEffectDefinitions.Add(new TransformBuildingDefinition((XContainer)(object)item8));
		}
		foreach (XElement item9 in ((XContainer)val).Elements(XName.op_Implicit("GenerateLightFog")))
		{
			PassiveEffectDefinitions.Add(new GenerateLightFogDefinition((XContainer)(object)item9));
		}
		foreach (XElement item10 in ((XContainer)val).Elements(XName.op_Implicit("GenerateGuardian")))
		{
			PassiveEffectDefinitions.Add(new GenerateGuardianDefinition((XContainer)(object)item10));
		}
		foreach (XElement item11 in ((XContainer)val).Elements(XName.op_Implicit("GainResources")))
		{
			PassiveEffectDefinitions.Add(new GainResourcesDefinition((XContainer)(object)item11));
		}
		TriggerDefinitions = new List<PassiveTriggerDefinition>();
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Triggers"));
		foreach (XElement item12 in ((XContainer)val3).Elements(XName.op_Implicit("Permanent")))
		{
			TriggerDefinitions.Add(new PermanentTriggerDefinition((XContainer)(object)item12));
		}
		foreach (XElement item13 in ((XContainer)val3).Elements(XName.op_Implicit("StartProductionTurn")))
		{
			TriggerDefinitions.Add(new StartOfProductionTriggerDefinition((XContainer)(object)item13));
		}
		foreach (XElement item14 in ((XContainer)val3).Elements(XName.op_Implicit("EndProductionTurn")))
		{
			TriggerDefinitions.Add(new EndOfProductionTriggerDefinition((XContainer)(object)item14));
		}
		foreach (XElement item15 in ((XContainer)val3).Elements(XName.op_Implicit("OnDeath")))
		{
			TriggerDefinitions.Add(new OnDeathTriggerDefinition((XContainer)(object)item15));
		}
		foreach (XElement item16 in ((XContainer)val3).Elements(XName.op_Implicit("OnConstruction")))
		{
			TriggerDefinitions.Add(new OnConstructionTriggerDefinition((XContainer)(object)item16));
		}
		foreach (XElement item17 in ((XContainer)val3).Elements(XName.op_Implicit("AfterXProductionPhases")))
		{
			TriggerDefinitions.Add(new AfterXProductionPhasesTriggerDefinition((XContainer)(object)item17));
		}
		foreach (XElement item18 in ((XContainer)val3).Elements(XName.op_Implicit("AfterXNightEnd")))
		{
			TriggerDefinitions.Add(new AfterXNightEndTriggerDefinition((XContainer)(object)item18));
		}
		foreach (XElement item19 in ((XContainer)val3).Elements(XName.op_Implicit("AfterXNightTurns")))
		{
			TriggerDefinitions.Add(new AfterXNightTurnsTriggerDefinition((XContainer)(object)item19));
		}
		foreach (XElement item20 in ((XContainer)val3).Elements(XName.op_Implicit("OnExtinguish")))
		{
			TriggerDefinitions.Add(new OnExtinguishTriggerDefinition((XContainer)(object)item20));
		}
		HasOnDeathEffect = TriggerDefinitions.Any((PassiveTriggerDefinition x) => x.EffectTime == E_EffectTime.OnDeath || x.EffectTime == E_EffectTime.OnExtinguish);
	}
}
