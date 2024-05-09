using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Database.Fog;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.Apocalypse.LightFogSpawner;
using TheLastStand.Definition.Brazier;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.WorldMap;

public class CityDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public static class CityIds
		{
			public const string Swampfurt = "TutorialMap";

			public const string Gildenberg = "Felderland";

			public const string Lakeburg = "LakeBurg";

			public const string Glenwald = "Glenwald";

			public const string Elderlicht = "Elderlicht";

			public const string Glintfein = "Glintfein";

			public const string Runenberg = "GildenbergRedux";
		}

		public const string CityMetaUnlockName = "Unlock{0}City";

		public const float WorldMapZoomedCityOffsetX = 4.392857f;
	}

	public bool BlackenBackground { get; private set; }

	public string BonePilesEvolutionId { get; private set; }

	public BrazierDefinition BrazierDefinition { get; private set; }

	public Vector4 CameraBoundaries { get; private set; }

	public string Description => Localizer.Get("WorldMap_CityDescription_" + Id);

	public int DifficultySkullsNb { get; private set; }

	public int EnemiesProgressionOffset { get; private set; }

	public string FogDefinitionId { get; private set; }

	public List<SpawnDirectionsDefinition.E_Direction> ForbiddenDirections { get; private set; }

	public bool HasLinkedCity => !string.IsNullOrEmpty(LinkedCityId);

	public bool HasLinkedDLC => !string.IsNullOrEmpty(LinkedDLCId);

	public bool Hidden { get; private set; }

	public bool HideGoddesses { get; private set; }

	public string Id { get; private set; }

	public string InitResourceDefinitionId { get; private set; }

	public bool IsLastMap { get; private set; }

	public bool IsStoryMap { get; private set; }

	public bool IsTutorialMap { get; private set; }

	public string LevelLayoutBuildingsId { get; private set; }

	public string LevelLayoutTileMapId { get; private set; }

	public string LevelArtPrefabId { get; private set; }

	public LightFogSpawnersGenerationDefinition LightFogSpawnersGenerationDefinition { get; private set; }

	public string LinkedCityId { get; private set; }

	public string LinkedDLCId { get; private set; }

	public int MaxGlyphPoints { get; private set; }

	public string Name => Localizer.Get("WorldMap_CityName_" + Id);

	public int PanicRewardItemsOffset { get; private set; }

	public int PanicRewardResourcesOffset { get; private set; }

	public string PreGameCutscene { get; private set; }

	public string PostVictoryCutscene { get; private set; }

	public string RandomBuildingsPerDayDefinitionId { get; private set; }

	public string SectorContainerPrefabId { get; private set; }

	public string ShopEvolutionId { get; private set; }

	public string SpawnDefinitionId { get; private set; }

	public Game.E_DayTurn StartingDayTurn { get; private set; }

	public string StartingSetup { get; private set; }

	public string UnitGenerationDefinitionId { get; private set; }

	public string UnitGenerationGuaranteedRaceId { get; private set; }

	public bool UseCommanderAsMage { get; private set; }

	public int VictoryDaysCount { get; private set; }

	public Vector2 WorldMapPosition { get; private set; }

	public CityDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0bcd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e3d: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		CityDefinition cityDefinition = null;
		Hidden = ((XContainer)val).Element(XName.op_Implicit("Hidden")) != null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val3 != null)
		{
			cityDefinition = CityDatabase.CityDefinitions[val3.Value];
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Gameplay"));
		if (val4 == null && cityDefinition == null)
		{
			CLoggerManager.Log((object)("CityDefinition " + Id + " must have a Gameplay element since it has no template."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("View"));
		if (val5 == null && cityDefinition == null)
		{
			CLoggerManager.Log((object)("CityDefinition " + Id + " must have a View element since it has no template."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XElement val6 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("BrazierDefinitionId")) : null);
		if (val6 != null)
		{
			if (BuildingDatabase.BraziersDefinition.BrazierDefinitions.TryGetValue(val6.Value, out var value))
			{
				BrazierDefinition = value;
			}
			else
			{
				CLoggerManager.Log((object)("Could not find Brazier Definition " + val6.Value + " in database."), (LogType)0, (CLogLevel)2, true, "CityDefinition", false);
			}
		}
		else if (cityDefinition != null)
		{
			BrazierDefinition = cityDefinition.BrazierDefinition;
		}
		XElement val7 = ((XContainer)val5).Element(XName.op_Implicit("DifficultySkullsNb"));
		if (val7 != null)
		{
			if (int.TryParse(val7.Value, out var result))
			{
				DifficultySkullsNb = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse DifficultySkullNb element into an int in \"" + Id + "\"'s definition."), (LogType)0, (CLogLevel)2, true, "CityDefinition", false);
			}
		}
		XElement val8 = ((XContainer)val5).Element(XName.op_Implicit("IsStoryMap"));
		if (val8 != null)
		{
			if (!bool.TryParse(val8.Value, out var result2))
			{
				CLoggerManager.Log((object)"Could not parse CityDefinition IsStoryMap value to a valid bool.", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				IsStoryMap = false;
			}
			else
			{
				IsStoryMap = result2;
			}
		}
		IsTutorialMap = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("IsTutorialMap")) : null) != null;
		IsLastMap = ((XContainer)val4).Element(XName.op_Implicit("IsLastMap")) != null;
		XElement val9 = ((XContainer)val5).Element(XName.op_Implicit("LinkedCityId"));
		if (val9 != null)
		{
			LinkedCityId = val9.Value;
		}
		XElement val10 = ((XContainer)val5).Element(XName.op_Implicit("LinkedDLCId"));
		if (val10 != null)
		{
			LinkedDLCId = val10.Value;
		}
		XElement val11 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("EnemiesProgressionOffset")) : null);
		if (val11 != null)
		{
			if (int.TryParse(val11.Value, out var result3))
			{
				EnemiesProgressionOffset = result3;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse EnemiesProgressionOffset element into an int in \"" + Id + "\"'s definition."), (Object)(object)TPSingleton<WorldMapCityManager>.Instance, (LogType)0, (CLogLevel)2, true, "WorldMapCityManager", false);
			}
		}
		XElement val12 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("FogId")) : null);
		if (val12.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no FogId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			FogDefinitionId = cityDefinition.FogDefinitionId;
		}
		else
		{
			FogDefinitionId = val12.Value;
		}
		XElement val13 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("InitResourceId")) : null);
		if (val13.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no InitResourceDefinitionId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			InitResourceDefinitionId = cityDefinition.InitResourceDefinitionId;
		}
		else
		{
			InitResourceDefinitionId = val13.Value;
		}
		XElement val14 = ((XContainer)val5).Element(XName.op_Implicit("LevelLayoutIds"));
		if (val14 != null)
		{
			XElement val15 = ((XContainer)val14).Element(XName.op_Implicit("Buildings"));
			LevelLayoutBuildingsId = ((!val15.IsNullOrEmpty()) ? val15.Value : Id);
			XElement val16 = ((XContainer)val14).Element(XName.op_Implicit("TileMap"));
			LevelLayoutTileMapId = ((!val16.IsNullOrEmpty()) ? val16.Value : Id);
			XElement val17 = ((XContainer)val14).Element(XName.op_Implicit("LevelArtPrefabId"));
			LevelArtPrefabId = ((!val17.IsNullOrEmpty()) ? val17.Value : Id);
		}
		else
		{
			LevelLayoutBuildingsId = Id;
			LevelLayoutTileMapId = Id;
			LevelArtPrefabId = Id;
		}
		XElement val18 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("LightFogSpawnersGenerationId")) : null);
		if (val18 != null && FogDatabase.LightFogDefinition.LightFogSpawnersGenerationDefinitions.TryGetValue(val18.Value, out var value2))
		{
			LightFogSpawnersGenerationDefinition = value2;
		}
		else
		{
			LightFogSpawnersGenerationDefinition = new LightFogSpawnersGenerationDefinition(null, FogDatabase.LightFogDefinition.TokenVariables);
		}
		XElement val19 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("MaxGlyphPoints")) : null);
		if (val19 != null)
		{
			if (int.TryParse(val19.Value, out var result4))
			{
				MaxGlyphPoints = result4;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse MaxGlyphPoints element into an int in \"" + Id + "\"'s definition."), (Object)(object)TPSingleton<WorldMapCityManager>.Instance, (LogType)0, (CLogLevel)2, true, "WorldMapCityManager", false);
			}
		}
		else if (cityDefinition != null)
		{
			MaxGlyphPoints = cityDefinition.MaxGlyphPoints;
		}
		XElement val20 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("PanicRewardOffsets")) : null);
		if (val20 != null)
		{
			XElement val21 = ((XContainer)val20).Element(XName.op_Implicit("Resources"));
			if (val21 != null)
			{
				if (int.TryParse(val21.Value, out var result5))
				{
					PanicRewardResourcesOffset = result5;
				}
				else
				{
					CLoggerManager.Log((object)("Could not parse PanicRewardOffsets Resources element into an int in \"" + Id + "\"'s definition."), (Object)(object)TPSingleton<WorldMapCityManager>.Instance, (LogType)0, (CLogLevel)2, true, "WorldMapCityManager", false);
				}
			}
			XElement val22 = ((XContainer)val20).Element(XName.op_Implicit("Items"));
			if (val22 != null)
			{
				if (int.TryParse(val22.Value, out var result6))
				{
					PanicRewardItemsOffset = result6;
				}
				else
				{
					CLoggerManager.Log((object)("Could not parse PanicRewardOffsets Items element into an int in \"" + Id + "\"'s definition."), (Object)(object)TPSingleton<WorldMapCityManager>.Instance, (LogType)0, (CLogLevel)2, true, "WorldMapCityManager", false);
				}
			}
		}
		else if (cityDefinition != null)
		{
			PanicRewardResourcesOffset = cityDefinition.PanicRewardResourcesOffset;
			PanicRewardItemsOffset = cityDefinition.PanicRewardItemsOffset;
		}
		XElement val23 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("SectorsId")) : null);
		if (val23.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no SectorsId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SectorContainerPrefabId = cityDefinition.SectorContainerPrefabId;
		}
		else
		{
			SectorContainerPrefabId = val23.Value;
		}
		XElement val24 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("BonePilesEvolutionId")) : null);
		if (val24 != null)
		{
			BonePilesEvolutionId = val24.Value;
		}
		else if (cityDefinition != null)
		{
			BonePilesEvolutionId = cityDefinition.BonePilesEvolutionId;
		}
		else
		{
			CLoggerManager.Log((object)("CityDefinition " + Id + " has no BonePilesEvolutionId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XElement val25 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("RandomBuildingsPerDayDefinitionId")) : null);
		if (val25 != null)
		{
			RandomBuildingsPerDayDefinitionId = val25.Value;
		}
		else if (cityDefinition != null)
		{
			RandomBuildingsPerDayDefinitionId = cityDefinition.RandomBuildingsPerDayDefinitionId;
		}
		XElement val26 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("ShopEvolutionId")) : null);
		if (val26 != null)
		{
			ShopEvolutionId = val26.Value;
		}
		else if (cityDefinition != null)
		{
			ShopEvolutionId = cityDefinition.ShopEvolutionId;
		}
		else
		{
			CLoggerManager.Log((object)("CityDefinition " + Id + " has no ShopEvolutionId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XElement val27 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("Spawn")) : null);
		XAttribute val28 = val27.Attribute(XName.op_Implicit("Id"));
		if (val28.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no SpawnId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnDefinitionId = cityDefinition.SpawnDefinitionId;
		}
		else
		{
			SpawnDefinitionId = val28.Value;
		}
		ForbiddenDirections = new List<SpawnDirectionsDefinition.E_Direction>();
		XElement val29 = ((XContainer)val27).Element(XName.op_Implicit("ForbiddenDirections"));
		if (val29.IsNullOrEmpty())
		{
			if (cityDefinition != null)
			{
				ForbiddenDirections = cityDefinition.ForbiddenDirections;
			}
		}
		else
		{
			foreach (XElement item in ((XContainer)val29).Elements(XName.op_Implicit("ForbiddenDirection")))
			{
				if (!Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item.Value, out var result7))
				{
					CLoggerManager.Log((object)("Could not parse Forbidden Direction " + item.Value + " of city " + Id + " as a valid Direction!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else if (ForbiddenDirections.Contains(result7))
				{
					CLoggerManager.Log((object)("Forbidden Direction " + item.Value + " of city " + Id + " is set twice!"), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					ForbiddenDirections.Add(result7);
				}
			}
		}
		XElement val30 = ((XContainer)val4).Element(XName.op_Implicit("StartingDayTurn"));
		Game.E_DayTurn result8;
		if (val30 == null)
		{
			StartingDayTurn = cityDefinition?.StartingDayTurn ?? Game.E_DayTurn.Deployment;
		}
		else if (Enum.TryParse<Game.E_DayTurn>(val30.Value, out result8))
		{
			StartingDayTurn = result8;
		}
		else
		{
			CLoggerManager.Log((object)$"Could not parse StartingDayTurn element {val30.Value} of city {Id} as a valid DayTurn! Setting it to {Game.E_DayTurn.Deployment}.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			StartingDayTurn = Game.E_DayTurn.Deployment;
		}
		XElement val31 = ((XContainer)val5).Element(XName.op_Implicit("StartingSetup"));
		if (val31.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no StartingSetup and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			StartingSetup = cityDefinition.StartingSetup;
		}
		else
		{
			StartingSetup = val31.Value;
		}
		XElement val32 = ((XContainer)val4).Element(XName.op_Implicit("UnitGenerationId"));
		if (val32.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no UnitGenerationId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			UnitGenerationDefinitionId = cityDefinition.UnitGenerationDefinitionId;
		}
		else
		{
			UnitGenerationDefinitionId = val32.Value;
		}
		XElement val33 = ((XContainer)val4).Element(XName.op_Implicit("UnitGenerationGuaranteedRaceId"));
		if (!val33.IsNullOrEmpty())
		{
			UnitGenerationGuaranteedRaceId = val33.Value;
		}
		XElement val34 = ((XContainer)val4).Element(XName.op_Implicit("VictoryDaysCount"));
		if (val34.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no VictoryDaysCount and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			VictoryDaysCount = cityDefinition.VictoryDaysCount;
		}
		else
		{
			if (!int.TryParse(val34.Value, out var result9))
			{
				CLoggerManager.Log((object)("Could not parse VictoryDaysCount from CityDefinition " + Id + " value " + val34.Value + " to a valid integer value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			if (result9 < 1)
			{
				CLoggerManager.Log((object)$"VictoryDaysCount in CityDefinition {Id} is less than 1 ({result9}) which is invalid, setting it to 1.", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
				result9 = 1;
			}
			VictoryDaysCount = result9;
		}
		XElement obj = ((XContainer)val5).Element(XName.op_Implicit("WorldMapPosition"));
		XAttribute val35 = obj.Attribute(XName.op_Implicit("X"));
		XAttribute val36 = obj.Attribute(XName.op_Implicit("Y"));
		if (float.TryParse(val35.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10) && float.TryParse(val36.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result11))
		{
			WorldMapPosition = new Vector2(result10, result11);
		}
		CameraBoundaries = default(Vector4);
		XElement val37 = ((XContainer)val5).Element(XName.op_Implicit("CameraBoundaries"));
		if (val37.IsNullOrEmpty())
		{
			if (cityDefinition == null)
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			CameraBoundaries = cityDefinition.CameraBoundaries;
		}
		else
		{
			float result12 = 0f;
			float result13 = 0f;
			float result14 = 0f;
			float result15 = 0f;
			XElement val38 = ((XContainer)val37).Element(XName.op_Implicit("Top"));
			if (val38.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Top and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result12 = cityDefinition.CameraBoundaries.x;
			}
			else if (!float.TryParse(val38.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result12))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Top Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			XElement val39 = ((XContainer)val37).Element(XName.op_Implicit("Bottom"));
			if (val39.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Bottom and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result13 = cityDefinition.CameraBoundaries.y;
			}
			else if (!float.TryParse(val39.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result13))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Bottom Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			XElement val40 = ((XContainer)val37).Element(XName.op_Implicit("Left"));
			if (val40.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Left and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result14 = cityDefinition.CameraBoundaries.z;
			}
			else if (!float.TryParse(val40.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result14))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Left Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			XElement val41 = ((XContainer)val37).Element(XName.op_Implicit("Right"));
			if (val41.IsNullOrEmpty())
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Right and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result15 = cityDefinition.CameraBoundaries.w;
			}
			else if (!float.TryParse(val41.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result15))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Right Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			CameraBoundaries = new Vector4(result12, result13, result14, result15);
		}
		BlackenBackground = ((XContainer)val5).Element(XName.op_Implicit("BlackenBackground")) != null;
		XElement val42 = ((XContainer)val5).Element(XName.op_Implicit("AnimatedCutscenes"));
		if (!val42.IsNullOrEmpty())
		{
			object preGameCutscene;
			if (val42 == null)
			{
				preGameCutscene = null;
			}
			else
			{
				XElement obj2 = ((XContainer)val42).Element(XName.op_Implicit("PreGameCutscene"));
				preGameCutscene = ((obj2 != null) ? obj2.Value : null);
			}
			PreGameCutscene = (string)preGameCutscene;
			object postVictoryCutscene;
			if (val42 == null)
			{
				postVictoryCutscene = null;
			}
			else
			{
				XElement obj3 = ((XContainer)val42).Element(XName.op_Implicit("PostVictoryCutscene"));
				postVictoryCutscene = ((obj3 != null) ? obj3.Value : null);
			}
			PostVictoryCutscene = (string)postVictoryCutscene;
		}
		HideGoddesses = ((XContainer)val5).Element(XName.op_Implicit("HideGoddesses")) != null;
		UseCommanderAsMage = ((XContainer)val5).Element(XName.op_Implicit("UseCommanderAsMage")) != null;
	}
}
