using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class ProbabilityTreeEntriesDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public Dictionary<int, int> ProbabilityLevels { get; set; } = new Dictionary<int, int>();


	public ProbabilityTreeEntriesDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			Debug.LogError((object)"ProbabilityLevelsElement must have a valid Id");
			return;
		}
		Id = val2.Value;
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Probability")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Weight"));
			int result2;
			if (val3.IsNullOrEmpty() || !int.TryParse(val3.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				Debug.LogError((object)("ProbabilityLevels " + Id + " Invalid weight!"));
			}
			else if (!int.TryParse(item.Value, out result2))
			{
				Debug.LogError((object)("ProbabilityLevels " + Id + " Invalid value!"));
			}
			else
			{
				ProbabilityLevels.Add(result2, result);
			}
		}
	}
}
