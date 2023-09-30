using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class UpgradeCityMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UpgradeCity";

	public string CityId { get; private set; }

	public int Level { get; private set; }

	public UpgradeCityMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Id"));
		CityId = val.Value;
		if (!int.TryParse(obj.Element(XName.op_Implicit("Level")).Value, out var result))
		{
			CLoggerManager.Log((object)"Could not cast the level value into an int !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			Level = result;
		}
	}
}
