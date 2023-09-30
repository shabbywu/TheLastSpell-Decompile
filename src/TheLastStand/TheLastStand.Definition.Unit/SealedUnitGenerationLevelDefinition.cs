using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class SealedUnitGenerationLevelDefinition : ILegacyDeserializable
{
	public Node Level { get; set; }

	public int Seal { get; set; }

	public UnitGenerationLevelDefinition UnitGenerationLevelDefinition { get; private set; }

	public SealedUnitGenerationLevelDefinition(UnitGenerationLevelDefinition unitGenerationLevelDefinition)
	{
		UnitGenerationLevelDefinition = unitGenerationLevelDefinition;
	}

	public void Deserialize(XContainer container)
	{
		XElement val = container.Element(XName.op_Implicit("Level"));
		if (val == null)
		{
			Debug.LogError((object)"The SealedUnitGenerationDefinition has no Level!");
		}
		else
		{
			Level = Parser.Parse(val.Value, (Dictionary<string, string>)null);
		}
	}

	public SealedUnitGenerationLevelDefinition ShallowCopy()
	{
		return (SealedUnitGenerationLevelDefinition)MemberwiseClone();
	}
}
