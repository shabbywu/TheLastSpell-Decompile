using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class RecruitmentDefinition : Definition
{
	public Node MageCost { get; set; }

	public float MageGenerationProbabilityIncreasedPerReRoll { get; set; }

	public float MageGenerationStartProbability { get; set; }

	public int RosterRerollCost { get; set; }

	public Node UnitCost { get; set; }

	public List<UnitGenerationDefinition> UnitsToGenerate { get; private set; } = new List<UnitGenerationDefinition>();


	public RecruitmentDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = container.Element(XName.op_Implicit("UnitGenerationSettings"));
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Cost"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"The UnitGenerationSettings must have a Cost");
			return;
		}
		UnitCost = Parser.Parse(val2.Value, (Dictionary<string, string>)null);
		((XContainer)val).Element(XName.op_Implicit("UnitLimits"));
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Slot")))
		{
			if (XDocumentExtensions.IsNullOrEmpty(item.Attribute(XName.op_Implicit("Id"))))
			{
				Debug.LogError((object)"The Slot must have Id");
			}
			else
			{
				UnitsToGenerate.Add(new UnitGenerationDefinition((XContainer)(object)item));
			}
		}
		XElement val3 = container.Element(XName.op_Implicit("MageGenerationSettings"));
		if (XDocumentExtensions.IsNullOrEmpty(val3))
		{
			Debug.LogError((object)"The document must have MageGenerationSettings");
			return;
		}
		XElement val4 = ((XContainer)val3).Element(XName.op_Implicit("Cost"));
		if (!XDocumentExtensions.IsNullOrEmpty(val4))
		{
			if (!int.TryParse(val4.Value, out var _))
			{
				Debug.LogError((object)"MageGenerationSettings must have a valid cost!");
				return;
			}
			MageCost = Parser.Parse(val4.Value, (Dictionary<string, string>)null);
		}
		XElement val5 = ((XContainer)val3).Element(XName.op_Implicit("StartProbability"));
		if (!XDocumentExtensions.IsNullOrEmpty(val5))
		{
			if (!float.TryParse(val5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				Debug.LogError((object)"MageGenerationSettings must have a valid start probability!");
				return;
			}
			MageGenerationStartProbability = result2;
		}
		XElement val6 = ((XContainer)val3).Element(XName.op_Implicit("ProbabilityIncreasedPerReRoll"));
		if (!XDocumentExtensions.IsNullOrEmpty(val6))
		{
			if (!float.TryParse(val6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				Debug.LogError((object)"MageGenerationSettings must have a valid ProbabilityIncreasedPerReRoll!");
				return;
			}
			MageGenerationProbabilityIncreasedPerReRoll = result3;
		}
		XElement val7 = container.Element(XName.op_Implicit("RosterRerollCost"));
		if (!XDocumentExtensions.IsNullOrEmpty(val7))
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
