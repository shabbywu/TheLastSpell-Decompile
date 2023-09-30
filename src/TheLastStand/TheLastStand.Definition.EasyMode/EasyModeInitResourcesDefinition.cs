using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.EasyMode;

public class EasyModeInitResourcesDefinition : EasyModeModifierDefinition
{
	public const string Name = "InitResources";

	public int InitGold { get; private set; }

	public int InitMaterials { get; private set; }

	public override string LocalizedModifier => Localizer.Format("EasyMode_Modifier_InitResources", new object[2] { InitGold, InitMaterials });

	public EasyModeInitResourcesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Gold"));
		if (val2 != null)
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse " + val2.Value + " to an integer value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			InitGold = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Materials"));
		if (val3 != null)
		{
			if (!int.TryParse(val3.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse " + val3.Value + " to an integer value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				InitMaterials = result2;
			}
		}
	}
}
