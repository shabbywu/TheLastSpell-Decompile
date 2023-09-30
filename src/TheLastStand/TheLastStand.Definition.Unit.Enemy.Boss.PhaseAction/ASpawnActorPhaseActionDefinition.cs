using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public abstract class ASpawnActorPhaseActionDefinition : ABossPhaseActionDefinition
{
	public string ActorId { get; private set; }

	public bool CameraFocus { get; private set; }

	public UnitCreationSettings UnitCreationSettings { get; protected set; }

	protected ASpawnActorPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ActorId"));
		ActorId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("CameraFocus"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				CameraFocus = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("CastSpawnSkill"));
		bool result2 = true;
		if (val3 != null && !bool.TryParse(val3.Value, out result2))
		{
			CLoggerManager.Log((object)("Could not parse SpawnActorPhaseActionDefinition castSpawnSkill value " + val3.Value + " as a valid bool value."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("PlaySpawnAnim"));
		bool result3 = true;
		if (val4 != null && !bool.TryParse(val4.Value, out result3))
		{
			CLoggerManager.Log((object)("Could not parse SpawnActorPhaseActionDefinition playSpawnAnim value " + val4.Value + " as a valid bool value."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val5 = ((XElement)obj).Attribute(XName.op_Implicit("PlaySpawnCutscene"));
		bool result4 = !(this is ReplaceActorsPhaseActionDefinition);
		if (val5 != null && val5.Value != null && !bool.TryParse(val5.Value, out result4))
		{
			CLoggerManager.Log((object)("Unable to parse " + val5.Value + " into bool"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val6 = ((XElement)obj).Attribute(XName.op_Implicit("WaitSpawnAnim"));
		bool result5 = false;
		if (val6 != null && !bool.TryParse(val6.Value, out result5))
		{
			CLoggerManager.Log((object)("Unable to parse " + val6.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val7 = ((XElement)obj).Attribute(XName.op_Implicit("IsGuardian"));
		bool result6 = false;
		if (val7 != null && !bool.TryParse(val7.Value, out result6))
		{
			CLoggerManager.Log((object)("Unable to parse " + val7.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val8 = ((XElement)obj).Attribute(XName.op_Implicit("IgnoreFromEnemyUnitsCount"));
		bool result7 = false;
		if (val8 != null && !bool.TryParse(val8.Value, out result7))
		{
			CLoggerManager.Log((object)("Unable to parse " + val8.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		UnitCreationSettings = new UnitCreationSettings(ActorId, result2, result3, result4, result5, -1, null, result6, result7);
	}
}
