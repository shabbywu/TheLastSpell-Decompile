using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingAction;

public class BuildingActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public static class Ids
		{
			public const string FillGauge = "FillGauge";

			public const string Heal = "Heal";

			public const string HealMana = "HealMana";

			public const string Scavenge = "Scavenge";

			public const string GainGold = "GainGold";

			public const string GainMaterials = "GainMaterials";

			public const string RepelFog = "RepelFog";

			public const string RevealWaveEnemiesRatio = "RevealWaveEnemiesRatio";

			public const string RerollWave = "RerollWave";

			public const string UpgradeStat = "UpgradeStat";
		}
	}

	private int workersCost;

	public List<BuildingActionEffectDefinition> BuildingActionEffectDefinition { get; private set; }

	public CastFxDefinition CastFxDefinition { get; private set; }

	public bool ContainsRepelFogEffect { get; private set; }

	public string Id { get; private set; }

	public string LoreDescription => string.Empty;

	public PhaseStates PhaseStates { get; } = new PhaseStates(PhaseStates.E_PhaseState.Available, PhaseStates.E_PhaseState.Available, PhaseStates.E_PhaseState.Available);


	public string Name => Localizer.Get("BuildingActionName_" + Id);

	public int UsesPerTurnCount { get; private set; } = -1;


	public int WorkersCost
	{
		get
		{
			if (WorkersExpression != null)
			{
				if (!(ApplicationManager.Application.State.GetName() == "Game"))
				{
					return -1;
				}
				return WorkersExpression.EvalToInt(TPSingleton<GameManager>.Instance);
			}
			return workersCost;
		}
	}

	public Node WorkersExpression { get; private set; }

	public BuildingActionDefinition(XContainer container)
		: base(container)
	{
	}

	public string GetDescription(int unitsThreshold = -1, int productionValue = 0)
	{
		return Localizer.Format("BuildingActionDescription_" + Id, GetArguments(unitsThreshold, productionValue));
	}

	public virtual BuildingActionDefinition Clone()
	{
		return MemberwiseClone() as BuildingActionDefinition;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2 == null)
		{
			Debug.LogError((object)"The skill has no ID !");
			return;
		}
		Id = val2.Value;
		if (((XContainer)val).Element(XName.op_Implicit("UsesPerTurnCount")) != null)
		{
			if (int.TryParse(((XContainer)val).Element(XName.op_Implicit("UsesPerTurnCount")).Value, out var result))
			{
				UsesPerTurnCount = result;
			}
			else
			{
				Debug.LogError((object)("Error while parsing UsesPerTurnCount parameter of building action " + Id + " !"));
			}
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("PhaseStates"));
		if (val3 != null)
		{
			XElement val4 = ((XContainer)val3).Element(XName.op_Implicit("Production"));
			if (!val4.IsNullOrEmpty())
			{
				if (!Enum.TryParse<PhaseStates.E_PhaseState>(val4.Value, out var result2))
				{
					Debug.LogError((object)("BuildingActionDefinition " + Id + " PhaseStates Production must be a valid E_PhaseState!"));
					return;
				}
				PhaseStates.ProductionState = result2;
			}
			XElement val5 = ((XContainer)val3).Element(XName.op_Implicit("Deployment"));
			if (!val5.IsNullOrEmpty())
			{
				if (!Enum.TryParse<PhaseStates.E_PhaseState>(val5.Value, out var result3))
				{
					Debug.LogError((object)("BuildingActionDefinition " + Id + " PhaseStates Deployment must be a valid E_PhaseState!"));
					return;
				}
				PhaseStates.DeploymentState = result3;
			}
			XElement val6 = ((XContainer)val3).Element(XName.op_Implicit("Night"));
			if (!val6.IsNullOrEmpty())
			{
				if (!Enum.TryParse<PhaseStates.E_PhaseState>(val6.Value, out var result4))
				{
					Debug.LogError((object)("BuildingActionDefinition " + Id + "PhaseStates Night must be a valid E_PhaseState!"));
					return;
				}
				PhaseStates.NightState = result4;
			}
		}
		XElement val7 = ((XContainer)val).Element(XName.op_Implicit("WorkersCost"));
		if (!string.IsNullOrEmpty((val7 != null) ? val7.Value : null))
		{
			if (int.TryParse(val7.Value, out var result5))
			{
				workersCost = result5;
			}
			else
			{
				WorkersExpression = Parser.Parse(val7.Value);
			}
		}
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("ActionEffects"));
		if (val8 != null)
		{
			BuildingActionEffectDefinition = new List<BuildingActionEffectDefinition>();
			foreach (XElement item in ((XContainer)val8).Elements())
			{
				BuildingActionEffectDefinition buildingActionEffectDefinition = item.Name.LocalName switch
				{
					"FillGauge" => new FillGaugeBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"Heal" => new HealBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"HealMana" => new HealManaBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"Scavenge" => new ScavengeBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"GainGold" => new GainGoldBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"GainMaterials" => new GainMaterialsBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"RepelFog" => new RepelFogBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"RevealWaveEnemiesRatio" => new RevealDangerIndicatorsBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"RerollWave" => new RerollWaveBuildingActionEffectDefinition((XContainer)(object)item, this), 
					"UpgradeStat" => new UpgradeStatBuildingActionEffectDefinition((XContainer)(object)item, this), 
					_ => null, 
				};
				if (buildingActionEffectDefinition is RepelFogBuildingActionEffectDefinition)
				{
					ContainsRepelFogEffect = true;
				}
				BuildingActionEffectDefinition.Add(buildingActionEffectDefinition);
			}
		}
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("CastFXs"));
		if (val9 != null)
		{
			CastFxDefinition = new CastFxDefinition((XContainer)(object)val9);
		}
	}

	protected object[] GetArguments(int unitsThreshold = -1, int productionValue = 0)
	{
		List<object> list = new List<object>();
		foreach (BuildingActionEffectDefinition item in BuildingActionEffectDefinition)
		{
			if (!(item is FillGaugeBuildingActionEffectDefinition fillGaugeBuildingActionEffectDefinition))
			{
				if (!(item is GainGoldBuildingActionEffectDefinition gainGoldBuildingActionEffectDefinition))
				{
					if (!(item is GainMaterialsBuildingActionEffectDefinition gainMaterialsBuildingActionEffectDefinition))
					{
						if (!(item is HealBuildingActionEffectDefinition healBuildingActionEffectDefinition))
						{
							if (!(item is HealManaBuildingActionEffectDefinition healManaBuildingActionEffectDefinition))
							{
								if (!(item is RepelFogBuildingActionEffectDefinition repelFogBuildingActionEffectDefinition))
								{
									if (item is ScavengeBuildingActionEffectDefinition scavengeBuildingActionEffectDefinition)
									{
										list.Add(scavengeBuildingActionEffectDefinition.GainGold);
										list.Add(scavengeBuildingActionEffectDefinition.GainMaterials);
										list.Add(scavengeBuildingActionEffectDefinition.GainDamnedSouls);
										list.Add(scavengeBuildingActionEffectDefinition.CreateItemDefinitions.Count);
									}
								}
								else
								{
									list.Add(repelFogBuildingActionEffectDefinition.Amount);
								}
							}
							else
							{
								list.Add(healManaBuildingActionEffectDefinition.Amount);
							}
						}
						else
						{
							list.Add(healBuildingActionEffectDefinition.Amount);
						}
					}
					else
					{
						list.Add(gainMaterialsBuildingActionEffectDefinition.GainMaterials);
					}
				}
				else
				{
					list.Add(gainGoldBuildingActionEffectDefinition.GainGold);
				}
			}
			else
			{
				list.Add(GetFillEffectAmount(fillGaugeBuildingActionEffectDefinition, unitsThreshold, productionValue));
			}
		}
		return list.ToArray();
	}

	private object GetFillEffectAmount(FillGaugeBuildingActionEffectDefinition fillGaugeBuildingActionEffectDefinition, int unitsThreshold, int productionValue)
	{
		return (unitsThreshold > 0) ? (fillGaugeBuildingActionEffectDefinition.Amount / unitsThreshold * productionValue) : 0;
	}
}
