using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class InitUnitVisualsCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "InitUnitVisuals";
	}

	public bool CastSpawnSkill { get; private set; } = true;


	public bool PlaySpawnAnim { get; private set; } = true;


	public bool WaitSpawnAnim { get; private set; } = true;


	public bool WaitAppearanceDelay { get; private set; } = true;


	public InitUnitVisualsCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("CastSpawnSkill"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				CastSpawnSkill = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse InitUnitVisualsCutsceneDefinition castSpawnSkill value " + val.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				CastSpawnSkill = true;
			}
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("PlaySpawnAnim"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result2))
			{
				PlaySpawnAnim = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse InitUnitVisualsCutsceneDefinition playSpawnAnim value " + val2.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				PlaySpawnAnim = true;
			}
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("WaitSpawnAnim"));
		if (val3 != null)
		{
			if (bool.TryParse(val3.Value, out var result3))
			{
				WaitSpawnAnim = result3;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse InitUnitVisualsCutsceneDefinition waitSpawnAnim value " + val3.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				WaitSpawnAnim = true;
			}
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("WaitAppearanceDelay"));
		if (val4 != null)
		{
			if (bool.TryParse(val4.Value, out var result4))
			{
				WaitAppearanceDelay = result4;
				return;
			}
			CLoggerManager.Log((object)("Could not parse InitUnitVisualsCutsceneDefinition waitAppearanceDelay value " + val4.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			WaitAppearanceDelay = true;
		}
	}
}
