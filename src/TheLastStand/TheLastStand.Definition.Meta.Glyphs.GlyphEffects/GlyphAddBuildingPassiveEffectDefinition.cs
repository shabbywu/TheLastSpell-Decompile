using System.Collections.Generic;
using System.Xml.Linq;
using Sirenix.Utilities;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphAddBuildingPassiveEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "AddBuildingPassive";

	public BuildingPassiveDefinition BuildingPassiveDefinition { get; private set; }

	public HashSet<string> BuildingIds { get; } = new HashSet<string>();


	public GlyphAddBuildingPassiveEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		string text = StringExtensions.Replace(val.Attribute(XName.op_Implicit("PassiveId")).Value, ((Definition)this).TokenVariables);
		if (BuildingDatabase.BuildingPassiveDefinitions.TryGetValue(text, out var value))
		{
			BuildingPassiveDefinition = value;
		}
		else
		{
			CLoggerManager.Log((object)("AddBuildingPassive BuildingPassive " + text + " was not found!"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Building")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("Id"));
			BuildingIds.Add(val2.Value);
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("BuildingsList")))
		{
			XAttribute val3 = item2.Attribute(XName.op_Implicit("Id"));
			LinqExtensions.AddRange<string>(BuildingIds, (IEnumerable<string>)GenericDatabase.IdsListDefinitions[val3.Value].Ids);
		}
	}
}
