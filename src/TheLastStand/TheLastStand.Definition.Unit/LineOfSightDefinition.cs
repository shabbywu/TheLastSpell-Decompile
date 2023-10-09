using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class LineOfSightDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> BuildingBlockingExceptions { get; private set; }

	public List<string> EnemyUnitsBlocking { get; set; }

	public bool PlayableUnitsBlocking { get; set; }

	public LineOfSightDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = container.Element(XName.op_Implicit("LineOfSightDefinition"));
		if (val == null)
		{
			Debug.LogError((object)"The document must have LineOfSightDefinition");
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Buildings"));
		if (val2 != null)
		{
			BuildingBlockingExceptions = new List<string>();
			foreach (XElement item in ((XContainer)val2).Elements(XName.op_Implicit("BuildingException")))
			{
				XAttribute val3 = item.Attribute(XName.op_Implicit("Id"));
				if (val3.IsNullOrEmpty())
				{
					Debug.LogError((object)"BuildingException must have a valid Id");
				}
				else
				{
					BuildingBlockingExceptions.Add(val3.Value);
				}
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("PlayableUnits"));
		PlayableUnitsBlocking = val4 != null;
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("EnemyUnits"));
		if (val5 == null)
		{
			return;
		}
		EnemyUnitsBlocking = new List<string>();
		foreach (XElement item2 in ((XContainer)val5).Elements(XName.op_Implicit("EnemyUnitsBlocking")))
		{
			XAttribute val6 = item2.Attribute(XName.op_Implicit("Id"));
			if (val6.IsNullOrEmpty())
			{
				Debug.LogError((object)"EnemyException must have a valid Id ");
			}
			else if (!EnemyUnitsBlocking.Contains(val6.Value))
			{
				EnemyUnitsBlocking.Add(val6.Value);
			}
		}
	}
}
