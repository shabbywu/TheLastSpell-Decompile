using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class RecruitmentDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node MageCost { get; set; }

	public float MageGenerationProbabilityIncreasedPerReRoll { get; set; }

	public float MageGenerationStartProbability { get; set; }

	public int RosterRerollCost { get; set; }

	public Node UnitCost { get; set; }

	public List<UnitGenerationDefinition> UnitsToGenerate { get; private set; } = new List<UnitGenerationDefinition>();


	public RecruitmentDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = container.Element(XName.op_Implicit("UnitGenerationSettings"));
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Cost"));
		if (val2.IsNullOrEmpty())
		{
			Debug.LogError((object)"The UnitGenerationSettings must have a Cost");
			return;
		}
		UnitCost = Parser.Parse(val2.Value);
		((XContainer)val).Element(XName.op_Implicit("UnitLimits"));
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Slot")))
		{
			if (item.Attribute(XName.op_Implicit("Id")).IsNullOrEmpty())
			{
				Debug.LogError((object)"The Slot must have Id");
			}
			else
			{
				UnitsToGenerate.Add(new UnitGenerationDefinition((XContainer)(object)item));
			}
		}
		XElement val3 = container.Element(XName.op_Implicit("MageGenerationSettings"));
		if (val3.IsNullOrEmpty())
		{
			Debug.LogError((object)"The document must have MageGenerationSettings");
			return;
		}
		XElement val4 = ((XContainer)val3).Element(XName.op_Implicit("Cost"));
		if (!val4.IsNullOrEmpty())
		{
			if (!int.TryParse(val4.Value, out var _))
			{
				Debug.LogError((object)"MageGenerationSettings must have a valid cost!");
				return;
			}
			MageCost = Parser.Parse(val4.Value);
		}
		XElement val5 = ((XContainer)val3).Element(XName.op_Implicit("StartProbability"));
		if (!val5.IsNullOrEmpty())
		{
			if (!float.TryParse(val5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				Debug.LogError((object)"MageGenerationSettings must have a valid start probability!");
				return;
			}
			MageGenerationStartProbability = result2;
		}
		XElement val6 = ((XContainer)val3).Element(XName.op_Implicit("ProbabilityIncreasedPerReRoll"));
		if (!val6.IsNullOrEmpty())
		{
			if (!float.TryParse(val6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				Debug.LogError((object)"MageGenerationSettings must have a valid ProbabilityIncreasedPerReRoll!");
				return;
			}
			MageGenerationProbabilityIncreasedPerReRoll = result3;
		}
		XElement val7 = container.Element(XName.op_Implicit("RosterRerollCost"));
		if (!val7.IsNullOrEmpty())
		{
			if (!int.TryParse(val7.Value, out var result4))
			{
				Debug.LogError((object)"RecruitmentDefinition must have a valid RosterRerollCost!");
			}
			else
			{
				RosterRerollCost = result4;
			}
		}
	}
}
