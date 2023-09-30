using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public abstract class EnemyStatusAffixEffectDefinition : EnemyAffixEffectDefinition
{
	public Status.E_StatusType StatusType { get; private set; }

	protected EnemyStatusAffixEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (Enum.TryParse<Status.E_StatusType>(StringExtensions.Replace(((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("StatusType")).Value, ((Definition)this).TokenVariables), out var result))
		{
			StatusType = result;
		}
	}
}
