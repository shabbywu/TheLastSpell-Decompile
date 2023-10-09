using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaReplicaDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int AnswersCount { get; private set; }

	public List<string> BlockReplicas { get; private set; }

	public MetaNarrationConditionsDefinition ConditionsDefinition { get; private set; }

	public string Id { get; private set; }

	public bool Mandatory { get; private set; }

	public MetaReplicaDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("AnswersCount"));
		if (val3 != null)
		{
			if (!int.TryParse(val3.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse " + val3.Value + " to a valid int value for replica \"" + Id + "\"."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			AnswersCount = result;
		}
		else
		{
			AnswersCount = 1;
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("BlockReplicas"));
		if (val4 != null)
		{
			BlockReplicas = new List<string>();
			foreach (XElement item in ((XContainer)val4).Elements(XName.op_Implicit("ReplicaId")))
			{
				BlockReplicas.Add(item.Value);
			}
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("Mandatory"));
		if (val5 != null)
		{
			if (!bool.TryParse(val5.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse Mandatory Attribute " + val5.Value + " to a valid boolean value for replica \"" + Id + "\"."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Mandatory = result2;
		}
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("Conditions"));
		if (val6 != null)
		{
			ConditionsDefinition = new MetaNarrationConditionsDefinition((XContainer)(object)val6);
		}
	}
}
