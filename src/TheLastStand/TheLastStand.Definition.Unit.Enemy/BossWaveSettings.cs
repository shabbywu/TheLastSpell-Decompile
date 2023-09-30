using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy;

public class BossWaveSettings
{
	public bool AutoUpdateNightSlider { get; }

	public string BossUnitTemplateId { get; }

	public bool DisplayEnemiesAmount { get; }

	public bool IsInfiniteWave { get; }

	public bool UseDefaultProgressBar { get; }

	public string SpecificPlaylistId { get; }

	public BossWaveSettings(XElement xBossElement)
	{
		AutoUpdateNightSlider = ((XContainer)xBossElement).Element(XName.op_Implicit("AutoUpdateNightSlider")) != null;
		XAttribute val = xBossElement.Attribute(XName.op_Implicit("BossId"));
		BossUnitTemplateId = val.Value;
		DisplayEnemiesAmount = ((XContainer)xBossElement).Element(XName.op_Implicit("DisplayEnemiesAmount")) != null;
		IsInfiniteWave = ((XContainer)xBossElement).Element(XName.op_Implicit("InfiniteWave")) != null;
		XAttribute val2 = xBossElement.Attribute(XName.op_Implicit("SpecificPlaylistId"));
		if (!string.IsNullOrEmpty((val2 != null) ? val2.Value : null))
		{
			SpecificPlaylistId = val2.Value;
		}
		UseDefaultProgressBar = ((XContainer)xBossElement).Element(XName.op_Implicit("UseDefaultProgressBar")) != null;
	}
}
