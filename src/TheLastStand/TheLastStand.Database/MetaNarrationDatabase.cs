using System.Xml.Linq;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class MetaNarrationDatabase : Database<MetaNarrationDatabase>
{
	[SerializeField]
	private TextAsset narrationDefinition;

	public static MetaNarrationDefinition DarkGoddessNarrationDefinition { get; private set; }

	public static MetaNarrationDefinition LightGoddessNarrationDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement obj = ((XContainer)XDocument.Parse(narrationDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("MetaNarrationDefinition"));
		DarkGoddessNarrationDefinition = new MetaNarrationDefinition((XContainer)(object)((XContainer)obj).Element(XName.op_Implicit("Dark")));
		LightGoddessNarrationDefinition = new MetaNarrationDefinition((XContainer)(object)((XContainer)obj).Element(XName.op_Implicit("Light")));
	}
}
