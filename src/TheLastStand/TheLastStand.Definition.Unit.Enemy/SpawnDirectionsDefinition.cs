using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnDirectionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_Direction
	{
		None,
		Top,
		Bottom,
		Left,
		Right
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct DirectionComparer : IEqualityComparer<E_Direction>
	{
		public bool Equals(E_Direction x, E_Direction y)
		{
			return x == y;
		}

		public int GetHashCode(E_Direction obj)
		{
			return (int)obj;
		}
	}

	[Serializable]
	public struct SpawnDirectionInfo
	{
		public bool EquidistantSpawnPoints { get; set; }

		public int EquidistanceBordersMargin { get; set; }

		public int Proportion { get; set; }

		public int SpawnPointsPerDirectionCount { get; set; }
	}

	public class SpawnDirectionInfoContainer : List<SpawnDirectionInfo>
	{
		public int TotalProportion
		{
			get
			{
				int num = 0;
				for (int i = 0; i < base.Count; i++)
				{
					num += base[i].Proportion;
				}
				return num;
			}
		}
	}

	public static readonly List<E_Direction> OrderedDirections = new List<E_Direction>
	{
		E_Direction.Top,
		E_Direction.Right,
		E_Direction.Bottom,
		E_Direction.Left
	};

	public static readonly DirectionComparer SharedDirectionComparer = default(DirectionComparer);

	public string Id { get; private set; }

	public Dictionary<E_Direction, SpawnDirectionInfoContainer> SpawnDirectionsInfo { get; } = new Dictionary<E_Direction, SpawnDirectionInfoContainer>(SharedDirectionComparer);


	public SpawnDirectionsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"SpawnDirectionsDefinition must have a Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Direction")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Id"));
			if (val3.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)("Direction of SpawnDirectionsDefinition " + Id + " must have an Id!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!Enum.TryParse<E_Direction>(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Direction of SpawnDirectionsDefinition " + Id + " must have a valid Id!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(((XContainer)item).Element(XName.op_Implicit("Proportion")).Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse SpawnDirectionsDefinition " + Id + " Proportion to a valid int value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(((XContainer)item).Element(XName.op_Implicit("SpawnPointsPerDirectionCount")).Value, out var result3))
			{
				CLoggerManager.Log((object)("Could not parse SpawnDirectionsDefinition " + Id + " SpawnPointsPerDirectionCount to a valid int value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			XElement val4 = ((XContainer)item).Element(XName.op_Implicit("Equidistant"));
			int result4 = -1;
			if (val4 != null && !int.TryParse(val4.Attribute(XName.op_Implicit("BordersMargin")).Value, out result4))
			{
				CLoggerManager.Log((object)("Could not parse SpawnDirectionsDefinition " + Id + " equidistant BordersMargin to a valid int value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			SpawnDirectionsInfo.AddAtKey(result, new SpawnDirectionInfo
			{
				Proportion = result2,
				EquidistantSpawnPoints = (val4 != null),
				EquidistanceBordersMargin = result4,
				SpawnPointsPerDirectionCount = result3
			});
		}
		if (SpawnDirectionsInfo.Count == 0)
		{
			CLoggerManager.Log((object)"SpawnDirectionDefinition must have at least one Direction!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
