using System.Xml.Linq;
using TPLib;
using UnityEngine;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class HeroSurroundedByEnemiesTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "HeroSurroundedByEnemies";

	public override object[] DescriptionLocalizationParameters => new object[2] { NumberOfEnemiesToBeSurroundedBy, base.Value };

	public int NumberOfEnemiesToBeSurroundedBy { get; private set; }

	public HeroSurroundedByEnemiesTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (!int.TryParse(val.Attribute(XName.op_Implicit("NumberOfTurns")).Value, out var result))
		{
			TPDebug.LogError((object)"The Attribute 'NumberOfTurns' of an Element : HeroSurroundedByEnnemies in TrophiesDefinitions should have a value of type int.", (Object)null);
			return;
		}
		base.Value = result;
		if (!int.TryParse(val.Value, out var result2))
		{
			TPDebug.LogError((object)"The Value of an Element : HeroSurroundedByEnnemies in TrophiesDefinitions isn't a valid int", (Object)null);
		}
		else
		{
			NumberOfEnemiesToBeSurroundedBy = result2;
		}
	}

	public override string ToString()
	{
		return "HeroSurroundedByEnemies";
	}
}
