using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Apocalypse.LightFogSpawner;
using TheLastStand.Definition.Hazard;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Fog;

public class LightFogDefinition : HazardDefinition
{
	public struct RepelInfo
	{
		public int Range;

		public bool CheckDiagonals;
	}

	public static class Constants
	{
		public const char EmptySymbol = '_';

		public const char OriginSymbol = 'O';

		public const char AffectedSymbol = 'X';
	}

	public Dictionary<string, LightFogSpawnersGenerationDefinition> LightFogSpawnersGenerationDefinitions { get; private set; }

	public Dictionary<string, float> LightFogSpawnersMultipliers { get; private set; }

	public RepelInfo Repel { get; private set; }

	public override E_HazardType HazardType => E_HazardType.LightFog;

	public Dictionary<string, List<Vector2Int>> Patterns { get; private set; }

	public LightFogDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		XElement val = container.Element(XName.op_Implicit("Repel"));
		int.TryParse(((XContainer)val).Element(XName.op_Implicit("Range")).Value, out var result);
		Repel = new RepelInfo
		{
			Range = result,
			CheckDiagonals = (((XContainer)val).Element(XName.op_Implicit("CheckDiagonals")) != null)
		};
		LightFogSpawnersGenerationDefinitions = new Dictionary<string, LightFogSpawnersGenerationDefinition>();
		LightFogSpawnersMultipliers = new Dictionary<string, float>();
		XElement val2 = container.Element(XName.op_Implicit("LightFogSpawnersGenerations"));
		foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("LightFogSpawnersGeneration")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Id"));
			LightFogSpawnersGenerationDefinitions.Add(val3.Value, new LightFogSpawnersGenerationDefinition((XContainer)(object)item, ((Definition)this).TokenVariables));
		}
		foreach (XElement item2 in ((XContainer)((XContainer)val2).Element(XName.op_Implicit("LightFogSpawnersMultipliers"))).Elements(XName.op_Implicit("LightFogSpawnersMultiplier")))
		{
			XAttribute val4 = item2.Attribute(XName.op_Implicit("FogDensity"));
			float.TryParse(item2.Attribute(XName.op_Implicit("Multiplier")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2);
			LightFogSpawnersMultipliers.Add(val4.Value, result2);
		}
		Patterns = new Dictionary<string, List<Vector2Int>>();
		foreach (XElement item3 in ((XContainer)container.Element(XName.op_Implicit("Patterns"))).Elements(XName.op_Implicit("Pattern")))
		{
			string value = item3.Attribute(XName.op_Implicit("Id")).Value;
			Vector2Int? val5 = null;
			List<Vector2Int> list = new List<Vector2Int>();
			string[] array = item3.Value.Split(new char[1] { '\n' });
			int num = array.Length - 1;
			int num2 = 0;
			while (num >= 0)
			{
				array[num] = TPHelpers.RemoveWhitespace(array[num]);
				for (int i = 0; i < array[num].Length; i++)
				{
					if (array[num][i] == '_')
					{
						continue;
					}
					if (array[num][i] == 'O')
					{
						if (!val5.HasValue)
						{
							val5 = new Vector2Int(i, num2);
						}
						else
						{
							CLoggerManager.Log((object)("origin tile is already set for pattern " + value + " !"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
						}
					}
					else if (array[num][i] == 'X')
					{
						list.Add(new Vector2Int(i, num2));
					}
				}
				num--;
				num2++;
			}
			if (!val5.HasValue)
			{
				CLoggerManager.Log((object)("origin tile must be set for pattern " + value + " !"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				for (int j = 0; j < list.Count; j++)
				{
					List<Vector2Int> list2 = list;
					int index = j;
					list2[index] -= val5.Value;
				}
			}
			Patterns.Add(value, list);
		}
	}
}
