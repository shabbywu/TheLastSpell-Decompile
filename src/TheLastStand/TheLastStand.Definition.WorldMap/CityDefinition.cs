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

public class CityDefinition : Definition
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
		}
	}

	public bool BlackenBackground { get; private set; }

	public string BonePilesEvolutionId { get; private set; }

	public BrazierDefinition BrazierDefinition { get; private set; }

	public Vector4 CameraBoundaries { get; private set; }

	public string Description => Localizer.Get("WorldMap_CityDescription_" + Id);

	public int EnemiesProgressionOffset { get; private set; }

	public string FogDefinitionId { get; private set; }

	public List<SpawnDirectionsDefinition.E_Direction> ForbiddenDirections { get; private set; }

	public bool Hidden { get; private set; }

	public bool HideGoddesses { get; private set; }

	public string Id { get; private set; }

	public string InitResourceDefinitionId { get; private set; }

	public bool IsLastMap { get; private set; }

	public bool IsTutorialMap { get; private set; }

	public string LevelLayoutBuildingsId { get; private set; }

	public string LevelLayoutTileMapId { get; private set; }

	public string LevelArtPrefabId { get; private set; }

	public LightFogSpawnersGenerationDefinition LightFogSpawnersGenerationDefinition { get; private set; }

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

	public bool UseCommanderAsMage { get; private set; }

	public int VictoryDaysCount { get; private set; }

	public Vector2 WorldMapPosition { get; private set; }

	public CityDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0ac2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c12: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d32: Unknown result type (might be due to invalid IL or missing references)
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
		IsTutorialMap = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("IsTutorialMap")) : null) != null;
		IsLastMap = ((XContainer)val4).Element(XName.op_Implicit("IsLastMap")) != null;
		XElement val7 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("EnemiesProgressionOffset")) : null);
		if (val7 != null)
		{
			if (int.TryParse(val7.Value, out var result))
			{
				EnemiesProgressionOffset = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse EnemiesProgressionOffset element into an int in \"" + Id + "\"'s definition."), (Object)(object)TPSingleton<WorldMapCityManager>.Instance, (LogType)0, (CLogLevel)2, true, "WorldMapCityManager", false);
			}
		}
		XElement val8 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("FogId")) : null);
		if (XDocumentExtensions.IsNullOrEmpty(val8))
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
			FogDefinitionId = val8.Value;
		}
		XElement val9 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("InitResourceId")) : null);
		if (XDocumentExtensions.IsNullOrEmpty(val9))
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
			InitResourceDefinitionId = val9.Value;
		}
		XElement val10 = ((XContainer)val5).Element(XName.op_Implicit("LevelLayoutIds"));
		if (val10 != null)
		{
			XElement val11 = ((XContainer)val10).Element(XName.op_Implicit("Buildings"));
			LevelLayoutBuildingsId = ((!XDocumentExtensions.IsNullOrEmpty(val11)) ? val11.Value : Id);
			XElement val12 = ((XContainer)val10).Element(XName.op_Implicit("TileMap"));
			LevelLayoutTileMapId = ((!XDocumentExtensions.IsNullOrEmpty(val12)) ? val12.Value : Id);
			XElement val13 = ((XContainer)val10).Element(XName.op_Implicit("LevelArtPrefabId"));
			LevelArtPrefabId = ((!XDocumentExtensions.IsNullOrEmpty(val13)) ? val13.Value : Id);
		}
		else
		{
			LevelLayoutBuildingsId = Id;
			LevelLayoutTileMapId = Id;
			LevelArtPrefabId = Id;
		}
		XElement val14 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("LightFogSpawnersGenerationId")) : null);
		if (val14 != null && FogDatabase.LightFogDefinition.LightFogSpawnersGenerationDefinitions.TryGetValue(val14.Value, out var value2))
		{
			LightFogSpawnersGenerationDefinition = value2;
		}
		else
		{
			LightFogSpawnersGenerationDefinition = new LightFogSpawnersGenerationDefinition(null, ((Definition)FogDatabase.LightFogDefinition).TokenVariables);
		}
		XElement val15 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("MaxGlyphPoints")) : null);
		if (val15 != null)
		{
			if (int.TryParse(val15.Value, out var result2))
			{
				MaxGlyphPoints = result2;
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
		XElement val16 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("PanicRewardOffsets")) : null);
		if (val16 != null)
		{
			XElement val17 = ((XContainer)val16).Element(XName.op_Implicit("Resources"));
			if (val17 != null)
			{
				if (int.TryParse(val17.Value, out var result3))
				{
					PanicRewardResourcesOffset = result3;
				}
				else
				{
					CLoggerManager.Log((object)("Could not parse PanicRewardOffsets Resources element into an int in \"" + Id + "\"'s definition."), (Object)(object)TPSingleton<WorldMapCityManager>.Instance, (LogType)0, (CLogLevel)2, true, "WorldMapCityManager", false);
				}
			}
			XElement val18 = ((XContainer)val16).Element(XName.op_Implicit("Items"));
			if (val18 != null)
			{
				if (int.TryParse(val18.Value, out var result4))
				{
					PanicRewardItemsOffset = result4;
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
		XElement val19 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("SectorsId")) : null);
		if (XDocumentExtensions.IsNullOrEmpty(val19))
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
			SectorContainerPrefabId = val19.Value;
		}
		XElement val20 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("BonePilesEvolutionId")) : null);
		if (val20 != null)
		{
			BonePilesEvolutionId = val20.Value;
		}
		else if (cityDefinition != null)
		{
			BonePilesEvolutionId = cityDefinition.BonePilesEvolutionId;
		}
		else
		{
			CLoggerManager.Log((object)("CityDefinition " + Id + " has no BonePilesEvolutionId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XElement val21 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("RandomBuildingsPerDayDefinitionId")) : null);
		if (val21 != null)
		{
			RandomBuildingsPerDayDefinitionId = val21.Value;
		}
		else if (cityDefinition != null)
		{
			RandomBuildingsPerDayDefinitionId = cityDefinition.RandomBuildingsPerDayDefinitionId;
		}
		XElement val22 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("ShopEvolutionId")) : null);
		if (val22 != null)
		{
			ShopEvolutionId = val22.Value;
		}
		else if (cityDefinition != null)
		{
			ShopEvolutionId = cityDefinition.ShopEvolutionId;
		}
		else
		{
			CLoggerManager.Log((object)("CityDefinition " + Id + " has no ShopEvolutionId and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XElement val23 = ((val4 != null) ? ((XContainer)val4).Element(XName.op_Implicit("Spawn")) : null);
		XAttribute val24 = val23.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val24))
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
			SpawnDefinitionId = val24.Value;
		}
		ForbiddenDirections = new List<SpawnDirectionsDefinition.E_Direction>();
		XElement val25 = ((XContainer)val23).Element(XName.op_Implicit("ForbiddenDirections"));
		if (XDocumentExtensions.IsNullOrEmpty(val25))
		{
			if (cityDefinition != null)
			{
				ForbiddenDirections = cityDefinition.ForbiddenDirections;
			}
		}
		else
		{
			foreach (XElement item in ((XContainer)val25).Elements(XName.op_Implicit("ForbiddenDirection")))
			{
				if (!Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item.Value, out var result5))
				{
					CLoggerManager.Log((object)("Could not parse Forbidden Direction " + item.Value + " of city " + Id + " as a valid Direction!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else if (ForbiddenDirections.Contains(result5))
				{
					CLoggerManager.Log((object)("Forbidden Direction " + item.Value + " of city " + Id + " is set twice!"), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					ForbiddenDirections.Add(result5);
				}
			}
		}
		XElement val26 = ((XContainer)val4).Element(XName.op_Implicit("StartingDayTurn"));
		Game.E_DayTurn result6;
		if (val26 == null)
		{
			StartingDayTurn = cityDefinition?.StartingDayTurn ?? Game.E_DayTurn.Deployment;
		}
		else if (Enum.TryParse<Game.E_DayTurn>(val26.Value, out result6))
		{
			StartingDayTurn = result6;
		}
		else
		{
			CLoggerManager.Log((object)$"Could not parse StartingDayTurn element {val26.Value} of city {Id} as a valid DayTurn! Setting it to {Game.E_DayTurn.Deployment}.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			StartingDayTurn = Game.E_DayTurn.Deployment;
		}
		XElement val27 = ((XContainer)val5).Element(XName.op_Implicit("StartingSetup"));
		if (XDocumentExtensions.IsNullOrEmpty(val27))
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
			StartingSetup = val27.Value;
		}
		XElement val28 = ((XContainer)val4).Element(XName.op_Implicit("UnitGenerationId"));
		if (XDocumentExtensions.IsNullOrEmpty(val28))
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
			UnitGenerationDefinitionId = val28.Value;
		}
		XElement val29 = ((XContainer)val4).Element(XName.op_Implicit("VictoryDaysCount"));
		if (XDocumentExtensions.IsNullOrEmpty(val29))
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
			if (!int.TryParse(val29.Value, out var result7))
			{
				CLoggerManager.Log((object)("Could not parse VictoryDaysCount from CityDefinition " + Id + " value " + val29.Value + " to a valid integer value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			if (result7 < 1)
			{
				CLoggerManager.Log((object)$"VictoryDaysCount in CityDefinition {Id} is less than 1 ({result7}) which is invalid, setting it to 1.", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
				result7 = 1;
			}
			VictoryDaysCount = result7;
		}
		XElement obj = ((XContainer)val5).Element(XName.op_Implicit("WorldMapPosition"));
		XAttribute val30 = obj.Attribute(XName.op_Implicit("X"));
		XAttribute val31 = obj.Attribute(XName.op_Implicit("Y"));
		if (float.TryParse(val30.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result8) && float.TryParse(val31.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result9))
		{
			WorldMapPosition = new Vector2(result8, result9);
		}
		CameraBoundaries = default(Vector4);
		XElement val32 = ((XContainer)val5).Element(XName.op_Implicit("CameraBoundaries"));
		if (XDocumentExtensions.IsNullOrEmpty(val32))
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
			float result10 = 0f;
			float result11 = 0f;
			float result12 = 0f;
			float result13 = 0f;
			XElement val33 = ((XContainer)val32).Element(XName.op_Implicit("Top"));
			if (XDocumentExtensions.IsNullOrEmpty(val33))
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Top and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result10 = cityDefinition.CameraBoundaries.x;
			}
			else if (!float.TryParse(val33.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result10))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Top Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			XElement val34 = ((XContainer)val32).Element(XName.op_Implicit("Bottom"));
			if (XDocumentExtensions.IsNullOrEmpty(val34))
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Bottom and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result11 = cityDefinition.CameraBoundaries.y;
			}
			else if (!float.TryParse(val34.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result11))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Bottom Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			XElement val35 = ((XContainer)val32).Element(XName.op_Implicit("Left"));
			if (XDocumentExtensions.IsNullOrEmpty(val35))
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Left and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result12 = cityDefinition.CameraBoundaries.z;
			}
			else if (!float.TryParse(val35.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result12))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Left Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			XElement val36 = ((XContainer)val32).Element(XName.op_Implicit("Right"));
			if (XDocumentExtensions.IsNullOrEmpty(val36))
			{
				if (cityDefinition == null)
				{
					CLoggerManager.Log((object)("CityDefinition " + Id + " has no CameraBoundaries.Right and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				result13 = cityDefinition.CameraBoundaries.w;
			}
			else if (!float.TryParse(val36.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result13))
			{
				CLoggerManager.Log((object)("CityDefinition " + Id + " has an invalid value from CameraBoundaries.Right Element !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			CameraBoundaries = new Vector4(result10, result11, result12, result13);
		}
		BlackenBackground = ((XContainer)val5).Element(XName.op_Implicit("BlackenBackground")) != null;
		XElement val37 = ((XContainer)val5).Element(XName.op_Implicit("AnimatedCutscenes"));
		if (!XDocumentExtensions.IsNullOrEmpty(val37))
		{
			object preGameCutscene;
			if (val37 == null)
			{
				preGameCutscene = null;
			}
			else
			{
				XElement obj2 = ((XContainer)val37).Element(XName.op_Implicit("PreGameCutscene"));
				preGameCutscene = ((obj2 != null) ? obj2.Value : null);
			}
			PreGameCutscene = (string)preGameCutscene;
			object postVictoryCutscene;
			if (val37 == null)
			{
				postVictoryCutscene = null;
			}
			else
			{
				XElement obj3 = ((XContainer)val37).Element(XName.op_Implicit("PostVictoryCutscene"));
				postVictoryCutscene = ((obj3 != null) ? obj3.Value : null);
			}
			PostVictoryCutscene = (string)postVictoryCutscene;
		}
		HideGoddesses = ((XContainer)val5).Element(XName.op_Implicit("HideGoddesses")) != null;
		UseCommanderAsMage = ((XContainer)val5).Element(XName.op_Implicit("UseCommanderAsMage")) != null;
	}
}
