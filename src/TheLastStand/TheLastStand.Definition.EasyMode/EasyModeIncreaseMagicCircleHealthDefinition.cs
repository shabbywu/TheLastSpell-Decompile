using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeIncreaseMagicCircleHealthDefinition : EasyModeModifierDefinition
{
	public const string Name = "IncreaseMagicCircleHealth";

	public override string LocalizedModifier => Localizer.Format("EasyMode_Modifier_IncreaseMagicCircleHealth", new object[1] { Value });

	public int Value { get; private set; }

	public EasyModeIncreaseMagicCircleHealthDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (!int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse " + val.Value + " to an integer value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			Value = result;
		}
	}
}
