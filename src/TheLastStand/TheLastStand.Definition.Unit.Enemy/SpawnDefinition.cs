using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnDefinition : Definition
{
	public List<string> DisallowedEnemies { get; private set; }

	public Dictionary<int, int> DistanceMaxFromCenterPerDays { get; private set; }

	public List<Dictionary<int, Node>> ElitesPerDayDefinitions { get; private set; }

	public string Id { get; private set; }

	public List<int> SpawnsCountMultipliers { get; private set; }

	public Node SpawnsCountPerWave { get; private set; }

	public Dictionary<int, Dictionary<string, int>> SpawnDirectionsPerDayDefinitions { get; private set; }

	public Dictionary<int, List<SpawnDirectionsDefinition.E_Direction>> OverridenForbiddenDirectionsPerDay { get; private set; }

	public Dictionary<int, Dictionary<string, int>> SpawnWavesPerDayDefinitions { get; private set; }

	public int SpawnPointsPerGroup { get; private set; }

	public Vector2Int SpawnPointRect { get; private set; }

	public SpawnDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0bc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c38: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		SpawnDefinition spawnDefinition = null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val3 != null)
		{
			spawnDefinition = SpawnWaveDatabase.SpawnDefinitions[val3.Value];
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("SpawnsCountMultipliers"));
		if (val4 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnDefinition " + Id + " has no SpawnsCountMultipliers and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnsCountMultipliers = new List<int>(spawnDefinition.SpawnsCountMultipliers);
		}
		else
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			int i = 1;
			foreach (XElement item in ((XContainer)val4).Elements(XName.op_Implicit("SpawnsCountMultiplier")))
			{
				if (!int.TryParse(item.Attribute(XName.op_Implicit("StartingNight")).Value, out var result))
				{
					CLoggerManager.Log((object)"Could not cast the StartingNight attribute into an int !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				if (!int.TryParse(item.Value, out var result2))
				{
					CLoggerManager.Log((object)"Could not cast the multiplier value into an int !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				if (i == 1)
				{
					if (result != 1)
					{
						CLoggerManager.Log((object)"The SpawnsCountMultiplier for the first night (StartingNight=\"1\") is required and must be placed first !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						list.Add(0);
					}
					else
					{
						list.Add(result2);
					}
					i++;
				}
				else if (result < i)
				{
					CLoggerManager.Log((object)"The order of the SpawnsCountMultipliers isn't respected, it might lead to errors !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					for (; i < result; i++)
					{
						list2.Add(i);
						list.Add(list[list.Count - 1]);
					}
					list.Add(result2);
					i++;
				}
			}
			if (list2.Count > 0)
			{
				string text = list2[0].ToString();
				while (list2.Count > 1)
				{
					text = text + ", " + list2[1];
					list2.RemoveAt(0);
				}
				CLoggerManager.Log((object)("indexes are missing in the SpawnsCountMultipliers of the SpawnWaveConfig, it might be unintended ! Missing indexes are : " + text), (LogType)2, (CLogLevel)0, true, "StaticLog", false);
			}
			SpawnsCountMultipliers = list;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("SpawnsCountPerWave"));
		if (XDocumentExtensions.IsNullOrEmpty(val5))
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnWaveDefinitions " + Id + " has no SpawnsCountPerWave and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnsCountPerWave = spawnDefinition.SpawnsCountPerWave.Clone();
		}
		else
		{
			SpawnsCountPerWave = Parser.Parse(val5.Value, (Dictionary<string, string>)null);
		}
		SpawnWavesPerDayDefinitions = new Dictionary<int, Dictionary<string, int>>();
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("SpawnWavesPerDayDefinitions"));
		if (val6 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnWaveDefinitions " + Id + " has no SpawnWavesPerDayDefinitions and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			foreach (KeyValuePair<int, Dictionary<string, int>> spawnWavesPerDayDefinition in spawnDefinition.SpawnWavesPerDayDefinitions)
			{
				SpawnWavesPerDayDefinitions.Add(spawnWavesPerDayDefinition.Key, new Dictionary<string, int>(spawnWavesPerDayDefinition.Value));
			}
		}
		else
		{
			foreach (XElement item2 in ((XContainer)val6).Elements(XName.op_Implicit("SpawnWavesPerDayDefinition")))
			{
				XAttribute val7 = item2.Attribute(XName.op_Implicit("StartingNight"));
				if (XDocumentExtensions.IsNullOrEmpty(val7))
				{
					CLoggerManager.Log((object)"SpawnWavesPerDayDefinition must have StartingNight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (!int.TryParse(val7.Value, out var result3))
				{
					CLoggerManager.Log((object)"StartingDay must be int!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				SpawnWavesPerDayDefinitions.Add(result3, new Dictionary<string, int>());
				foreach (XElement item3 in ((XContainer)item2).Elements(XName.op_Implicit("SpawnWaveDefinition")))
				{
					XAttribute val8 = item3.Attribute(XName.op_Implicit("Id"));
					if (XDocumentExtensions.IsNullOrEmpty(val8))
					{
						CLoggerManager.Log((object)"SpawnWaveDefinition must have an Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						continue;
					}
					XAttribute val9 = item3.Attribute(XName.op_Implicit("Weight"));
					int result4;
					if (XDocumentExtensions.IsNullOrEmpty(val9))
					{
						CLoggerManager.Log((object)"SpawnWaveDefinition must have a Weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else if (!int.TryParse(val9.Value, out result4))
					{
						CLoggerManager.Log((object)"Weight must be int!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else
					{
						SpawnWavesPerDayDefinitions[result3].Add(val8.Value, result4);
					}
				}
			}
		}
		SpawnDirectionsPerDayDefinitions = new Dictionary<int, Dictionary<string, int>>();
		XElement val10 = ((XContainer)val).Element(XName.op_Implicit("SpawnDirectionsPerDayDefinitions"));
		if (val10 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnWaveDefinitions " + Id + " has no SpawnDirectionsPerDayDefinitions and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			foreach (KeyValuePair<int, Dictionary<string, int>> spawnDirectionsPerDayDefinition in spawnDefinition.SpawnDirectionsPerDayDefinitions)
			{
				SpawnDirectionsPerDayDefinitions.Add(spawnDirectionsPerDayDefinition.Key, new Dictionary<string, int>(spawnDirectionsPerDayDefinition.Value));
			}
		}
		else
		{
			foreach (XElement item4 in ((XContainer)val10).Elements(XName.op_Implicit("SpawnDirectionsPerDayDefinition")))
			{
				XAttribute val11 = item4.Attribute(XName.op_Implicit("StartingNight"));
				if (XDocumentExtensions.IsNullOrEmpty(val11))
				{
					CLoggerManager.Log((object)"SpawnDirectionPerDayDefinition must have StartingNight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (!int.TryParse(val11.Value, out var result5))
				{
					CLoggerManager.Log((object)"StartingDay must be int!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				SpawnDirectionsPerDayDefinitions.Add(result5, new Dictionary<string, int>());
				foreach (XElement item5 in ((XContainer)item4).Elements(XName.op_Implicit("SpawnDirectionDefinition")))
				{
					XAttribute val12 = item5.Attribute(XName.op_Implicit("Id"));
					if (XDocumentExtensions.IsNullOrEmpty(val12))
					{
						CLoggerManager.Log((object)"SpawnDirectionDefinition must have an Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						continue;
					}
					XAttribute val13 = item5.Attribute(XName.op_Implicit("Weight"));
					int result6;
					if (XDocumentExtensions.IsNullOrEmpty(val13))
					{
						CLoggerManager.Log((object)"SpawnDirectionDefinition must have a Weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else if (!int.TryParse(val13.Value, out result6))
					{
						CLoggerManager.Log((object)"Weight must be int!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else
					{
						SpawnDirectionsPerDayDefinitions[result5].Add(val12.Value, result6);
					}
				}
			}
		}
		OverridenForbiddenDirectionsPerDay = new Dictionary<int, List<SpawnDirectionsDefinition.E_Direction>>();
		XElement val14 = ((XContainer)val).Element(XName.op_Implicit("OverridenForbiddenDirectionsPerDay"));
		if (XDocumentExtensions.IsNullOrEmpty(val14))
		{
			if (spawnDefinition != null)
			{
				OverridenForbiddenDirectionsPerDay = spawnDefinition.OverridenForbiddenDirectionsPerDay;
			}
		}
		else
		{
			foreach (XElement item6 in ((XContainer)val14).Elements(XName.op_Implicit("OverrideForbiddenDirection")))
			{
				XAttribute obj = item6.Attribute(XName.op_Implicit("StartingNight"));
				int num = int.Parse(((obj != null) ? obj.Value : null) ?? "1");
				if (OverridenForbiddenDirectionsPerDay.ContainsKey(num))
				{
					CLoggerManager.Log((object)$"Forbidden Direction starting day {num} of SpawnDefinition {Id} is set twice!", (LogType)2, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				OverridenForbiddenDirectionsPerDay[num] = new List<SpawnDirectionsDefinition.E_Direction>();
				List<XElement> list3 = ((XContainer)item6).Elements(XName.op_Implicit("ForbiddenDirection")).ToList();
				if (list3.Count == 0)
				{
					continue;
				}
				foreach (XElement item7 in list3)
				{
					if (!Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item7.Value, out var result7))
					{
						CLoggerManager.Log((object)("Could not parse Forbidden Direction " + item7.Value + " of SpawnDefinition " + Id + " as a valid Direction!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					}
					else
					{
						DictionaryExtensions.AddAtKey<int, SpawnDirectionsDefinition.E_Direction>(OverridenForbiddenDirectionsPerDay, num, result7);
					}
				}
			}
		}
		ElitesPerDayDefinitions = new List<Dictionary<int, Node>>();
		XElement val15 = ((XContainer)val).Element(XName.op_Implicit("ElitesPerDayDefinitions"));
		if (val15 != null)
		{
			int j = 0;
			Dictionary<int, Node> dictionary = new Dictionary<int, Node>();
			foreach (XElement item8 in ((XContainer)val15).Elements(XName.op_Implicit("ElitesPerDayDefinition")))
			{
				if (!int.TryParse(item8.Attribute(XName.op_Implicit("StartingNight")).Value, out var result8))
				{
					CLoggerManager.Log((object)("Could not parse StartingNight attribute into an int (" + Id + "), skipping this one."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
					continue;
				}
				if (result8 <= j)
				{
					CLoggerManager.Log((object)$"StartingNight attribute is inferior or equal to the current index : {result8} <= {j} ({Id}). Please check the order, skipping this one.", (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
					continue;
				}
				for (; j < result8 - 1; j++)
				{
					ElitesPerDayDefinitions.Add(dictionary);
				}
				dictionary = new Dictionary<int, Node>();
				foreach (XElement item9 in ((XContainer)item8).Elements(XName.op_Implicit("Elites")))
				{
					if (!int.TryParse(item9.Attribute(XName.op_Implicit("Tier")).Value, out var result9))
					{
						CLoggerManager.Log((object)$"Could not parse Tier attribute into an int in element ElitesPerDayDefinition ({Id}, starting night : {result8}), skipping this one.", (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
						continue;
					}
					Node val16 = Parser.Parse(item9.Value, (Dictionary<string, string>)null);
					if (val16 == null)
					{
						CLoggerManager.Log((object)$"Could not parse Elites element into an interpreted expression in element ElitesPerDayDefinition ({Id}, starting night : {result8}), skipping this one.", (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
					}
					else if (dictionary.ContainsKey(result9))
					{
						CLoggerManager.Log((object)$"This tier ({result9}) is already defined in element ElitesPerDayDefinition ({Id}, starting night : {result8}), skipping this one.", (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
					}
					else
					{
						dictionary.Add(result9, val16);
					}
				}
				ElitesPerDayDefinitions.Add(dictionary);
				j++;
			}
		}
		else
		{
			ElitesPerDayDefinitions.Add(new Dictionary<int, Node>());
		}
		XElement val17 = ((XContainer)val).Element(XName.op_Implicit("SpawnPointsPerGroup"));
		if (XDocumentExtensions.IsNullOrEmpty(val17))
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnWaveDefinitions " + Id + " has no SpawnPointsPerGroup and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnPointsPerGroup = spawnDefinition.SpawnPointsPerGroup;
		}
		else
		{
			if (!int.TryParse(val17.Value, out var result10))
			{
				CLoggerManager.Log((object)"Invalid SpawnPointsPerGroup", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnPointsPerGroup = result10;
		}
		XElement val18 = ((XContainer)val).Element(XName.op_Implicit("SpawnPointRect"));
		if (val18 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnWaveDefinitions " + Id + " has no SpawnPointRect and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnPointRect = spawnDefinition.SpawnPointRect;
		}
		else
		{
			if (!int.TryParse(val18.Attribute(XName.op_Implicit("Width")).Value, out var result11))
			{
				CLoggerManager.Log((object)"Invalid SpawnPointRect Width", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			if (!int.TryParse(val18.Attribute(XName.op_Implicit("Height")).Value, out var result12))
			{
				CLoggerManager.Log((object)"Invalid SpawnPointRect Height", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			SpawnPointRect = new Vector2Int(result11, result12);
		}
		XElement val19 = ((XContainer)val).Element(XName.op_Implicit("DistanceMaxFromCenterPerDays"));
		if (val19 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log((object)("SpawnWaveDefinitions " + Id + " has no DistanceMaxFromCenterPerDays and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			DistanceMaxFromCenterPerDays = spawnDefinition.DistanceMaxFromCenterPerDays;
			DistanceMaxFromCenterPerDays = new Dictionary<int, int>(spawnDefinition.DistanceMaxFromCenterPerDays);
		}
		else
		{
			DistanceMaxFromCenterPerDays = new Dictionary<int, int>();
			foreach (XElement item10 in ((XContainer)val19).Elements(XName.op_Implicit("DistanceMaxFromCenterPerDay")))
			{
				XAttribute val20 = item10.Attribute(XName.op_Implicit("StartingNight"));
				if (XDocumentExtensions.IsNullOrEmpty(val20))
				{
					CLoggerManager.Log((object)"DistanceMaxFromCenterPerDay must have StartingNight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (!int.TryParse(val20.Value, out var result13))
				{
					CLoggerManager.Log((object)"StartingDay must be int!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				XAttribute val21 = item10.Attribute(XName.op_Implicit("Value"));
				int result14;
				if (XDocumentExtensions.IsNullOrEmpty(val21))
				{
					CLoggerManager.Log((object)"DistanceMaxFromCenterPerDay must have Value", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else if (!int.TryParse(val21.Value, out result14))
				{
					CLoggerManager.Log((object)("Invalid DistanceMaxFromCenterPerDay Value " + item10.Value), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					DistanceMaxFromCenterPerDays.Add(result13, result14);
				}
			}
		}
		XElement val22 = ((XContainer)val).Element(XName.op_Implicit("DisallowedEnemies"));
		if (val22 == null)
		{
			if (spawnDefinition != null && spawnDefinition.DisallowedEnemies != null)
			{
				DisallowedEnemies = new List<string>(spawnDefinition.DisallowedEnemies);
			}
			return;
		}
		DisallowedEnemies = new List<string>();
		foreach (XElement item11 in ((XContainer)val22).Elements(XName.op_Implicit("EnemyId")))
		{
			DisallowedEnemies.Add(item11.Value);
		}
	}

	public List<SpawnDirectionsDefinition.E_Direction> GetOverridenForbiddenDirectionsForDayNumber(int dayNumber)
	{
		Dictionary<int, List<SpawnDirectionsDefinition.E_Direction>> overridenForbiddenDirectionsPerDay = OverridenForbiddenDirectionsPerDay;
		if (overridenForbiddenDirectionsPerDay == null)
		{
			return null;
		}
		List<SpawnDirectionsDefinition.E_Direction> result = null;
		foreach (KeyValuePair<int, List<SpawnDirectionsDefinition.E_Direction>> item in overridenForbiddenDirectionsPerDay)
		{
			if (item.Key <= dayNumber)
			{
				result = item.Value;
				continue;
			}
			break;
		}
		return result;
	}
}
