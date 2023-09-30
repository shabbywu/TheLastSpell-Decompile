using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.BonePile;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class BonePileDatabase : Database<BonePileDatabase>
{
	[SerializeField]
	private TextAsset boneZoneDefinitions;

	[SerializeField]
	private TextAsset bonePileGeneratorsDefinitions;

	public static BoneZonesDefinition BoneZonesDefinition { get; private set; }

	public static BonePileGeneratorsDefinition BonePileGeneratorsDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		DeserializeBoneZoneDefinitions();
		DeserializeBonePileGeneratorsDefinitions();
	}

	private void DeserializeBoneZoneDefinitions()
	{
		if (BoneZonesDefinition == null)
		{
			XElement val = ((XContainer)XDocument.Parse(boneZoneDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BoneZoneDefinitions"));
			if (val == null)
			{
				CLoggerManager.Log((object)"boneZoneDefinitions document must have a BoneZoneDefinitions Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				BoneZonesDefinition = new BoneZonesDefinition((XContainer)(object)val);
			}
		}
	}

	private void DeserializeBonePileGeneratorsDefinitions()
	{
		if (BonePileGeneratorsDefinition == null)
		{
			XElement val = ((XContainer)XDocument.Parse(bonePileGeneratorsDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("BonePileGeneratorDefinitions"));
			if (val == null)
			{
				CLoggerManager.Log((object)"bonePileGeneratorsDefinitions document must have a BonePileGeneratorDefinitions Element", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				BonePileGeneratorsDefinition = new BonePileGeneratorsDefinition((XContainer)(object)val);
			}
		}
	}
}
