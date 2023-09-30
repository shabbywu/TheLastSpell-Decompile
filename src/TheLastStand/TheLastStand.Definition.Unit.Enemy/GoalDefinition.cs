using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Enemy.GoalCondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPostcondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;
using TheLastStand.Definition.Unit.Enemy.PositioningMethod;
using TheLastStand.Definition.Unit.Enemy.TargetingMethod;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class GoalDefinition : Definition
{
	[Flags]
	public enum E_InterruptionCondition
	{
		None = 0,
		AdjacentToPlayableUnit = 1,
		AdjacentToEnemyUnit = 2,
		AdjacentToUnit = 3,
		AdjacentToBuildingExceptWallAndBarricade = 4,
		AdjacentToWall = 8,
		AdjacentToBarricade = 0x10,
		AdjacentToBuilding = 0x1C
	}

	public int Cooldown { get; private set; }

	public GoalTargetTypeDefinition[] GoalTargetTypeDefinitions { get; private set; }

	public string Id { get; private set; }

	public E_InterruptionCondition InterruptionCondition { get; private set; }

	public IBehaviorModel.E_GoalComputingStep GoalComputingStep { get; private set; } = IBehaviorModel.E_GoalComputingStep.DuringTurn;


	public TheLastStand.Definition.Unit.Enemy.PositioningMethod.PositioningMethod PositioningMethod { get; private set; }

	public GoalConditionDefinition[][] PostconditionGroups { get; private set; }

	public GoalConditionDefinition[][] PreconditionGroups { get; private set; }

	public string SkillId { get; private set; }

	public TargetingMethodsContainerDefinition TargetingMethodsContainer { get; private set; }

	public GoalDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("GoalComputingStep"));
		if (val3 != null && Enum.TryParse<IBehaviorModel.E_GoalComputingStep>(val3.Value, out var result))
		{
			GoalComputingStep = result;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("SkillId"));
		if (val4 == null)
		{
			Debug.LogError((object)"GoalDefinition must have SkillId");
			return;
		}
		XAttribute val5 = val4.Attribute(XName.op_Implicit("Value"));
		if (val5 == null)
		{
			Debug.LogError((object)"SkillId must have Value");
			return;
		}
		SkillId = val5.Value;
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("Cooldown"));
		if (val6 != null)
		{
			XAttribute val7 = val6.Attribute(XName.op_Implicit("Value"));
			Cooldown = int.Parse(val7.Value);
		}
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("Preconditions"));
		if (val8 != null)
		{
			int num = 0;
			foreach (XElement item in ((XContainer)val8).Elements(XName.op_Implicit("ConditionsGroup")))
			{
				_ = item;
				num++;
			}
			PreconditionGroups = new GoalConditionDefinition[num][];
			int num2 = 0;
			foreach (XElement item2 in ((XContainer)val8).Elements(XName.op_Implicit("ConditionsGroup")))
			{
				int num3 = 0;
				foreach (XElement item3 in ((XContainer)item2).Elements())
				{
					_ = item3;
					num3++;
				}
				PreconditionGroups[num2] = new GoalConditionDefinition[num3];
				int num4 = 0;
				XElement val9 = ((XContainer)item2).Element(XName.op_Implicit("CasterHasStatusCondition"));
				if (val9 != null)
				{
					PreconditionGroups[num2][num4++] = new CasterHasStatusConditionDefinition((XContainer)(object)val9);
				}
				XElement val10 = ((XContainer)item2).Element(XName.op_Implicit("CasterHealthCondition"));
				if (val10 != null)
				{
					PreconditionGroups[num2][num4++] = new CasterHealthConditionDefinition((XContainer)(object)val10);
				}
				XElement val11 = ((XContainer)item2).Element(XName.op_Implicit("InterpretedTurnCondition"));
				if (val11 != null)
				{
					PreconditionGroups[num2][num4++] = new InterpretedTurnCondition((XContainer)(object)val11);
				}
				XElement val12 = ((XContainer)item2).Element(XName.op_Implicit("PlayableUnitCloseToCasterCondition"));
				if (val12 != null)
				{
					PreconditionGroups[num2][num4++] = new PlayableUnitCloseToCasterConditionDefinition((XContainer)(object)val12);
				}
				XElement val13 = ((XContainer)item2).Element(XName.op_Implicit("NotInFogCondition"));
				if (val13 != null)
				{
					PreconditionGroups[num2][num4++] = new NotInFogCondition((XContainer)(object)val13);
				}
				XElement val14 = ((XContainer)item2).Element(XName.op_Implicit("NotInAnyFogCondition"));
				if (val14 != null)
				{
					PreconditionGroups[num2][num4++] = new NotInAnyFogCondition((XContainer)(object)val14);
				}
				XElement val15 = ((XContainer)item2).Element(XName.op_Implicit("DamageableAroundCondition"));
				if (val15 != null)
				{
					PreconditionGroups[num2][num4++] = new DamageableAroundConditionDefinition((XContainer)(object)val15);
				}
				XElement val16 = ((XContainer)item2).Element(XName.op_Implicit("SkillProgressionFlagIsToggledCondition"));
				if (val16 != null)
				{
					PreconditionGroups[num2][num4++] = new SkillProgressionFlagIsToggledConditionDefinition((XContainer)(object)val16);
				}
				num2++;
			}
		}
		XElement val17 = ((XContainer)val).Element(XName.op_Implicit("TargetTypes"));
		int num5 = 0;
		foreach (XElement item4 in ((XContainer)val17).Elements())
		{
			_ = item4;
			num5++;
		}
		GoalTargetTypeDefinitions = new GoalTargetTypeDefinition[num5];
		int num6 = 0;
		foreach (XElement item5 in ((XContainer)val17).Elements())
		{
			GoalTargetTypeDefinitions[num6++] = new GoalTargetTypeDefinition((XContainer)(object)item5);
		}
		XElement val18 = ((XContainer)val).Element(XName.op_Implicit("Postconditions"));
		if (val18 != null)
		{
			int num7 = 0;
			foreach (XElement item6 in ((XContainer)val18).Elements(XName.op_Implicit("ConditionsGroup")))
			{
				_ = item6;
				num7++;
			}
			PostconditionGroups = new GoalConditionDefinition[num7][];
			int num8 = 0;
			foreach (XElement item7 in ((XContainer)val18).Elements(XName.op_Implicit("ConditionsGroup")))
			{
				int num9 = 0;
				foreach (XElement item8 in ((XContainer)item7).Elements())
				{
					_ = item8;
					num9++;
				}
				PostconditionGroups[num8] = new GoalConditionDefinition[num9];
				int num10 = 0;
				XElement val19 = ((XContainer)item7).Element(XName.op_Implicit("TargetsCountCondition"));
				if (val19 != null)
				{
					PostconditionGroups[num8][num10++] = new TargetsCountCondition((XContainer)(object)val19);
				}
				num8++;
			}
		}
		TargetingMethodsContainer = new TargetingMethodsContainerDefinition((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("TargetingMethod")));
		XElement val20 = ((XContainer)val).Element(XName.op_Implicit("PositioningMethod"));
		if (val20 != null)
		{
			if (((XContainer)val20).Element(XName.op_Implicit("ClosestTile")) != null)
			{
				PositioningMethod = new ClosestTilePositioningMethod();
			}
			if (((XContainer)val20).Element(XName.op_Implicit("FarthestTile")) != null)
			{
				PositioningMethod = new FarthestTilePositioningMethod();
			}
			if (((XContainer)val20).Element(XName.op_Implicit("Standard")) != null)
			{
				PositioningMethod = new StandardPositioningMethod();
			}
		}
		XElement val21 = ((XContainer)val).Element(XName.op_Implicit("InterruptionCondition"));
		if (val21 != null)
		{
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToPlayableUnit")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToPlayableUnit;
			}
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToEnemyUnit")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToEnemyUnit;
			}
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToUnit")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToUnit;
			}
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToBuildingExceptWallAndBarricade")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToBuildingExceptWallAndBarricade;
			}
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToWall")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToWall;
			}
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToBarricade")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToBarricade;
			}
			if (((XContainer)val21).Element(XName.op_Implicit("AdjacentToBuilding")) != null)
			{
				InterruptionCondition |= E_InterruptionCondition.AdjacentToBuilding;
			}
		}
	}
}
