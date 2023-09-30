using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ApocalypseEffectDefinition : Definition
{
	public static class Consts
	{
		public const string EnemiesStatModifier = "EnemiesStatModifier";

		public const string GenerateFogSpawners = "GenerateFogSpawners";

		public const string GenerateMalusAffixes = "GenerateMalusAffixes";

		public const string IncreaseEnemiesNumber = "IncreaseEnemiesNumber";

		public const string IncreasePrices = "IncreasePrices";

		public const string IncreaseStartingFogDensity = "IncreaseStartingFogDensity";

		public const string IncreaseDailyFogUpdateFrequency = "IncreaseDailyFogUpdateFrequency";
	}

	public ApocalypseEffectDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
