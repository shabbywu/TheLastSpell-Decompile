using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class BossWaveSettings
{
	public bool AutoUpdateNightSlider { get; }

	public string BossUnitTemplateId { get; }

	public bool DisplayBossInSeer { get; }

	public bool DisplayEnemiesAmount { get; }

	public bool IsInfiniteWave { get; }

	public bool UseDefaultProgressBar { get; }

	public string SpecificPlaylistId { get; }

	public BossWaveSettings(XElement xBossElement)
	{
		AutoUpdateNightSlider = ((XContainer)xBossElement).Element(XName.op_Implicit("AutoUpdateNightSlider")) != null;
		XAttribute val = xBossElement.Attribute(XName.op_Implicit("BossId"));
		BossUnitTemplateId = val.Value;
		DisplayBossInSeer = true;
		XAttribute val2 = xBossElement.Attribute(XName.op_Implicit("DisplayBossInSeer"));
		if (val2 != null)
		{
			if (!bool.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse attribute DisplayInSeer into a bool : '" + val2.Value + "'."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
			}
			else
			{
				DisplayBossInSeer = result;
			}
		}
		DisplayEnemiesAmount = ((XContainer)xBossElement).Element(XName.op_Implicit("DisplayEnemiesAmount")) != null;
		IsInfiniteWave = ((XContainer)xBossElement).Element(XName.op_Implicit("InfiniteWave")) != null;
		XAttribute val3 = xBossElement.Attribute(XName.op_Implicit("SpecificPlaylistId"));
		if (!string.IsNullOrEmpty((val3 != null) ? val3.Value : null))
		{
			SpecificPlaylistId = val3.Value;
		}
		UseDefaultProgressBar = ((XContainer)xBossElement).Element(XName.op_Implicit("UseDefaultProgressBar")) != null;
	}
}
