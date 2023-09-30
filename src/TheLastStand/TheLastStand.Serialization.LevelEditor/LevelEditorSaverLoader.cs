using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.TileMap;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Generic;
using TheLastStand.View.LevelEditor;
using UnityEngine;

namespace TheLastStand.Serialization.LevelEditor;

public static class LevelEditorSaverLoader
{
	[Flags]
	public enum E_ImpossibleSaveCause
	{
		None = 0,
		NoMagicCircle = 1,
		TooManyMagicCircles = 2,
		MissingSaveId = 4
	}

	public static string SaveFilePathPrefix => SaveFolderPath + "/" + LevelEditorManager.SaveId + "/" + LevelEditorManager.SaveId;

	public static string SaveFolderPath => "Assets/Resources/TextAssets/Cities/Level Editor";

	public static void Save()
	{
		((MonoBehaviour)TPSingleton<LevelEditorManager>.Instance).StartCoroutine(SaveCoroutine());
	}

	private static IEnumerator SaveCoroutine()
	{
		if (!IsSaveAllowed(out var impossibleSaveCause))
		{
			GenericPopUp.Open("Level Editor Save Error", $"Could not save Level. Failure cause(s) : {impossibleSaveCause}");
			yield break;
		}
		LevelEditorManager.OnSaveBegan();
		int tileIndex = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length - 1;
		while (tileIndex >= 0)
		{
			if (tileIndex % 30 == 0)
			{
				yield return SharedYields.WaitForEndOfFrame;
			}
			TileController tileController = TPSingleton<TileMapManager>.Instance.TileMap.Tiles[tileIndex].TileController;
			tileController.ComputeDistanceToCity();
			tileController.ComputeDistanceToMagicCircle();
			int num = tileIndex - 1;
			tileIndex = num;
		}
		string text = SaveFolderPath + "/" + LevelEditorManager.SaveId;
		if (Directory.Exists(text))
		{
			((CLogger<LevelEditorManager>)TPSingleton<LevelEditorManager>.Instance).Log((object)("Deleting directory " + text + " to overwrite level data files."), (CLogLevel)2, false, false);
			Directory.Delete(text, recursive: true);
		}
		else
		{
			((CLogger<LevelEditorManager>)TPSingleton<LevelEditorManager>.Instance).Log((object)("Creating directory " + text + "."), (CLogLevel)2, false, false);
		}
		Directory.CreateDirectory(text);
		SaveTiles();
		SaveBuildings();
		LevelEditorManager.OnSaveComplete();
		((CLogger<LevelEditorManager>)TPSingleton<LevelEditorManager>.Instance).Log((object)("Successfully saved Level data files at path " + text + "."), (CLogLevel)2, true, false);
	}

