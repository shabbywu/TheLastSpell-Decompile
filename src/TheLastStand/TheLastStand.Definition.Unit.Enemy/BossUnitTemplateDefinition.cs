using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class BossUnitTemplateDefinition : EnemyUnitTemplateDefinition
{
	public string DeathCutsceneId { get; private set; }

	public bool AlwaysPlayDeathCutscene { get; private set; }

	public BossUnitTemplateDefinition(XContainer container)
		: base(container)
	{
		base.UnitType = DamageableType.Boss;
	}

	protected override bool CanSpawnOnSingleTile(Tile tile, bool isPhaseActor = false, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (base.MoveMethod == E_MoveMethod.Walking && !tile.GroundDefinition.IsCrossable)
		{
			return false;
		}
		if ((ignoreBuildings || tile.Building == null) && (ignoreUnits || tile.Unit == null) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
		{
			return false;
		}
		if (!ignoreUnits && tile.Unit != null && (!isPhaseActor || tile.Unit is EnemyUnit { IsBossPhaseActor: not false }))
		{
			return false;
		}
		return true;
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BossUnitTemplateDefinition value = null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val2 != null && !BossUnitDatabase.BossUnitTemplateDefinitions.TryGetValue(val2.Value, out value))
		{
			CLoggerManager.Log((object)("BossUnit " + base.Id + " could not find template with Id " + val2.Value + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("DeathCutscene"));
		if (val3 != null)
		{
			XAttribute val4 = val3.Attribute(XName.op_Implicit("Id"));
			XAttribute val5 = val3.Attribute(XName.op_Implicit("AlwaysPlay"));
			DeathCutsceneId = val4.Value;
			if (val5 != null)
			{
				if (bool.TryParse(val5.Value, out var result))
				{
					AlwaysPlayDeathCutscene = result;
					return;
				}
				CLoggerManager.Log((object)("AlwaysPlayCutscene " + base.Id + ": Could not parse " + val5.Value + " as a bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else if (value != null)
		{
			DeathCutsceneId = value.DeathCutsceneId;
			AlwaysPlayDeathCutscene = value.AlwaysPlayDeathCutscene;
		}
	}
}
