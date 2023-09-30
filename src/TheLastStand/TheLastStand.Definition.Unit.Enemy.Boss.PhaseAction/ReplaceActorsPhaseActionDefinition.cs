using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class ReplaceActorsPhaseActionDefinition : ASpawnActorPhaseActionDefinition
{
	public string ReplacementId { get; private set; }

	public bool WaitDeathAnim { get; private set; }

	public ReplaceActorsPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ReplacementId"));
		ReplacementId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("WaitDeathAnim"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				WaitDeathAnim = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		base.UnitCreationSettings = new UnitCreationSettings(ReplacementId, base.UnitCreationSettings.CastSpawnSkill, base.UnitCreationSettings.PlaySpawnAnim, base.UnitCreationSettings.PlaySpawnCutscene, base.UnitCreationSettings.WaitSpawnAnim, -1, null, base.UnitCreationSettings.IsGuardian);
	}
}
