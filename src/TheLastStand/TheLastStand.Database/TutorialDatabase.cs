using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Framework.Database;
using TheLastStand.View.Tutorial;
using UnityEngine;

namespace TheLastStand.Database;

public class TutorialDatabase : Database<TutorialDatabase>
{
	[SerializeField]
	private TextAsset tutorialDefinitions;

	[SerializeField]
	private TutorialSpritesSetsTable tutorialSpritesSetsTable;

	public static Dictionary<string, TutorialDefinition> TutorialsDefinitions { get; private set; }

	public TutorialSpritesSetsTable TutorialSpritesSetsTable => tutorialSpritesSetsTable;

	public override void Deserialize(XContainer container = null)
	{
		XElement obj = ((XContainer)XDocument.Parse(tutorialDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("TutorialDefinitions"));
		TutorialsDefinitions = new Dictionary<string, TutorialDefinition>();
		foreach (XElement item in ((XContainer)obj).Elements(XName.op_Implicit("TutorialDefinition")))
		{
			TutorialDefinition tutorialDefinition = new TutorialDefinition((XContainer)(object)item);
			TutorialsDefinitions.Add(tutorialDefinition.Id, tutorialDefinition);
		}
	}
}
