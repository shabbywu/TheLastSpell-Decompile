using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnWaveDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Duration { get; private set; }

	public SpawnWaveEnemiesDefinition WaveEnemiesDefinition { get; private set; }

	public string Id { get; private set; }

	public bool IsBossWave => WaveEnemiesDefinition.BossWaveSettings != null;

	public BossWaveSettings BossWaveSettings => WaveEnemiesDefinition.BossWaveSettings;

	public bool IsInfinite
	{
		get
		{
			if (IsBossWave)
			{
				return WaveEnemiesDefinition.BossWaveSettings.IsInfiniteWave;
			}
			return false;
		}
	}

	public float SpawnsCountMultiplier { get; private set; }

	public Dictionary<int, float> TemporalDistribution { get; private set; } = new Dictionary<int, float>();


	public float TemporalDistributionTotalWeight { get; private set; }

	public SpawnWaveDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"SpawnWaveDefinition has no Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		WaveEnemiesDefinition = new SpawnWaveEnemiesDefinition(container);
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("TemporalDistribution"));
		if (val3 == null)
		{
			CLoggerManager.Log((object)"SpawnWaveDefinition has no TemporalDistribution!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("Turn")))
		{
			XAttribute val4 = item.Attribute(XName.op_Implicit("Id"));
			if (val4.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)"Turn has no Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(val4.Value, out var result))
			{
				CLoggerManager.Log((object)"Turn has an invalid Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (result > Duration)
			{
				Duration = result;
			}
			XAttribute val5 = item.Attribute(XName.op_Implicit("Weight"));
			if (val5.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)"Turn has no Weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!float.TryParse(val5.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				CLoggerManager.Log((object)"Turn has an invalid Weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			TemporalDistribution.Add(result, result2);
			TemporalDistributionTotalWeight += result2;
		}
		float result3 = 1f;
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("SpawnsCountMultiplier"));
		if (!val6.IsNullOrEmpty() && !float.TryParse(val6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result3))
		{
			CLoggerManager.Log((object)"SpawnWaveDefinition has an invalid SpawnsCountMultiplier!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			SpawnsCountMultiplier = result3;
		}
	}
}
