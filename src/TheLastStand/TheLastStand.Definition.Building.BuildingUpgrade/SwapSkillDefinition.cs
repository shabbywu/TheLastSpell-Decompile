using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class SwapSkillDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "SwapSkill";

	public string OldSkillId { get; private set; } = string.Empty;


	public string NewSkillId { get; private set; } = string.Empty;


	public SwapSkillDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XContainer obj = ((xContainer is XElement) ? xContainer : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("OldSkillId"));
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("NewSkillId"));
		if (!val.IsNullOrEmpty())
		{
			OldSkillId = val.Value;
		}
		else
		{
			TPDebug.LogError((object)(base.Id + " UpgradeEffect must have a valid Attribute OldSkillId (token)"), (Object)null);
		}
		if (!val2.IsNullOrEmpty())
		{
			NewSkillId = val2.Value;
		}
		else
		{
			TPDebug.LogError((object)(base.Id + " UpgradeEffect must have a valid Attribute NewSkillId (token)"), (Object)null);
		}
	}
}
