using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class PlayableUnitTemplateDefinition : UnitTemplateDefinition
{
	public override Tile.E_UnitAccess UnitAccessNeeded => Tile.E_UnitAccess.Hero;

	public PlayableUnitTemplateDefinition(XContainer container)
		: base(container)
	{
		base.UnitType = DamageableType.Playable;
	}

	protected override bool CanStopOnSingleTile(Tile tile, TheLastStand.Model.Unit.Unit unit = null)
	{
		if (!base.CanStopOnSingleTile(tile, unit))
		{
			return false;
		}
		TheLastStand.Model.Unit.Unit unit2 = tile.Unit;
		if (unit2 != null && !unit2.IsDead)
		{
			return false;
		}
		return true;
	}

	public override bool CanTravelThrough(Tile tile, E_MoveMethod moveMethod, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (!base.CanTravelThrough(tile, moveMethod, ignoreUnits, ignoreBuildings))
		{
			return false;
		}
		switch (moveMethod)
		{
		case E_MoveMethod.Walking:
			if (!ignoreUnits && tile.Unit is EnemyUnit && !tile.Unit.IsDead)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		DeserializeInjuries((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Injuries")));
	}

	public override void DeserializeInjuries(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		base.InjuryDefinitions = new List<InjuryDefinition>();
		XAttribute val2 = val.Attribute(XName.op_Implicit("BaseHealth"));
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log((object)("Could not parse Injuries BaseHealth attribute value " + val2.Value + " to a valid float."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("Injury")))
		{
			InjuryDefinition item = new InjuryDefinition((XContainer)(object)item2, result);
			base.InjuryDefinitions.Add(item);
		}
	}
}
