using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SetPhasePhaseActionDefinition : ABossPhaseActionDefinition
{
	public string PhaseId { get; private set; }

	public SetPhasePhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Id"));
		PhaseId = val.Value;
	}
}
