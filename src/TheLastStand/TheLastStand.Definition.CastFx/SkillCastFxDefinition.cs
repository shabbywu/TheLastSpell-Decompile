using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.CastFx;

public class SkillCastFxDefinition : CastFxDefinition
{
	public class CasterAnimDefinition : Definition
	{
		public Node Delay { get; private set; }

		public string Path { get; private set; } = "Hero_CastSkill_Default_";


		public CasterAnimDefinition(XContainer container)
			: base(container, (Dictionary<string, string>)null)
		{
		}

		public CasterAnimDefinition()
			: this(null)
		{
		}

		public override void Deserialize(XContainer container)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			XElement val = ((container != null) ? container.Element(XName.op_Implicit("Path")) : null);
			if (val != null)
			{
				Path = val.Value;
			}
			XElement val2 = ((container != null) ? container.Element(XName.op_Implicit("Delay")) : null);
			Delay = (Node)((val2 != null) ? ((object)Parser.Parse(val2.Value, (Dictionary<string, string>)null)) : ((object)new NodeNumber(0.0)));
		}
	}

	public CasterAnimDefinition CasterAnimDef { get; private set; }

	public ManeuverFxDefinition ManeuverFxDefinition { get; private set; }

	public FollowFxDefinition FollowFxDefinition { get; private set; }

	public float MultiHitDelay { get; private set; }

	public float PropagationDelay { get; private set; }

	public float RepeatDelay { get; private set; }

	public SkillCastFxDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		if (val != null)
		{
			XAttribute val2 = val.Attribute(XName.op_Implicit("MultiHitDelay"));
			MultiHitDelay = ((val2 != null) ? float.Parse(val2.Value, CultureInfo.InvariantCulture) : 0f);
			XAttribute val3 = val.Attribute(XName.op_Implicit("RepeatDelay"));
			RepeatDelay = ((val3 != null) ? float.Parse(val3.Value, CultureInfo.InvariantCulture) : 0f);
			XAttribute val4 = val.Attribute(XName.op_Implicit("PropagationDelay"));
			PropagationDelay = ((val4 != null) ? float.Parse(val4.Value, CultureInfo.InvariantCulture) : 0f);
			XElement val5 = ((XContainer)val).Element(XName.op_Implicit("ManeuverFx"));
			if (val5 != null)
			{
				ManeuverFxDefinition = new ManeuverFxDefinition((XContainer)(object)val5);
			}
			XElement val6 = ((XContainer)val).Element(XName.op_Implicit("FollowFx"));
			if (val6 != null)
			{
				FollowFxDefinition = new FollowFxDefinition((XContainer)(object)val6);
			}
			XElement val7 = ((XContainer)val).Element(XName.op_Implicit("CasterAnim"));
			if (val7 != null)
			{
				CasterAnimDef = new CasterAnimDefinition((XContainer)(object)val7);
			}
		}
		if (CasterAnimDef == null)
		{
			CasterAnimDef = new CasterAnimDefinition();
		}
	}
}
