using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class BuildingDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_ConstructionAnimationType
	{
		None,
		Instantaneous,
		Animated
	}

	public enum E_OccupationVolumeType
	{
		None,
		Adjacent,
		Ignore
	}

	[Flags]
	public enum E_BuildingCategory
	{
		None = 0,
		Obstacle = 1,
		Defensive = 2,
		Production = 4,
		LightFogSpawner = 9,
		LitBrazier = 0x11,
		UnlitBrazier = 0x21,
		Wall = 0x42,
		Watchtower = 0x82,
		Turret = 0x102,
		Trap = 0x202,
		HandledDefense = 0x402,
		Gate = 0x842,
		Barricade = 0x1002,
		BonePile = 0x3002
	}

	[Flags]
	public enum E_ConstructionCategory
	{
		None = 0,
		Defensive = 1,
		Production = 2,
		All = 3
	}

	public static class Constants
	{
		public static class Ids
		{
			public const string Catapult = "Catapult";
		}

		public const float DamagedSpriteThreshold = 0.5f;
	}

	public BattleModuleDefinition BattleModuleDefinition { get; private set; }

	public BlueprintModuleDefinition BlueprintModuleDefinition { get; private set; }

	public BrazierModuleDefinition BrazierModuleDefinition { get; private set; }

	public ConstructionModuleDefinition ConstructionModuleDefinition { get; private set; }

	public DamageableModuleDefinition DamageableModuleDefinition { get; private set; }

	public PassivesModuleDefinition PassivesModuleDefinition { get; private set; }

	public ProductionModuleDefinition ProductionModuleDefinition { get; private set; }

	public UpgradeModuleDefinition UpgradeModuleDefinition { get; private set; }

	public string Description => Localizer.Get("BuildingDescription_" + Id);

	public string Id { get; private set; }

	public string Name => Localizer.Get("BuildingName_" + Id);

	public List<string> IdListIds { get; }

	public BuildingDefinition(XContainer buildingDefinitionContainer)
		: base(buildingDefinitionContainer)
	{
		if (GenericDatabase.TryGetIdListIdsForEntity(Id, out var foundDefinitions))
		{
			IdListIds = foundDefinitions;
		}
	}

	public override void Deserialize(XContainer buildingDefinitionContainer)
	{
		XElement val = (XElement)(object)((buildingDefinitionContainer is XElement) ? buildingDefinitionContainer : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Construction"));
		if (val3 == null)
		{
			CLoggerManager.Log((object)("The Construction element is missing in " + Id + "."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			return;
		}
		ConstructionModuleDefinition = new ConstructionModuleDefinition(this, (XContainer)(object)val3);
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Blueprint"));
		if (val4 == null)
		{
			CLoggerManager.Log((object)("The Blueprint element is missing in " + Id + "."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			return;
		}
		BlueprintModuleDefinition = new BlueprintModuleDefinition(this, (XContainer)(object)val4);
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Damageable"));
		if (val5 != null)
		{
			DamageableModuleDefinition = new DamageableModuleDefinition(this, (XContainer)(object)val5);
		}
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("Brazier"));
		if (val6 != null)
		{
			BrazierModuleDefinition = new BrazierModuleDefinition(this, (XContainer)(object)val6);
		}
		XElement val7 = ((XContainer)val).Element(XName.op_Implicit("Upgrade"));
		if (val7 != null)
		{
			UpgradeModuleDefinition = new UpgradeModuleDefinition(this, (XContainer)(object)val7);
		}
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("Passives"));
		if (val8 != null)
		{
			PassivesModuleDefinition = new PassivesModuleDefinition(this, (XContainer)(object)val8);
		}
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("Battle"));
		if (val9 != null)
		{
			BattleModuleDefinition = new BattleModuleDefinition(this, (XContainer)(object)val9);
		}
		XElement val10 = ((XContainer)val).Element(XName.op_Implicit("Production"));
		if (val10 != null)
		{
			ProductionModuleDefinition = new ProductionModuleDefinition(this, (XContainer)(object)val10);
		}
	}
}
