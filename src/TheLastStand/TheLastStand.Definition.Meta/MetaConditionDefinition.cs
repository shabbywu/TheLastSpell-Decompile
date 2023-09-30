using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaConditionDefinition : Definition
{
	public List<string> Arguments { get; private set; } = new List<string>();


	public int ConditionsGroupIndex { get; }

	public bool Hidden { get; }

	public string LocalizationKey { get; private set; }

	public string Name { get; private set; }

	public int Occurences { get; private set; } = 1;


	public MetaConditionDefinition(XContainer container, bool hidden, int conditionsGroupIndex)
		: base(container, (Dictionary<string, string>)null)
	{
		ConditionsGroupIndex = conditionsGroupIndex;
		Hidden = hidden;
	}

	public override void Deserialize(XContainer container)
	{
		XElement metaConditionElement = (XElement)(object)((container is XElement) ? container : null);
		Name = metaConditionElement.Name.LocalName;
		if (!TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary.ContainsKey(Name))
		{
			throw new Exception("Invalid condition function " + Name + " - Please pick a function in the following list: " + string.Join(", ", TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary.Keys));
		}
		XAttribute val = metaConditionElement.Attribute(XName.op_Implicit("Occurences"));
		if (val != null)
		{
			if (int.TryParse(val.Value, out var result))
			{
				Occurences = result;
			}
			else
			{
				CLoggerManager.Log((object)("Occurences attribute has an invalid value " + val.Value + ". Setting it to 1."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		XAttribute obj = metaConditionElement.Attribute(XName.op_Implicit("LocalizationKey"));
		LocalizationKey = ((obj != null) ? obj.Value : null);
		Arguments = (from value in Enumerable.Range(65, 26).Select(delegate(int index)
			{
				XAttribute obj2 = metaConditionElement.Attribute(XName.op_Implicit(((char)index).ToString()));
				return (obj2 == null) ? null : obj2.Value.Trim();
			})
			where value != null
			select value).ToList();
	}

	public override string ToString()
	{
		return string.Format("{0} ({1}) (Occurences={2})", Name, string.Join(", ", Arguments), Occurences);
	}
}
