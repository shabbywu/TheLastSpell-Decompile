using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public abstract class ASpawnActorPhaseActionDefinition : ABossPhaseActionDefinition
{
	protected UnitCreationSettings unitCreationSettingsTemplate;

	public List<string> ActorsIds { get; protected set; }

	public bool CameraFocus { get; private set; }

	public bool HasMultipleActorId
	{
		get
		{
			List<string> actorsIds = ActorsIds;
			if (actorsIds == null)
			{
				return false;
			}
			return actorsIds.Count > 1;
		}
	}

	protected ASpawnActorPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		ActorsIds = new List<string>();
		XAttribute val2 = val.Attribute(XName.op_Implicit("ActorId"));
		ActorsIds = TPHelpers.RemoveWhitespace(val2.Value).Split(new char[1] { ',' }).ToList();
		XAttribute val3 = val.Attribute(XName.op_Implicit("CameraFocus"));
		if (val3 != null)
		{
			if (bool.TryParse(val3.Value, out var result))
			{
				CameraFocus = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val3.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("CastSpawnSkill"));
		bool result2 = true;
		if (val4 != null && !bool.TryParse(val4.Value, out result2))
		{
			CLoggerManager.Log((object)("Could not parse SpawnActorPhaseActionDefinition castSpawnSkill value " + val4.Value + " as a valid bool value."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("PlaySpawnAnim"));
		bool result3 = true;
		if (val5 != null && !bool.TryParse(val5.Value, out result3))
		{
			CLoggerManager.Log((object)("Could not parse SpawnActorPhaseActionDefinition playSpawnAnim value " + val5.Value + " as a valid bool value."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val6 = val.Attribute(XName.op_Implicit("PlaySpawnCutscene"));
		bool result4 = !(this is ReplaceActorsPhaseActionDefinition);
		if (val6 != null && val6.Value != null && !bool.TryParse(val6.Value, out result4))
		{
			CLoggerManager.Log((object)("Unable to parse " + val6.Value + " into bool"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val7 = val.Attribute(XName.op_Implicit("WaitSpawnAnim"));
		bool result5 = false;
		if (val7 != null && !bool.TryParse(val7.Value, out result5))
		{
			CLoggerManager.Log((object)("Unable to parse " + val7.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val8 = val.Attribute(XName.op_Implicit("IsGuardian"));
		bool result6 = false;
		if (val8 != null && !bool.TryParse(val8.Value, out result6))
		{
			CLoggerManager.Log((object)("Unable to parse " + val8.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val9 = val.Attribute(XName.op_Implicit("IgnoreFromEnemyUnitsCount"));
		bool result7 = false;
		if (val9 != null && !bool.TryParse(val9.Value, out result7))
		{
			CLoggerManager.Log((object)("Unable to parse " + val9.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		unitCreationSettingsTemplate = new UnitCreationSettings(string.Empty, result2, result3, result4, result5, -1, null, result6, result7);
	}

	public string GetRandomActorId()
	{
		int index = 0;
		if (HasMultipleActorId)
		{
			index = RandomManager.GetRandomRange(this, 0, ActorsIds.Count);
		}
		return ActorsIds[index];
	}

	public virtual UnitCreationSettings GetUnitCreationSettings()
	{
		return new UnitCreationSettings(GetRandomActorId(), unitCreationSettingsTemplate.CastSpawnSkill, unitCreationSettingsTemplate.PlaySpawnAnim, unitCreationSettingsTemplate.PlaySpawnCutscene, unitCreationSettingsTemplate.WaitSpawnAnim, -1, null, unitCreationSettingsTemplate.IsGuardian, unitCreationSettingsTemplate.IgnoreFromEnemyUnitsCount);
	}
}
