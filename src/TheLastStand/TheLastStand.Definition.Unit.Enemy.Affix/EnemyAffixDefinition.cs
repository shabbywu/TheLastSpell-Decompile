using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyAffixDefinition : LocalizableDefinition
{
	public EnemyAffixEffectDefinition.E_EnemyAffixBoxType BoxType { get; private set; }

	public HashSet<string> ExcludedElites { get; private set; }

	public EnemyAffixEffectDefinition EnemyAffixEffectDefinition { get; private set; }

	public string Id { get; private set; }

	public bool IsEliteAffix { get; private set; }

	public float Weight { get; private set; }

	public EnemyAffixDefinition(XContainer container)
		: base(container)
	{
	}

	public string GetAdditionalDescription(InterpreterContext interpreter)
	{
		if (!Localizer.Exists(string.Format("{0}{1}", "EnemyAffix_AdditionalDescription_", EnemyAffixEffectDefinition.EnemyAffixEffect)))
		{
			return null;
		}
		return Localizer.Format(string.Format("{0}{1}", "EnemyAffix_AdditionalDescription_", EnemyAffixEffectDefinition.EnemyAffixEffect), GetArguments(interpreter));
	}

	public string GetDescription(InterpreterContext interpreter)
	{
		return Localizer.Format(string.Format("{0}{1}", "EnemyAffix_Description_", EnemyAffixEffectDefinition.EnemyAffixEffect), GetArguments(interpreter));
	}

	public string GetTitle()
	{
		return Localizer.Get(string.Format("{0}{1}", "EnemyAffix_Name_", EnemyAffixEffectDefinition.EnemyAffixEffect));
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		Weight = 1f;
		ExcludedElites = new HashSet<string>();
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		((Definition)this).DeserializeTokenVariables(((XContainer)val).Element(XName.op_Implicit("TokenVariables")));
		base.Deserialize((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("LocArguments")));
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("IsEliteAffix"));
		IsEliteAffix = val3 != null;
		if (IsEliteAffix)
		{
			XAttribute val4 = val3.Attribute(XName.op_Implicit("Weight"));
			if (!float.TryParse(val4.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Could not parse Weight into a float for affix \"" + Id + "\". Value : \"" + val4.Value + "\". Setting it to 1"), (Object)(object)TPSingleton<EnemyUnitDatabase>.Instance, (LogType)2, (CLogLevel)2, true, "EnemyUnitDatabase", false);
			}
			else
			{
				Weight = result;
			}
			XElement val5 = ((XContainer)val3).Element(XName.op_Implicit("ExcludedElites"));
			if (val5 != null)
			{
				foreach (XElement item in ((XContainer)val5).Elements(XName.op_Implicit("ExcludedElite")))
				{
					ExcludedElites.Add(item.Value);
				}
			}
		}
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("OverrideBoxAsset"));
		if (val6 != null)
		{
			XAttribute val7 = val6.Attribute(XName.op_Implicit("BoxId"));
			if (Enum.TryParse<EnemyAffixEffectDefinition.E_EnemyAffixBoxType>(val7.Value, out var result2))
			{
				BoxType = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse BoxId into an E_EnemyAffixBoxType : " + val7.Value), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				BoxType = EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Base;
			}
		}
		else
		{
			BoxType = EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Base;
		}
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("AffixEffect"));
		if (((XContainer)val8).Element(XName.op_Implicit("Reinforced")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyReinforcedAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Reinforced")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Aura")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyAuraAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Aura")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Mirror")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyMirrorAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Mirror")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Misty")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyMistyAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Misty")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Regenerative")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyRegenerativeAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Regenerative")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Energetic")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyEnergeticAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Energetic")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Revenge")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyRevengeAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Revenge")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Purge")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyPurgeAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Purge")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("Barrier")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyBarrierAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("Barrier")), ((Definition)this).TokenVariables);
		}
		else if (((XContainer)val8).Element(XName.op_Implicit("HigherPlane")) != null)
		{
			EnemyAffixEffectDefinition = new EnemyHigherPlaneAffixEffectDefinition((XContainer)(object)((XContainer)val8).Element(XName.op_Implicit("HigherPlane")), ((Definition)this).TokenVariables);
		}
		else
		{
			CLoggerManager.Log((object)("There is no effect defined for affix \"" + Id + "\". Did you forget to add the deserialization ?"), (Object)(object)TPSingleton<EnemyUnitDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "EnemyUnitDatabase", false);
		}
	}
}
