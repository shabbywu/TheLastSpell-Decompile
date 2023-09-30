using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitLevelUpDefinition : Definition
{
	public ProbabilityTreeEntriesDefinition RaritiesList { get; private set; }

	public List<int> MainStatDraws { get; private set; } = new List<int>();


	public int MaxAmountOfReroll { get; private set; }

	public List<int> SecondaryStatDraws { get; private set; } = new List<int>();


	public UnitLevelUpDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("RaritiesList"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"UnitLevelUpDefinition must have a RaritiesList");
			return;
		}
		RaritiesList = new ProbabilityTreeEntriesDefinition((XContainer)(object)val2);
		int.TryParse(((XContainer)val).Element(XName.op_Implicit("MaxAmountOfReroll")).Value, out var result);
		MaxAmountOfReroll = result;
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("MainStatDraws"))).Elements(XName.op_Implicit("Draw")))
		{
			if (XDocumentExtensions.IsNullOrEmpty(item))
			{
				TPDebug.Log((object)"ConstructionDefinition must have a Draw", (Object)null);
				return;
			}
			if (!int.TryParse(item.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2))
			{
				TPDebug.Log((object)"MagicCircle Draw must be a valid int", (Object)null);
				return;
			}
			MainStatDraws.Add(result2);
		}
		foreach (XElement item2 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("SecondaryStatDraws"))).Elements(XName.op_Implicit("Draw")))
		{
			if (XDocumentExtensions.IsNullOrEmpty(item2))
			{
				TPDebug.Log((object)"ConstructionDefinition must have a Draw", (Object)null);
				break;
			}
			if (!int.TryParse(item2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3))
			{
				TPDebug.Log((object)"MagicCircle Draw must be a valid int", (Object)null);
				break;
			}
			SecondaryStatDraws.Add(result3);
		}
	}
}
