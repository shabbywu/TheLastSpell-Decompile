using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class ChangeMusicCutsceneDefinition : Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "ChangeMusic";
	}

	public bool Instant { get; private set; }

	public ChangeMusicCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Instant"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				Instant = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse ChangeMusicCutsceneDefinition Instant Attribute value " + val.Value + " as a valid bool value."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
