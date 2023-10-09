using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BlueprintModuleDefinition : BuildingModuleDefinition, ITileObjectDefinition
{
	public bool BlockFlying { get; private set; }

	public BuildingDefinition.E_BuildingCategory Category { get; private set; }

	public Vector2 HUDOffset { get; private set; }

	public int OriginX { get; private set; }

	public int OriginY { get; private set; }

	public string ShadowType { get; private set; } = "TilingShadow";


	public string SidewalkType { get; private set; } = "Sidewalk";


	public List<List<Tile.E_UnitAccess>> Tiles { get; private set; }

	public BlueprintModuleDefinition(BuildingDefinition buildingDefinition, XContainer blueprintDefinition)
		: base(buildingDefinition, blueprintDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Tiles"));
		Tiles = new List<List<Tile.E_UnitAccess>>();
		string[] array = val2.Value.Split(new char[1] { '\n' });
		for (int num = array.Length - 1; num >= 0; num--)
		{
			array[num] = TPHelpers.RemoveWhitespace(array[num]);
			if (array[num] != string.Empty)
			{
				Tiles.Add(new List<Tile.E_UnitAccess>(array[num].Length));
				for (int i = 0; i < array[num].Length; i++)
				{
					char tileChar = array[num][i];
					Tiles[Tiles.Count - 1].Add(Tile.CharToUnitAccess(tileChar));
				}
			}
		}
		OriginX = ((val2.Attribute(XName.op_Implicit("OriginX")) != null) ? int.Parse(val2.Attribute(XName.op_Implicit("OriginX")).Value) : 0);
		OriginY = ((val2.Attribute(XName.op_Implicit("OriginY")) != null) ? int.Parse(val2.Attribute(XName.op_Implicit("OriginY")).Value) : 0);
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("Category"));
		string text = ((obj != null) ? obj.Value : null);
		if (text != null)
		{
			if (Enum.TryParse<BuildingDefinition.E_BuildingCategory>(text, out var result))
			{
				Category = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse the Category element in " + BuildingDefinition.Id + " into an enum : " + text + "."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else
		{
			Category = ((BuildingDefinition.ConstructionModuleDefinition.NativeMaterialsCost > 0) ? BuildingDefinition.E_BuildingCategory.Defensive : ((BuildingDefinition.ConstructionModuleDefinition.NativeGoldCost > 0) ? BuildingDefinition.E_BuildingCategory.Production : BuildingDefinition.E_BuildingCategory.None));
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("ShadowType"));
		if (val3 != null)
		{
			if (val3.IsEmpty)
			{
				Debug.LogError((object)("BuildingDefinition " + BuildingDefinition.Id + " must have a valid ShadowType!"));
				return;
			}
			ShadowType = val3.Value;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("SidewalkType"));
		if (!val4.IsNullOrEmpty())
		{
			SidewalkType = val4.Value;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("BuildingHUDOffset"));
		if (val5 != null)
		{
			XAttribute val6 = val5.Attribute(XName.op_Implicit("X"));
			XAttribute val7 = val5.Attribute(XName.op_Implicit("Y"));
			HUDOffset = new Vector2((float)int.Parse(val6.Value), (float)int.Parse(val7.Value));
		}
		else
		{
			HUDOffset = Vector2.zero;
		}
		BlockFlying = ((XContainer)val).Element(XName.op_Implicit("BlockFlying")) != null;
	}
}
