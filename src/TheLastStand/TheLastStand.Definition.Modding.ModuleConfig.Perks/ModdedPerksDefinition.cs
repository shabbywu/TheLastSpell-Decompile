using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class ModdedPerksDefinition : Definition
{
	public Dictionary<string, PerkDefinition> PerkDefinitions { get; } = new Dictionary<string, PerkDefinition>();


	public ModdedPerksDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = ((container is XDocument) ? container : null).Element(XName.op_Implicit("PerkDefinitions"));
		IEnumerable<XElement> enumerable = ((obj != null) ? ((XContainer)obj).Elements(XName.op_Implicit("PerkDefinition")) : null);
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item in enumerable)
		{
			PerkDefinition value = new PerkDefinition((XContainer)(object)item);
			XAttribute obj2 = item.Attribute(XName.op_Implicit("Id"));
			string text = ((obj2 != null) ? obj2.Value : null);
			if (!string.IsNullOrEmpty(text))
			{
				PerkDefinitions[text] = value;
				PlayableUnitDatabase.PerkDefinitions[text] = value;
			}
			else
			{
				CLoggerManager.Log((object)"Missing or Empty Id attribute for a PerkDefinition Element, skipping.", (LogType)0, (CLogLevel)2, true, "ModManager", false);
			}
		}
	}
}
