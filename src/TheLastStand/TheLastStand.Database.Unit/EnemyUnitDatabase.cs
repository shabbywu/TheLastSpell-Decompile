using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Framework.Database;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class EnemyUnitDatabase : Database<EnemyUnitDatabase>
{
	[SerializeField]
	private TextAsset enemySkillSoundDefinitionsTextAsset;

	[SerializeField]
	private TextAsset hitByEnemySoundDefinitionsTextAsset;

	[SerializeField]
	private TextAsset tierDefinitions;

	[SerializeField]
	private IEnumerable<TextAsset> individualEnemyUnitTemplateDefinitionsTextAssets;

	[SerializeField]
	private IEnumerable<TextAsset> groupEnemyUnitTemplateDefinitionsTextAssets;

	[SerializeField]
	private IEnumerable<TextAsset> individualEliteEnemyUnitTemplateDefinitionsTextAssets;

	[SerializeField]
	private IEnumerable<TextAsset> groupEliteEnemyUnitTemplateDefinitionsTextAssets;

	[SerializeField]
	private TextAsset eliteGaugeSizeThresholdsDefinition;

	[SerializeField]
	private IEnumerable<TextAsset> individualEliteAffixDefinitionsTextAssets;

	[SerializeField]
	private IEnumerable<TextAsset> groupEliteAffixDefinitionsTextAssets;

	[SerializeField]
	[Range(0f, 100f)]
	private float defaultUnitMoveSpeed = 10f;

	public static float DefaultUnitMoveSpeed => TPSingleton<EnemyUnitDatabase>.Instance.defaultUnitMoveSpeed;

	public static Dictionary<string, EliteEnemyUnitTemplateDefinition> EliteEnemyUnitTemplateDefinitions { get; private set; }

	public static Dictionary<string, EnemyAffixDefinition> EnemyAffixDefinitions { get; private set; }

	public static List<int> EliteGaugeSizeThresholds { get; private set; }

	public static Dictionary<string, List<EnemyAffixDefinition>> EliteToAffixDefinitions { get; private set; }

	public static Dictionary<string, string> EnemyToEliteIds { get; private set; } = new Dictionary<string, string>();


	public static Dictionary<string, EnemyUnitTemplateDefinition> EnemyUnitTemplateDefinitions { get; private set; }

	public static Dictionary<int, List<EnemyUnitTemplateDefinition>> EnemyUnitTemplatesByTierDefinitions { get; private set; }

	public static List<RangeDefinition> EnemySkillSoundRangeDefinitions { get; private set; }

	public static Dictionary<string, string> SkillSoundIdOverrides { get; private set; }

	public static Dictionary<string, HitByEnemySoundDefinition> HitByEnemySoundDefinitions { get; private set; }

	public static Dictionary<int, TierDefinition> TierDefinitions { get; private set; }

	public static HashSet<string> UnintegratedElites { get; private set; }

	public static Dictionary<int, List<EnemyUnitTemplateDefinition>> GetEnemyUnitTemplatesByTierDefinitionsCopy()
	{
		Dictionary<int, List<EnemyUnitTemplateDefinition>> dictionary = new Dictionary<int, List<EnemyUnitTemplateDefinition>>();
		foreach (int key in EnemyUnitTemplatesByTierDefinitions.Keys)
		{
			List<EnemyUnitTemplateDefinition> list = new List<EnemyUnitTemplateDefinition>();
			foreach (EnemyUnitTemplateDefinition item in EnemyUnitTemplatesByTierDefinitions[key])
			{
				list.Add(item);
			}
			list = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, list).ToList();
			dictionary.Add(key, list);
		}
		return dictionary;
	}

	public override void Deserialize(XContainer container = null)
	{
		if (EnemyUnitTemplateDefinitions != null)
		{
			return;
		}
		EnemyAffixDefinitions = new Dictionary<string, EnemyAffixDefinition>();
		Queue<XElement> queue = GatherElements(groupEliteAffixDefinitionsTextAssets, individualEliteAffixDefinitionsTextAssets, "EnemyAffixDefinition");
		while (queue.Count > 0)
		{
			EnemyAffixDefinition enemyAffixDefinition = new EnemyAffixDefinition((XContainer)(object)queue.Dequeue());
			try
			{
				EnemyAffixDefinitions.Add(enemyAffixDefinition.Id, enemyAffixDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate affix definition found for ID " + enemyAffixDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file."), (Object)(object)this, (LogType)2, (CLogLevel)2, true, "EnemyUnitDatabase", false);
			}
		}
		EnemyUnitTemplateDefinitions = new Dictionary<string, EnemyUnitTemplateDefinition>();
		Queue<XElement> elements = GatherElements(groupEnemyUnitTemplateDefinitionsTextAssets, individualEnemyUnitTemplateDefinitionsTextAssets, "EnemyUnitTemplateDefinition");
		foreach (XElement item in SortElementsByDependencies(elements))
		{
			EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = new EnemyUnitTemplateDefinition((XContainer)(object)item);
			try
			{
				EnemyUnitTemplateDefinitions.Add(enemyUnitTemplateDefinition.Id, enemyUnitTemplateDefinition);
			}
			catch (ArgumentException)
			{
				Debug.LogWarning((object)("Duplicate enemy definition found for ID " + enemyUnitTemplateDefinition.Id + ": the individual template files will have PRIORITY over the all-in-one template file."));
			}
		}
		EnemyUnitTemplatesByTierDefinitions = new Dictionary<int, List<EnemyUnitTemplateDefinition>>();
		foreach (EnemyUnitTemplateDefinition value in EnemyUnitTemplateDefinitions.Values)
		{
			if (!EnemyUnitTemplatesByTierDefinitions.ContainsKey(value.Tier))
			{
				EnemyUnitTemplatesByTierDefinitions.Add(value.Tier, new List<EnemyUnitTemplateDefinition>());
			}
			EnemyUnitTemplatesByTierDefinitions[value.Tier].Add(value);
		}
		EliteEnemyUnitTemplateDefinitions = new Dictionary<string, EliteEnemyUnitTemplateDefinition>();
		Queue<XElement> queue2 = GatherElements(groupEliteEnemyUnitTemplateDefinitionsTextAssets, individualEliteEnemyUnitTemplateDefinitionsTextAssets, "EliteEnemyUnitTemplateDefinition");
		while (queue2.Count > 0)
		{
			EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition = new EliteEnemyUnitTemplateDefinition((XContainer)(object)queue2.Dequeue());
			try
			{
				EliteEnemyUnitTemplateDefinitions.Add(eliteEnemyUnitTemplateDefinition.EliteId, eliteEnemyUnitTemplateDefinition);
				if (EnemyToEliteIds.ContainsKey(eliteEnemyUnitTemplateDefinition.EliteId))
				{
					CLoggerManager.Log((object)("An enemy has at least 2 elite versions : " + EnemyToEliteIds[eliteEnemyUnitTemplateDefinition.EliteId] + " and " + eliteEnemyUnitTemplateDefinition.EliteId + ". This will definitely mess things up."), (Object)(object)this, (LogType)0, (CLogLevel)2, true, "EnemyUnitDatabase", false);
				}
				else
				{
					EnemyToEliteIds.Add(eliteEnemyUnitTemplateDefinition.Id, eliteEnemyUnitTemplateDefinition.EliteId);
				}
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate enemy definition found for ID " + eliteEnemyUnitTemplateDefinition.EliteId + ": the individual template files will have PRIORITY over the all-in-one template file."), (LogType)2, (CLogLevel)2, true, "EnemyUnitDatabase", false);
			}
		}
		UnintegratedElites = new HashSet<string>();
		foreach (KeyValuePair<string, EliteEnemyUnitTemplateDefinition> eliteEnemyUnitTemplateDefinition2 in EliteEnemyUnitTemplateDefinitions)
		{
			if ((Object)(object)ResourcePooler.LoadOnce<RuntimeAnimatorController>("Animators/Units/EnemyUnits/" + eliteEnemyUnitTemplateDefinition2.Key + "/" + eliteEnemyUnitTemplateDefinition2.Key + "_01", failSilently: true) == (Object)null)
			{
				CLoggerManager.Log((object)("Elite with id \"" + eliteEnemyUnitTemplateDefinition2.Key + "\" animator could not be found at path : Animators/Units/EnemyUnits/" + eliteEnemyUnitTemplateDefinition2.Key + "/" + eliteEnemyUnitTemplateDefinition2.Key + "_01.\nIt will be considered as impossible to spawn."), (Object)(object)this, (LogType)2, (CLogLevel)2, true, "EnemyUnitDatabase", false);
				UnintegratedElites.Add(eliteEnemyUnitTemplateDefinition2.Key);
			}
		}
		EliteToAffixDefinitions = new Dictionary<string, List<EnemyAffixDefinition>>();
		foreach (KeyValuePair<string, EliteEnemyUnitTemplateDefinition> eliteEnemyUnitTemplateDefinition3 in EliteEnemyUnitTemplateDefinitions)
		{
			List<EnemyAffixDefinition> list = new List<EnemyAffixDefinition>();
			foreach (KeyValuePair<string, EnemyAffixDefinition> enemyAffixDefinition2 in EnemyAffixDefinitions)
			{
				if (enemyAffixDefinition2.Value.IsEliteAffix && !enemyAffixDefinition2.Value.ExcludedElites.Contains(eliteEnemyUnitTemplateDefinition3.Key))
				{
					list.Add(enemyAffixDefinition2.Value);
				}
			}
			if (list.Count == 0)
			{
				CLoggerManager.Log((object)("There are no affixes available for elite \"" + eliteEnemyUnitTemplateDefinition3.Key + "\". It is excluded in all known affixes."), (Object)(object)this, (LogType)2, (CLogLevel)2, true, "EnemyUnitDatabase", false);
			}
			EliteToAffixDefinitions.Add(eliteEnemyUnitTemplateDefinition3.Key, list);
		}
		EliteGaugeSizeThresholds = new List<int>();
		foreach (XElement item2 in ((XContainer)((XContainer)XDocument.Parse(eliteGaugeSizeThresholdsDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("EliteGaugeSizeThresholdsDefinition"))).Elements(XName.op_Implicit("Threshold")))
		{
			if (!int.TryParse(item2.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse Elite Health Threshold element value " + item2.Value + " to a valid integer value."), (Object)(object)this, (LogType)0, (CLogLevel)2, true, "EnemyUnitDatabase", false);
			}
			else
			{
				EliteGaugeSizeThresholds.Add(result);
			}
		}
		EnemySkillSoundRangeDefinitions = new List<RangeDefinition>();
		XElement val = ((XContainer)XDocument.Parse(enemySkillSoundDefinitionsTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("EnemySkillSoundDefinitions"));
		foreach (XElement item3 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("RangeDefinitions"))).Elements(XName.op_Implicit("Range")))
		{
			EnemySkillSoundRangeDefinitions.Add(new RangeDefinition((XContainer)(object)item3));
		}
		SkillSoundIdOverrides = new Dictionary<string, string>();
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("SkillGroupIdOverrides"));
		if (val2 != null)
		{
			foreach (XElement item4 in ((XContainer)val2).Elements(XName.op_Implicit("SkillGroupIdOverride")))
			{
				XAttribute val3 = item4.Attribute(XName.op_Implicit("SkillGroupId"));
				XAttribute val4 = item4.Attribute(XName.op_Implicit("IdOverride"));
				SkillSoundIdOverrides.Add(val3.Value, val4.Value);
			}
		}
		HitByEnemySoundDefinitions = new Dictionary<string, HitByEnemySoundDefinition>();
		foreach (XElement item5 in ((XContainer)((XContainer)XDocument.Parse(hitByEnemySoundDefinitionsTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("HitByEnemySoundDefinitions"))).Elements(XName.op_Implicit("HitByEnemySoundDefinition")))
		{
			HitByEnemySoundDefinition hitByEnemySoundDefinition = new HitByEnemySoundDefinition((XContainer)(object)item5);
			HitByEnemySoundDefinitions.Add(hitByEnemySoundDefinition.Target, hitByEnemySoundDefinition);
		}
		TierDefinitions = new Dictionary<int, TierDefinition>();
		foreach (XElement item6 in ((XContainer)((XContainer)XDocument.Parse(tierDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("TierDefinitions"))).Elements(XName.op_Implicit("TierDefinition")))
		{
			TierDefinition tierDefinition = new TierDefinition((XContainer)(object)item6);
			TierDefinitions.Add(tierDefinition.Index, tierDefinition);
		}
	}
}
