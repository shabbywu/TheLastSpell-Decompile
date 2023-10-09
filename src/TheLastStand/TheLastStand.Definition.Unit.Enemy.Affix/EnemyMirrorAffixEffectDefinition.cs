using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyMirrorAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public override E_EnemyAffixEffect EnemyAffixEffect => E_EnemyAffixEffect.Mirror;

	public Node DamageValue { get; private set; }

	public EnemyMirrorAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("DamageValue"));
		DamageValue = Parser.Parse(val.Value, base.TokenVariables);
	}
}
