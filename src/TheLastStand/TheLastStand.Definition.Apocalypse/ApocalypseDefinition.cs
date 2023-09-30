using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseDefinition : Definition
{
	public List<ApocalypseEffectDefinition> Effects { get; private set; } = new List<ApocalypseEffectDefinition>();


	public int Id { get; private set; }

	public ApocalypseDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (!int.TryParse(val2.Value, out var result))
		{
			CLoggerManager.Log((object)("An apocalypse's Id " + ((Definition)this).HasAnInvalidInt(val2.Value) + " !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = result;
		foreach (XElement item8 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("Effects"))).Elements())
		{
			if (item8.Name.LocalName == "EnemiesStatModifier")
			{
				EnemiesStatModifierApocalyseEffectDefinition item = new EnemiesStatModifierApocalyseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item);
			}
			else if (item8.Name.LocalName == "GenerateFogSpawners")
			{
				GenerateFogSpawnersApocalypseEffectDefinition item2 = new GenerateFogSpawnersApocalypseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item2);
			}
			else if (item8.Name.LocalName == "GenerateMalusAffixes")
			{
				GenerateMalusAffixesApocalypseEffectDefinition item3 = new GenerateMalusAffixesApocalypseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item3);
			}
			else if (item8.Name.LocalName == "IncreaseEnemiesNumber")
			{
				IncreaseEnemiesNumberApocalypseEffectDefinition item4 = new IncreaseEnemiesNumberApocalypseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item4);
			}
			else if (item8.Name.LocalName == "IncreasePrices")
			{
				IncreasePricesApocalypseEffectDefinition item5 = new IncreasePricesApocalypseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item5);
			}
			else if (item8.Name.LocalName == "IncreaseStartingFogDensity")
			{
				IncreaseStartingFogDensityApocalypseEffectDefinition item6 = new IncreaseStartingFogDensityApocalypseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item6);
			}
			else if (item8.Name.LocalName == "IncreaseDailyFogUpdateFrequency")
			{
				IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition item7 = new IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition((XContainer)(object)item8);
				Effects.Add(item7);
			}
			else
			{
				CLoggerManager.Log((object)("Unhandled Apocalypse effect name " + item8.Name.LocalName + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
