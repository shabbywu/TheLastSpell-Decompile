using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public class StartOfNightPlayableTurnTrigger : PassiveTrigger
{
	public StartOfNightPlayableTurnTriggerController StartOfNightPlayableTurnTriggerController => base.PassiveTriggerController as StartOfNightPlayableTurnTriggerController;

	public StartOfNightPlayableTurnTriggerDefinition StartOfNightPlayableTurnTriggerDefinition => base.PassiveTriggerDefinition as StartOfNightPlayableTurnTriggerDefinition;

	public StartOfNightPlayableTurnTrigger(StartOfNightPlayableTurnTriggerDefinition passiveTriggerDefinition, StartOfNightPlayableTurnTriggerController controller)
		: base(passiveTriggerDefinition, controller)
	{
	}
}
