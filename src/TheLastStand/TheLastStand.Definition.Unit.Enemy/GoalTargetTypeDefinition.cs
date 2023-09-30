using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit.Enemy.GoalCondition;
using TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class GoalTargetTypeDefinition : Definition
{
	public enum E_TargetType
	{
		Undefined,
		PlayableUnit,
		EnemyUnit,
		Building,
		Tile,
		TileFlag,
		Itself
	}

	public static class Constants
	{
		public const string Occupied = "Occupied";

		public const string Empty = "Empty";
	}

	public GoalConditionDefinition[][] ConditionGroups { get; private set; }

	public E_TargetType TargetType { get; private set; }

	public bool? IsTileContentAccepted { get; private set; }

	public GoalTargetTypeDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (!Enum.TryParse<E_TargetType>(val.Name.LocalName, out var result))
		{
			Debug.LogError((object)("Unknown TargetType " + val.Name.LocalName));
			return;
		}
		TargetType = result;
		XAttribute obj = val.Attribute(XName.op_Implicit("MustBe"));
		switch ((obj != null) ? obj.Value : null)
		{
		case "Occupied":
			IsTileContentAccepted = true;
			break;
		case "Empty":
			IsTileContentAccepted = false;
			break;
		default:
			IsTileContentAccepted = null;
			break;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("TargetConditions"));
		if (val2 == null)
		{
			return;
		}
		int num = 0;
		foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("ConditionsGroup")))
		{
			_ = item;
			num++;
		}
		ConditionGroups = new GoalConditionDefinition[num][];
		int num2 = 0;
		foreach (XElement item2 in ((XContainer)val2).Elements(XName.op_Implicit("ConditionsGroup")))
		{
			int num3 = 0;
			foreach (XElement item3 in ((XContainer)item2).Elements())
			{
				_ = item3;
				num3++;
			}
			ConditionGroups[num2] = new GoalConditionDefinition[num3];
			int num4 = 0;
			XElement val3 = ((XContainer)item2).Element(XName.op_Implicit("DamageableCountInAoeCondition"));
			if (val3 != null)
			{
				ConditionGroups[num2][num4++] = new DamageableCountInAoeConditionDefinition((XContainer)(object)val3);
			}
			XElement val4 = ((XContainer)item2).Element(XName.op_Implicit("ExcludeDamageableTypeInAoeCondition"));
			if (val4 != null)
			{
				ConditionGroups[num2][num4++] = new ExcludeDamageableTypeInAoeConditionDefinition((XContainer)(object)val4);
			}
			XElement val5 = ((XContainer)item2).Element(XName.op_Implicit("FlagTagCondition"));
			if (val5 != null)
			{
				ConditionGroups[num2][num4++] = new FlagTagConditionDefinition((XContainer)(object)val5);
			}
			XElement val6 = ((XContainer)item2).Element(XName.op_Implicit("GroundCategoryCondition"));
			if (val6 != null)
			{
				ConditionGroups[num2][num4++] = new GroundCategoryConditionDefinition((XContainer)(object)val6);
			}
			XElement val7 = ((XContainer)item2).Element(XName.op_Implicit("TargetIdCondition"));
			if (val7 != null)
			{
				ConditionGroups[num2][num4++] = new TargetIdConditionDefinition((XContainer)(object)val7);
			}
			XElement val8 = ((XContainer)item2).Element(XName.op_Implicit("TargetInRangeCondition"));
			if (val8 != null)
			{
				ConditionGroups[num2][num4++] = new TargetInRangeConditionDefinition((XContainer)(object)val8);
			}
			XElement val9 = ((XContainer)item2).Element(XName.op_Implicit("TargetIsNotEliteCondition"));
			if (val9 != null)
			{
				ConditionGroups[num2][num4++] = new TargetIsNotEliteConditionDefinition((XContainer)(object)val9);
			}
			XElement val10 = ((XContainer)item2).Element(XName.op_Implicit("TargetHealthCondition"));
			if (val10 != null)
			{
				ConditionGroups[num2][num4++] = new TargetHealthConditionDefinition((XContainer)(object)val10);
			}
			XElement val11 = ((XContainer)item2).Element(XName.op_Implicit("TileHasHazardCondition"));
			if (val11 != null)
			{
				ConditionGroups[num2][num4++] = new TileHasHazardConditionDefinition((XContainer)(object)val11);
			}
			num2++;
		}
	}
}
