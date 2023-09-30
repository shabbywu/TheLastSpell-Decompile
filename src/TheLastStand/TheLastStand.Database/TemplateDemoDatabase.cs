using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Maths;
using UnityEngine;

namespace TheLastStand.Database;

public class TemplateDemoDatabase : Database<TemplateDemoDatabase>
{
	[SerializeField]
	private TextAsset demoTextAsset;

	public static Dictionary<string, TemplateDemoDefinition> DemoDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		CLoggerManager.Log((object)("Deserializing " + typeof(TemplateDemoDatabase).Name + ", make sure it is for testing purpose, because this prefab should NOT be in Database."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
		DemoDefinitions = new Dictionary<string, TemplateDemoDefinition>();
		DeserializeUsingGenericTopologicSortMethod();
		foreach (KeyValuePair<string, TemplateDemoDefinition> demoDefinition in DemoDefinitions)
		{
			CLoggerManager.Log((object)((object)demoDefinition.Value).ToString(), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
	}

	private void DeserializeUsingTwoStepsDeserialization()
	{
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(demoTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("TemplateDemoDefinitions"))).Elements(XName.op_Implicit("DemoDefinition")))
		{
			TemplateDemoDefinition templateDemoDefinition = new TemplateDemoDefinition((XContainer)(object)item);
			DemoDefinitions.Add(templateDemoDefinition.Id, templateDemoDefinition);
		}
		foreach (TemplateDemoDefinition item2 in TopologicSorter.Sort<TemplateDemoDefinition>((IEnumerable<TemplateDemoDefinition>)DemoDefinitions.Values).ToList())
		{
			item2.DeserializeAfterTemplatesOrdering();
		}
	}

	private void DeserializeUsingGenericTopologicSortMethod()
	{
		IEnumerable<XElement> enumerable = ((XContainer)((XContainer)XDocument.Parse(demoTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("TemplateDemoDefinitions"))).Elements(XName.op_Implicit("DemoDefinition"));
		foreach (XElement item in base.SortElementsByDependencies(enumerable))
		{
			TemplateDemoDefinition templateDemoDefinition = new TemplateDemoDefinition((XContainer)(object)item);
			DemoDefinitions.Add(templateDemoDefinition.Id, templateDemoDefinition);
		}
	}
}
