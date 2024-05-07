using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SpawnActorPhaseActionDefinition : ASpawnActorPhaseActionDefinition
{
	public int AmountToSpawn { get; private set; } = 1;


	public bool PrioritizeWaveSide { get; private set; }

	public SpawnActorPhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Amount"));
		if (val != null && val.Value != null)
		{
			if (int.TryParse(val.Value, out var result) && result > 0)
			{
				AmountToSpawn = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val.Value + " into int or value <= 0"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("PrioritizeWaveSide"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result2))
			{
				PrioritizeWaveSide = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		if (base.CameraFocus && !unitCreationSettingsTemplate.WaitSpawnAnim)
		{
			CLoggerManager.Log((object)"CameraFocus was set to true, but WaitSpawnAnim is set to false. Are you sure that is intended ? -> weird behavior INC.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
	}
}
