using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ExileCasterEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ExileCaster";
	}

	public override string Id => "ExileCaster";

	public override bool DisplayCompendiumEntry => false;

	public bool ForcePlayDieAnim { get; private set; }

	public override bool ShouldBeDisplayed => false;

	public ExileCasterEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		AffectedUnits = E_SkillUnitAffect.Caster;
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("ForcePlayDieAnim"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				ForcePlayDieAnim = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse ExileCaster effect ForcePlayDieAnim value " + val.Value + " to a valid bool."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
