using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class AffixMalusDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_MalusLevel
	{
		Undefined,
		Small,
		Medium,
		Big
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MalusLevelComparer : IEqualityComparer<E_MalusLevel>
	{
		public bool Equals(E_MalusLevel x, E_MalusLevel y)
		{
			return x == y;
		}

		public int GetHashCode(E_MalusLevel obj)
		{
			return (int)obj;
		}
	}

	public static readonly MalusLevelComparer SharedMalusLevelComparer;

	public Dictionary<E_MalusLevel, float> MalusPerLevel { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public int Weight { get; private set; }

	public AffixMalusDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("StatId"));
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(val2.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse StatId Attribute value " + val2.Value + " to a valid E_Stat value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Stat = result;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Weight"));
		if (!int.TryParse(val3.Value, out var result2))
		{
			CLoggerManager.Log((object)("Could not parse AffixMalusDefinition Weight element value " + val3.Value + " to a valid int value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Weight = result2;
		MalusPerLevel = new Dictionary<E_MalusLevel, float>(SharedMalusLevelComparer);
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("Values"))).Elements())
		{
			string localName = item.Name.LocalName;
			if (!Enum.TryParse<E_MalusLevel>(localName, out var result3))
			{
				CLoggerManager.Log((object)$"Could not parse value Element Name of AffixMalusDefinition with Id {Stat} {localName} to a valid E_MalusLevel value.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			if (!float.TryParse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
			{
				CLoggerManager.Log((object)$"Could not parse Malus value element of AffixMalusDefinition {Stat} {item.Value} to a valid float value.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			MalusPerLevel.Add(result3, result4);
		}
	}

	public bool IsMalusLevelDefined(E_MalusLevel malusLevel)
	{
		return MalusPerLevel.ContainsKey(malusLevel);
	}
}
