using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeDecreasePricesDefinition : EasyModeModifierDefinition
{
	public const string Name = "DecreasePrices";

	public override string LocalizedModifier => Localizer.Format("EasyMode_Modifier_DecreasePrices", new object[1] { Value });

	public int Value { get; private set; }

	public EasyModeDecreasePricesDefinition(XContainer container)
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
