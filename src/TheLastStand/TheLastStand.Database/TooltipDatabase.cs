using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class TooltipDatabase : Database<TooltipDatabase>
{
	[SerializeField]
	private TextAsset compendiumDefinitionTextAsset;

	public static CompendiumDefinition CompendiumDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement val = ((XContainer)XDocument.Parse(compendiumDefinitionTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("CompendiumDefinition"));
		if (val.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)("The document " + ((Object)compendiumDefinitionTextAsset).name + " must have an CompendiumDefinition!"), (LogType)0, (CLogLevel)2, true, "TooltipDatabase", false);
		}
		else
		{
			CompendiumDefinition = new CompendiumDefinition((XContainer)(object)val);
		}
	}

	public HashSet<ACompendiumEntryDefinition> GetLinkedEntryDefinitions(string id)
	{
		if (!CompendiumDefinition.CompendiumEntryDefinitions.TryGetValue(id, out var value))
		{
			CLoggerManager.Log((object)("Compendium entry \"" + id + "\" wasn't found in the database."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			return null;
		}
		return GetLinkedEntryDefinitions(value);
	}

	public HashSet<ACompendiumEntryDefinition> GetLinkedEntryDefinitions(ACompendiumEntryDefinition entryDefinition)
	{
		HashSet<ACompendiumEntryDefinition> hashSet = new HashSet<ACompendiumEntryDefinition>();
		foreach (string linkedEntry in entryDefinition.LinkedEntries)
		{
			if (!CompendiumDefinition.CompendiumEntryDefinitions.TryGetValue(linkedEntry, out var value))
			{
				CLoggerManager.Log((object)("Linked compendium entry \"" + linkedEntry + "\" wasn't found in the database."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				hashSet.Add(value);
			}
		}
		return hashSet;
	}
}
