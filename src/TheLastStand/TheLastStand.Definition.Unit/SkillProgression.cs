using System;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Meta;

namespace TheLastStand.Definition.Unit;

public class SkillProgression
{
	private SkillDefinition skillDefinitionCache;

	public string Id { get; }

	public SkillDefinition SkillDefinition
	{
		get
		{
			if (skillDefinitionCache == null)
			{
				skillDefinitionCache = DictionaryExtensions.GetValueOrDefault<string, SkillDefinition>(SkillDatabase.SkillDefinitions, Id);
			}
			return skillDefinitionCache;
		}
	}

	public int UnlockedAtDay { get; }

	public int LockedAtDay { get; }

	public GlyphManager.E_SkillProgressionFlag GlyphFlagUnlock { get; }

	private SkillProgression(string id, int unlockedAtDay, int lockedAtDay, GlyphManager.E_SkillProgressionFlag glyphFlagUnlock)
	{
		Id = id;
		UnlockedAtDay = unlockedAtDay;
		LockedAtDay = lockedAtDay;
		GlyphFlagUnlock = glyphFlagUnlock;
	}

	public static SkillProgression Deserialize(XElement container)
	{
		XAttribute obj = container.Attribute(XName.op_Implicit("Id"));
		XAttribute obj2 = container.Attribute(XName.op_Implicit("UnlockedAtDay"));
		string text = ((obj2 != null) ? obj2.Value : null);
		XAttribute obj3 = container.Attribute(XName.op_Implicit("LockedAtDay"));
		string text2 = ((obj3 != null) ? obj3.Value : null);
		XAttribute obj4 = container.Attribute(XName.op_Implicit("GlyphFlagUnlock"));
		string value = ((obj4 != null) ? obj4.Value : null);
		int unlockedAtDay = ((!string.IsNullOrEmpty(text)) ? int.Parse(text) : (-1));
		int lockedAtDay = ((!string.IsNullOrEmpty(text2)) ? int.Parse(text2) : (-1));
		if (string.IsNullOrEmpty(value) || !Enum.TryParse<GlyphManager.E_SkillProgressionFlag>(value, out var result))
		{
			result = GlyphManager.E_SkillProgressionFlag.None;
		}
		return new SkillProgression(obj.Value, unlockedAtDay, lockedAtDay, result);
	}

	public bool AreConditionsValid(int customDayNumber, int minimumUnlockedAtDay)
	{
		bool num = (UnlockedAtDay == -1 || customDayNumber >= UnlockedAtDay) && (LockedAtDay == -1 || customDayNumber < LockedAtDay) && minimumUnlockedAtDay <= UnlockedAtDay;
		bool flag = GlyphFlagUnlock == GlyphManager.E_SkillProgressionFlag.None || (TPSingleton<GlyphManager>.Instance.SkillProgressionFlag & GlyphFlagUnlock) != 0;
		return num && flag;
	}
}
