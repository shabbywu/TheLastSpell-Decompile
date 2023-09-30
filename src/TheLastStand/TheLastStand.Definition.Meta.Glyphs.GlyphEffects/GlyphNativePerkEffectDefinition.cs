using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphNativePerkEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "NativePerk";

	public PerkDefinition PerkDefinition { get; private set; }

	public bool ForceHideTooltip { get; private set; }

	public GlyphNativePerkEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		string text = StringExtensions.Replace(((XElement)obj).Attribute(XName.op_Implicit("PerkId")).Value, ((Definition)this).TokenVariables);
		if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(text, out var value))
		{
			PerkDefinition = value;
		}
		else
		{
			CLoggerManager.Log((object)("NativePerk Perk " + text + " was not found!"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ForceHideTooltip"));
		if (val != null)
		{
			ForceHideTooltip = bool.Parse(val.Value);
		}
	}
}
