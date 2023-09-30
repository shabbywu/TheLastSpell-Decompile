using System.Xml.Linq;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class ApocalypseDatabase : Database<ApocalypseDatabase>
{
	[SerializeField]
	private TextAsset apocalypsesXmlFile;

	[SerializeField]
	private TextAsset configurationXmlFile;

	public static ApocalypsesDefinition ApocalypsesDefinition { get; private set; }

	public static ApocalypseConfigurationDefinition ConfigurationDefinition { get; private set; }

	public static ApocalypseDefinition[] ComputeApocalypses(int id)
	{
		if (id == 0)
		{
			return new ApocalypseDefinition[1] { ApocalypsesDefinition.ApocalypseDefinitions[id] };
		}
		ApocalypseDefinition[] array = new ApocalypseDefinition[id];
		for (int i = 1; i <= id; i++)
		{
			array[i - 1] = ApocalypsesDefinition.ApocalypseDefinitions[i];
		}
		return array;
	}

	public static bool DoesApocalypseExist(int id)
	{
		return ApocalypsesDefinition.ApocalypseDefinitions.Find((ApocalypseDefinition x) => x.Id == id) != null;
	}

	public override void Deserialize(XContainer container = null)
	{
		ApocalypsesDefinition = new ApocalypsesDefinition((XContainer)(object)((XContainer)XDocument.Parse(apocalypsesXmlFile.text, (LoadOptions)2)).Element(XName.op_Implicit("ApocalypseDefinitions")));
		ConfigurationDefinition = new ApocalypseConfigurationDefinition((XContainer)(object)((XContainer)XDocument.Parse(configurationXmlFile.text, (LoadOptions)2)).Element(XName.op_Implicit("ApocalypseConfigurationDefinitions")));
	}
}
