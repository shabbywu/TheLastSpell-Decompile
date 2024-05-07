using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class ReplaceActorsPhaseActionDefinition : ASpawnActorPhaseActionDefinition
{
	private List<string> replacementIds;

	public int Amount { get; private set; }

	public bool HasMultipleReplacementId
	{
		get
		{
			List<string> list = replacementIds;
			if (list == null)
			{
				return false;
			}
			return list.Count > 1;
		}
	}

	public bool IncludeNonActor { get; private set; }

	public bool WaitDeathAnim { get; private set; }

	public ReplaceActorsPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		replacementIds = new List<string>();
		XAttribute val2 = val.Attribute(XName.op_Implicit("ReplacementId"));
		replacementIds = TPHelpers.RemoveWhitespace(val2.Value).Split(new char[1] { ',' }).ToList();
		XAttribute val3 = val.Attribute(XName.op_Implicit("Amount"));
		if (val3 != null)
		{
			Amount = int.Parse(val3.Value);
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("IncludeNonActor"));
		if (val4 != null)
		{
			if (bool.TryParse(val4.Value, out var result))
			{
				IncludeNonActor = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val4.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("WaitDeathAnim"));
		if (val5 != null)
		{
			if (bool.TryParse(val5.Value, out var result2))
			{
				WaitDeathAnim = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val5.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		unitCreationSettingsTemplate = new UnitCreationSettings(string.Empty, unitCreationSettingsTemplate.CastSpawnSkill, unitCreationSettingsTemplate.PlaySpawnAnim, unitCreationSettingsTemplate.PlaySpawnCutscene, unitCreationSettingsTemplate.WaitSpawnAnim, -1, null, unitCreationSettingsTemplate.IsGuardian);
	}

	public override UnitCreationSettings GetUnitCreationSettings()
	{
		int index = 0;
		if (HasMultipleReplacementId)
		{
			index = RandomManager.GetRandomRange(this, 0, replacementIds.Count);
		}
		return new UnitCreationSettings(replacementIds[index], unitCreationSettingsTemplate.CastSpawnSkill, unitCreationSettingsTemplate.PlaySpawnAnim, unitCreationSettingsTemplate.PlaySpawnCutscene, unitCreationSettingsTemplate.WaitSpawnAnim, -1, null, unitCreationSettingsTemplate.IsGuardian);
	}
}
