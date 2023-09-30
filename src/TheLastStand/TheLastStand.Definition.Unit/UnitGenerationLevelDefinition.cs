using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

[Serializable]
public class UnitGenerationLevelDefinition : Definition
{
	public string Id { get; set; }

	public Dictionary<int, SealedUnitGenerationLevelDefinition> SealDefinitions { get; set; } = new Dictionary<int, SealedUnitGenerationLevelDefinition>();


	public UnitGenerationLevelDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			Debug.LogError((object)"The UnitGenerationDefinition has no Id!");
			return;
		}
		Id = val2.Value;
		for (int i = 0; i < 7; i++)
		{
			SealedUnitGenerationLevelDefinition sealedUnitGenerationLevelDefinition = ((i == 0 || SealDefinitions[0] == null) ? new SealedUnitGenerationLevelDefinition(this) : SealDefinitions[0].ShallowCopy());
			XElement val3 = null;
			if (i == 0)
			{
				val3 = ((XContainer)val).Element(XName.op_Implicit("Default"));
			}
			else
			{
				foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("SealOpenOverride")))
				{
					if (item.Attribute(XName.op_Implicit("Seal")) != null && int.TryParse(item.Attribute(XName.op_Implicit("Seal")).Value, out var result) && result == i)
					{
						val3 = item;
						break;
					}
				}
			}
			if (val3 != null)
			{
				sealedUnitGenerationLevelDefinition.Deserialize((XContainer)(object)val3);
				sealedUnitGenerationLevelDefinition.Seal = i;
				SealDefinitions.Add(sealedUnitGenerationLevelDefinition.Seal, sealedUnitGenerationLevelDefinition);
			}
		}
	}
}
