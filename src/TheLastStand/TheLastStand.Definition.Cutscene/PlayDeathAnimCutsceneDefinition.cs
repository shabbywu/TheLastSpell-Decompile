using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class PlayDeathAnimCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayDeathAnim";
	}

	public bool WaitDeathAnim { get; private set; }

	public PlayDeathAnimCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("WaitDeathAnim"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				WaitDeathAnim = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
