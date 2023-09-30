using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class ResourceDefinition : Definition
{
	public static class Constants
	{
		public static class Ids
		{
			public const string Gold = "Gold";

			public const string Materials = "Materials";

			public const string Workers = "Workers";
		}
	}

	public int Gold { get; private set; }

	public string Id { get; private set; }

	public int Materials { get; private set; }

	public ResourceDefinition(XContainer xContainer)
		: base(xContainer, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement val = (XElement)(object)((xContainer is XElement) ? xContainer : null);
		ResourceDefinition resourceDefinition = null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			CLoggerManager.Log((object)"A ResourceDefinition must have an Id attribute.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val3 != null)
		{
			resourceDefinition = ResourceDatabase.ResourceDefinitions[val3.Value];
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Gold"));
		if (XDocumentExtensions.IsNullOrEmpty(val4))
		{
			if (resourceDefinition == null)
			{
				CLoggerManager.Log((object)("ResourceDefinition " + Id + " has no Gold and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Gold = resourceDefinition.Gold;
		}
		else
		{
			if (!int.TryParse(val4.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse Gold element value " + val4.Value + " to a valid int value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Gold = result;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Materials"));
		int result2;
		if (XDocumentExtensions.IsNullOrEmpty(val5))
		{
			if (resourceDefinition == null)
			{
				CLoggerManager.Log((object)("ResourceDefinition " + Id + " has no Materials and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				Materials = resourceDefinition.Materials;
			}
		}
		else if (!int.TryParse(val5.Value, out result2))
		{
			CLoggerManager.Log((object)("Could not parse Materials element value " + val5.Value + " to a valid int value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			Materials = result2;
		}
	}
}
