using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphToggleSkillProgressionFlagEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "ToggleSkillProgressionFlag";

	public GlyphManager.E_SkillProgressionFlag SkillProgressionFlag { get; private set; }

	public GlyphToggleSkillProgressionFlagEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Flag"));
		string text = StringExtensions.Replace(val.Value, ((Definition)this).TokenVariables);
		if (!Enum.TryParse<GlyphManager.E_SkillProgressionFlag>(text, out var result))
		{
			CLoggerManager.Log((object)("ToggleSkillProgressionFlag Unable to parse " + text + " (" + val.Value + ") into a E_SkillProgressionFlag"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		SkillProgressionFlag = result;
	}
}
