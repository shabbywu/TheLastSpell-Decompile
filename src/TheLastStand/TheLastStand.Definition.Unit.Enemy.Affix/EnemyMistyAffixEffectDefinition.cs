using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyMistyAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public bool CanLightFogExistOnSelf { get; private set; }

	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Misty;

	public Node Range { get; private set; }

	public EnemyMistyAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("CanLightFogExistOnSelf"));
		CanLightFogExistOnSelf = val != null;
		XElement val2 = obj.Element(XName.op_Implicit("Range"));
		Range = Parser.Parse(val2.Value, ((Definition)this).TokenVariables);
	}
}
