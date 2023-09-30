using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DG.Tweening;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class BossStagingDefinition : Definition
{
	public Node TotalDuration { get; private set; }

	public Ease MovementEasing { get; private set; }

	public float MovementDuration { get; private set; }

	public float PauseDuration { get; private set; }

	public BossStagingDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("Duration"));
		TotalDuration = Parser.Parse(val.Value, (Dictionary<string, string>)null);
		if (float.TryParse(obj.Element(XName.op_Implicit("PauseDuration")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			PauseDuration = result;
		}
		if (Enum.TryParse<Ease>(obj.Element(XName.op_Implicit("MovementEasing")).Value, out Ease result2))
		{
			MovementEasing = result2;
		}
		if (float.TryParse(obj.Element(XName.op_Implicit("MovementDuration")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
		{
			MovementDuration = result3;
		}
	}
}
