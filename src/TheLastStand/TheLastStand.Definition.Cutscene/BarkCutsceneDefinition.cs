using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class BarkCutsceneDefinition : Definition, ICutsceneDefinition
{
	public enum E_BarkerType
	{
		Unit,
		MagicCircle,
		BossUnit,
		CutsceneUnit
	}

	public enum E_LookAtTarget
	{
		None,
		Barker,
		MagicCircle
	}

	public enum E_MoveCamera
	{
		Never,
		IfOutOfScreen,
		Always
	}

	public static class Constants
	{
		public const string Id = "Bark";
	}

	public BarkDefinition BarkDefinition { get; private set; }

	public string BarkerId { get; private set; }

	public E_BarkerType BarkerType { get; private set; }

	public E_LookAtTarget LookAtTarget { get; private set; }

	public E_MoveCamera MoveCamera { get; private set; }

	public BarkCutsceneDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement container2 = obj.Element(XName.op_Implicit("BarkDefinition"));
		BarkDefinition = new BarkDefinition((XContainer)(object)container2);
		BarkDatabase.BarkDefinitions.Add(BarkDefinition.Id, BarkDefinition);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("BarkerType"));
		if (Enum.TryParse<E_BarkerType>(val.Value, out var result))
		{
			BarkerType = result;
		}
		else
		{
			CLoggerManager.Log((object)("Could not parse " + val.Value + " as a valid BarkerType."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("BarkerId"));
		if (!string.IsNullOrEmpty((val2 != null) ? val2.Value : null))
		{
			BarkerId = val2.Value;
		}
		XElement val3 = obj.Element(XName.op_Implicit("LookAt"));
		if (val3 != null)
		{
			if (Enum.TryParse<E_LookAtTarget>(val3.Value, out var result2))
			{
				LookAtTarget = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse " + val3.Value + " as a valid LookAtTarget."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			LookAtTarget = E_LookAtTarget.None;
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("MoveCamera"));
		if (val4 != null)
		{
			if (Enum.TryParse<E_MoveCamera>(val4.Value, out var result3))
			{
				MoveCamera = result3;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse " + val4.Value + " as a valid E_MoveCamera."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			MoveCamera = E_MoveCamera.Never;
		}
	}
}
