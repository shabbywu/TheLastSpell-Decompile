using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.EasyMode;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class EasyModeDatabase : Database<EasyModeDatabase>
{
	[SerializeField]
	private TextAsset easyModeDefinitionTextAsset;

	public static EasyModeDefinition EasyModeDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement val = ((XContainer)XDocument.Parse(easyModeDefinitionTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("EasyModeDefinition"));
		if (XDocumentExtensions.IsNullOrEmpty(val))
		{
			CLoggerManager.Log((object)("The document " + ((Object)easyModeDefinitionTextAsset).name + " must have an EasyModeDefinition!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			EasyModeDefinition = new EasyModeDefinition((XContainer)(object)val);
		}
	}
}
