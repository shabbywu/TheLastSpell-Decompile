using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class CompendiumDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<string, ACompendiumEntryDefinition> CompendiumEntryDefinitions { get; private set; }

	public Dictionary<string, string> SkillEffectAliases { get; private set; }

	public Dictionary<string, string> AttackTypeAliases { get; private set; }

	public CompendiumDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		CompendiumEntryDefinitions = new Dictionary<string, ACompendiumEntryDefinition>();
		SkillEffectAliases = new Dictionary<string, string>();
		AttackTypeAliases = new Dictionary<string, string>();
		foreach (XElement item in ((XContainer)obj.Element(XName.op_Implicit("CompendiumEntries"))).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "SkillEffectEntry":
			{
				SkillEffectEntryDefinition skillEffectEntryDefinition = new SkillEffectEntryDefinition((XContainer)(object)item);
				if (AddCompendiumEntry(skillEffectEntryDefinition) && !SkillEffectAliases.ContainsKey(skillEffectEntryDefinition.SkillEffectId))
				{
					SkillEffectAliases.Add(skillEffectEntryDefinition.SkillEffectId, skillEffectEntryDefinition.Id);
				}
				break;
			}
			case "AttackTypeEntry":
			{
				AttackTypeEntryDefinition attackTypeEntryDefinition = new AttackTypeEntryDefinition((XContainer)(object)item);
				if (AddCompendiumEntry(attackTypeEntryDefinition) && !AttackTypeAliases.ContainsKey(attackTypeEntryDefinition.AttackType.ToString()))
				{
					AttackTypeAliases.Add(attackTypeEntryDefinition.AttackType.ToString(), attackTypeEntryDefinition.Id);
				}
				break;
			}
			case "GameConceptEntry":
				AddCompendiumEntry(new GameConceptEntryDefinition((XContainer)(object)item));
				break;
			}
		}
	}

	private bool AddCompendiumEntry(ACompendiumEntryDefinition compendiumEntryDefinition)
	{
		if (!CompendiumEntryDefinitions.ContainsKey(compendiumEntryDefinition.Id))
		{
			CompendiumEntryDefinitions.Add(compendiumEntryDefinition.Id, compendiumEntryDefinition);
			return true;
		}
		return false;
	}
}
