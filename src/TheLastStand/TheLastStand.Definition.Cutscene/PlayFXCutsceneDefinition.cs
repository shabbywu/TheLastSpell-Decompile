using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.SpawnFx;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class PlayFXCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayFX";
	}

	public SpawnFxDefinition SpawnFxDefinition { get; private set; }

	public int? TileX { get; private set; }

	public int? TileY { get; private set; }

	public bool WaitForFXDuration { get; private set; }

	public bool SpecifiedTilePosition
	{
		get
		{
			if (TileX.HasValue)
			{
				return TileY.HasValue;
			}
			return false;
		}
	}

	public PlayFXCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("SpawnFXs"));
		if (val != null)
		{
			SpawnFxDefinition = new SpawnFxDefinition((XContainer)(object)val);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("WaitForFXDuration"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				WaitForFXDuration = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val2.Value + " into bool."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("TileX"));
		if (val3 != null)
		{
			if (int.TryParse(val3.Value, out var result2))
			{
				TileX = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val3.Value + " into int."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("TileY"));
		if (val4 != null)
		{
			if (int.TryParse(val4.Value, out var result3))
			{
				TileY = result3;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val4.Value + " into int."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
	}
}
