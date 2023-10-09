using System;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public class AttackSkillActionDefinition : SkillActionDefinition
{
	public enum E_AttackType : byte
	{
		None,
		Physical,
		Magical,
		Ranged,
		Adaptative
	}

	public const string Name = "Attack";

	public Node MinDamageNode;

	public Node MaxDamageNode;

	public E_AttackType AttackType { get; private set; }

	public float CriticProbability { get; private set; }

	public float DamageMultiplier { get; private set; }

	public AttackSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = container.Element(XName.op_Implicit("Attack"));
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("BaseDamage"));
		if (val2 != null)
		{
			MinDamageNode = Parser.Parse(val2.Attribute(XName.op_Implicit("Min")).Value);
			MaxDamageNode = Parser.Parse(val2.Attribute(XName.op_Implicit("Max")).Value);
		}
		DamageMultiplier = float.Parse(((XContainer)val).Element(XName.op_Implicit("DamageMultiplier")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("AttackType"));
		if (Enum.TryParse<E_AttackType>((obj != null) ? obj.Value : null, out var result))
		{
			AttackType = result;
		}
		else
		{
			XAttribute obj2 = ((XObject)((XObject)val).Parent).Parent.Attribute(XName.op_Implicit("Id"));
			CLoggerManager.Log((object)("Error while parsing AttackType of Skill " + ((obj2 != null) ? obj2.Value : null) + " \"skillActionSpecificElement.Element(\"AttackType\")?.Value\" to a valid E_AttackType value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		if (((XContainer)val).Element(XName.op_Implicit("CriticProbability")) != null)
		{
			if (float.TryParse(((XContainer)val).Element(XName.op_Implicit("CriticProbability")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				CriticProbability = result2;
			}
			else
			{
				Debug.LogError((object)("CriticProbability of AttackDefinition value is not a float: " + ((XContainer)val).Element(XName.op_Implicit("CriticProbability")).Value));
			}
		}
	}

	public Vector2 GetBaseDamage(FormulaInterpreterContext formulaInterpreterContext)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (MinDamageNode != null && MaxDamageNode != null)
		{
			return new Vector2(MinDamageNode.EvalToFloat(formulaInterpreterContext), MaxDamageNode.EvalToFloat(formulaInterpreterContext));
		}
		return Vector2.zero;
	}
}
