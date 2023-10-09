using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.BonePile;

public class BonePileGeneratorDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string BlockingBonePilesBuildingsIdsList = "BlockingBonePilesBuildings";
	}

	public struct BoneGroup
	{
		public int Tier;

		public bool Elite;

		public List<BonePileGenerationInfo> BonePileGenerationInfo;
	}

	public struct BonePileGenerationInfo
	{
		public string BuildingId;

		public int AddedPercentage;
	}

	public string ZoneId { get; private set; }

	public List<BoneGroup> BoneGroupGenerations { get; } = new List<BoneGroup>();


	public BonePileGeneratorDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public bool TryGetGroup(int tier, bool elite, out BoneGroup boneGroup)
	{
		for (int num = BoneGroupGenerations.Count - 1; num >= 0; num--)
		{
			boneGroup = BoneGroupGenerations[num];
			if (boneGroup.Tier == tier && boneGroup.Elite == elite)
			{
				return true;
			}
		}
		boneGroup = default(BoneGroup);
		return false;
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("ZoneId"));
		ZoneId = val.Value;
		foreach (XElement item in obj.Elements(XName.op_Implicit("BoneGroup")))
		{
			DeserializeBoneGroup(item);
		}
	}

	private void DeserializeBoneGroup(XElement boneGroupElement)
	{
		XAttribute val = boneGroupElement.Attribute(XName.op_Implicit("Tier"));
		if (!int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse BoneGroup Tier attribute value " + val.Value + " to a valid int!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XAttribute val2 = boneGroupElement.Attribute(XName.op_Implicit("Elite"));
		if (!bool.TryParse(val2.Value, out var result2))
		{
			CLoggerManager.Log((object)("Could not parse BoneGroup Elite attribute value " + val2.Value + " to a valid bool!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		List<BonePileGenerationInfo> list = new List<BonePileGenerationInfo>();
		foreach (XElement item in ((XContainer)boneGroupElement).Elements(XName.op_Implicit("BonePile")))
		{
			string value = item.Attribute(XName.op_Implicit("Id")).Value;
			XAttribute val3 = item.Attribute(XName.op_Implicit("AddedPercentage"));
			if (!int.TryParse(val3.Value, out var result3))
			{
				CLoggerManager.Log((object)("Could not parse BonePile AddedPercentage attribute value " + val3.Value + " to a valid int!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			list.Add(new BonePileGenerationInfo
			{
				BuildingId = value,
				AddedPercentage = result3
			});
		}
		BoneGroupGenerations.Add(new BoneGroup
		{
			Tier = result,
			Elite = result2,
			BonePileGenerationInfo = list
		});
	}
}
