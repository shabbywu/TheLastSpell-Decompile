using TheLastStand.Framework;
using UnityEngine;

namespace TheLastStand.View.Skill;

public class SkillView
{
	public static class Constants
	{
		public const string UIPlaceholderSpritePath = "View/Sprites/UI/Skills/Tmp/PatternPlaceholder";
	}

	public static Sprite GetIconSprite(string skillDefinitionId)
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/Icons/" + skillDefinitionId, false);
	}

	public static Sprite GetPatternSprite(string skillDefinitionId, int level)
	{
		Sprite val = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Skills/Patterns/{skillDefinitionId}_Lvl{level}", false);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/Tmp/PatternPlaceholder", false);
	}
}
