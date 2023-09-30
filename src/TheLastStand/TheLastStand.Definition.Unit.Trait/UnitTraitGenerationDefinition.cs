using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Trait;

public class UnitTraitGenerationDefinition : Definition
{
	public int StartTraitTotalPoints { get; private set; }

	public int StartTraitTotalPointsWithModifiers => StartTraitTotalPoints + TPSingleton<MetaUpgradesManager>.Instance.ComputeStartTraitTotalPointsModifiers();

	public Vector2Int UnitTraitPointBoundaries { get; private set; }

	public Vector2Int UnitTraitPointBoundariesWithModifiers => UnitTraitPointBoundaries + TPSingleton<MetaUpgradesManager>.Instance.ComputeUnitTraitPointsBoundariesModifiers();

	public UnitTraitGenerationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		XElement val = container.Element(XName.op_Implicit("UnitTraitGenerationDefinition"));
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("StartTraitTotalPoints"));
		if (val2 == null)
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition must have an element StartTraitTotalPoints", (Object)null);
			return;
		}
		XAttribute val3 = val2.Attribute(XName.op_Implicit("Value"));
		if (XDocumentExtensions.IsNullOrEmpty(val3))
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition StartTraitTotalPoints must have an attribute Value", (Object)null);
			return;
		}
		if (!int.TryParse(val3.Value, out var result))
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition StartTraitTotalPoints must have a valid attribute Value (int)", (Object)null);
			return;
		}
		StartTraitTotalPoints = result;
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("UnitTraitPointsBoundaries"));
		if (val4 == null)
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition must have an element UnitTraitPointsBoundaries", (Object)null);
			return;
		}
		XAttribute val5 = val4.Attribute(XName.op_Implicit("Min"));
		if (XDocumentExtensions.IsNullOrEmpty(val5))
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have an attribute Min", (Object)null);
			return;
		}
		if (!int.TryParse(val5.Value, out var result2))
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have a valid attribute Min (int)", (Object)null);
			return;
		}
		XAttribute val6 = val4.Attribute(XName.op_Implicit("Max"));
		int result3;
		if (XDocumentExtensions.IsNullOrEmpty(val6))
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have an attribute Max", (Object)null);
		}
		else if (!int.TryParse(val6.Value, out result3))
		{
			TPDebug.LogError((object)"UnitTraitGenerationDefinition UnitTraitPointsBoundaries must have a valid attribute Max (int)", (Object)null);
		}
		else
		{
			UnitTraitPointBoundaries = new Vector2Int(result2, result3);
		}
	}
}