	public static void SaveBuildings()
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
		{
			((XNode)SerializeBuildings()).WriteTo(xmlWriter);
		}
		using StreamWriter streamWriter = new StreamWriter(SaveFilePathPrefix + "_Buildings.xml");
		streamWriter.Write(stringBuilder.ToString());
		streamWriter.Close();
	}

	public static void SaveTiles()
	{
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder))
		{
			((XNode)SerializeTiles()).WriteTo(xmlWriter);
		}
		using StreamWriter streamWriter = new StreamWriter(SaveFilePathPrefix + "_TileMap.xml");
		streamWriter.Write(stringBuilder.ToString());
		streamWriter.Close();
	}

	private static bool IsSaveAllowed(out E_ImpossibleSaveCause impossibleSaveCause)
	{
		impossibleSaveCause = E_ImpossibleSaveCause.None;
		if (string.IsNullOrEmpty(LevelEditorManager.SaveId))
		{
			impossibleSaveCause |= E_ImpossibleSaveCause.MissingSaveId;
		}
		int num = 0;
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if (building is MagicCircle)
			{
				num++;
			}
		}
		if (num == 0)
		{
			impossibleSaveCause |= E_ImpossibleSaveCause.NoMagicCircle;
		}
		else if (num > 1)
		{
			impossibleSaveCause |= E_ImpossibleSaveCause.TooManyMagicCircles;
		}
		return impossibleSaveCause == E_ImpossibleSaveCause.None;
	}

	private static XContainer SerializeBuildings()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Expected O, but got Unknown
		XDocument val = new XDocument();
		XElement val2 = new XElement(XName.op_Implicit("Buildings"));
		((XContainer)val).Add((object)val2);
		List<TheLastStand.Model.Building.Building> list = new List<TheLastStand.Model.Building.Building>(TPSingleton<BuildingManager>.Instance.Buildings);
		list.Sort((TheLastStand.Model.Building.Building a, TheLastStand.Model.Building.Building b) => a.BuildingDefinition.Id.CompareTo(b.BuildingDefinition.Id));
		foreach (TheLastStand.Model.Building.Building item in list)
		{
			XElement val3 = new XElement(XName.op_Implicit("Building"));
			((XContainer)val3).Add((object)new XAttribute(XName.op_Implicit("Id"), (object)item.BuildingDefinition.Id));
			((XContainer)val3).Add((object)new XAttribute(XName.op_Implicit("X"), (object)item.OriginTile.X));
			((XContainer)val3).Add((object)new XAttribute(XName.op_Implicit("Y"), (object)item.OriginTile.Y));
			if (TPSingleton<BuildingsSettingsManager>.Instance.BuildingSettingsPanels.TryGetValue(item, out var value))
			{
				if (!value.BuildingSettingsHealth.IsFullHealth)
				{
					((XContainer)val3).Add((object)new XElement(XName.op_Implicit("Health"), (object)value.BuildingSettingsHealth.CurrentHealth));
				}
				if (value.BuildingSettingsUpgradeLevels.Count > 0 && value.BuildingSettingsUpgradeLevels.Any((BuildingSettingsUpgradeLevel o) => o.CurrentUpgradeLevel > 0))
				{
					XElement val4 = new XElement(XName.op_Implicit("UpgradesLevels"));
					foreach (BuildingSettingsUpgradeLevel buildingSettingsUpgradeLevel in value.BuildingSettingsUpgradeLevels)
					{
						if (buildingSettingsUpgradeLevel.CurrentUpgradeLevel > 0)
						{
							((XContainer)val4).Add((object)new XElement(XName.op_Implicit(buildingSettingsUpgradeLevel.BuildingUpgradeDefinition.Id), (object)buildingSettingsUpgradeLevel.CurrentUpgradeLevel));
						}
					}
					((XContainer)val3).Add((object)val4);
				}
			}
			((XContainer)val2).Add((object)val3);
		}
		return (XContainer)(object)val;
	}

	private static XContainer SerializeTiles()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Expected O, but got Unknown
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Expected O, but got Unknown
		XDocument val = new XDocument();
		XElement val2 = new XElement(XName.op_Implicit("TileMap"));
		((XContainer)val).Add((object)val2);
		((XContainer)val2).Add((object)new XElement(XName.op_Implicit("Width"), (object)TPSingleton<TileMapManager>.Instance.TileMap.Width));
		((XContainer)val2).Add((object)new XElement(XName.op_Implicit("Height"), (object)TPSingleton<TileMapManager>.Instance.TileMap.Height));
		XElement val3 = new XElement(XName.op_Implicit("Grounds"));
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int num = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length - 1; num >= 0; num--)
		{
			Tile tile = TPSingleton<TileMapManager>.Instance.TileMap.Tiles[num];
			if (!dictionary.ContainsKey(tile.GroundDefinition.Id))
			{
				dictionary.Add(tile.GroundDefinition.Id, string.Empty);
			}
			dictionary[tile.GroundDefinition.Id] += $"{tile.X},{tile.Y},{tile.DistanceToCity},{tile.DistanceToMagicCircle}|";
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			string text = item.Value[..^1];
			((XContainer)val3).Add((object)new XElement(XName.op_Implicit(item.Key), (object)text));
		}
		if (val3.HasElements)
		{
			((XContainer)val2).Add((object)val3);
		}
		XElement val4 = new XElement(XName.op_Implicit("Flags"));
		foreach (KeyValuePair<TileFlagDefinition.E_TileFlagTag, List<Tile>> item2 in TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag)
		{
			string text2 = string.Empty;
			for (int num2 = item2.Value.Count - 1; num2 >= 0; num2--)
			{
				Tile tile2 = item2.Value[num2];
				text2 += string.Format("{0},{1}{2}", tile2.X, tile2.Y, (num2 > 0) ? "|" : string.Empty);
			}
			if (!string.IsNullOrEmpty(text2))
			{
				((XContainer)val4).Add((object)new XElement(XName.op_Implicit(item2.Key.ToString()), (object)text2));
			}
		}
		if (val4.HasElements)
		{
			((XContainer)val2).Add((object)val4);
		}
		return (XContainer)(object)val;
	}
}
