using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Maths;
using UnityEngine;

namespace TheLastStand.Database;

public class GenericDatabase : Database<GenericDatabase>
{
	[SerializeField]
	private TextAsset[] idsListsDefinitionsTextAssets;

	public static Dictionary<string, IdsListDefinition> IdsListDefinitions { get; private set; }

	public static List<IdsListDefinition> GetIdListDefinitionForEntity(string entityId)
	{
		List<IdsListDefinition> list = new List<IdsListDefinition>();
		foreach (IdsListDefinition value in IdsListDefinitions.Values)
		{
			if (value.Ids.Contains(entityId))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static List<string> GetIdListIdsForEntity(string entityId)
	{
		return (from x in GetIdListDefinitionForEntity(entityId)
			select x.Id).ToList();
	}

	public static bool TryGetIdListDefinitionForEntity(string entityId, out List<IdsListDefinition> foundDefinitions)
	{
		foundDefinitions = GetIdListDefinitionForEntity(entityId);
		return foundDefinitions.Count > 0;
	}

	public static bool TryGetIdListIdsForEntity(string entityId, out List<string> foundDefinitions)
	{
		foundDefinitions = GetIdListIdsForEntity(entityId);
		return foundDefinitions.Count > 0;
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializeIdsListDefinitions();
	}

	private void DeserializeIdsListDefinitions()
	{
		IdsListDefinitions = new Dictionary<string, IdsListDefinition>();
		foreach (XElement item in base.GatherElements((IEnumerable<TextAsset>)idsListsDefinitionsTextAssets, (IEnumerable<TextAsset>)null, "IdsListDefinition", "IdsListsDefinitions"))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Id"));
			IdsListDefinitions.Add(val.Value, new IdsListDefinition((XContainer)(object)item));
		}
		foreach (IdsListDefinition item2 in TopologicSorter.Sort<IdsListDefinition>((IEnumerable<IdsListDefinition>)IdsListDefinitions.Values).ToList())
		{
			item2.DeserializeAfterDependencySorting();
		}
	}

	[ContextMenu("Log Ids Lists content")]
	private void LogIdsListsContent()
	{
		foreach (KeyValuePair<string, IdsListDefinition> idsListDefinition in IdsListDefinitions)
		{
			CLoggerManager.Log((object)idsListDefinition, (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			idsListDefinition.Value.Ids.ForEach(delegate(string o)
			{
				CLoggerManager.Log((object)o, (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			});
		}
	}
}
