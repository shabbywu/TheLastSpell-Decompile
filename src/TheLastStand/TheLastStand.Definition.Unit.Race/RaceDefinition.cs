using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Race;

public class RaceDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string AnimatorControllerName = "{0}_Template";

		public const string DefaultRaceId = "Human";

		public const string HumanRaceId = "Human";

		public const string DwarfRaceId = "Dwarf";

		public const string DefaultNamesFolderPath = "TextAssets/Races/";

		public const string DefaultNamesFileName = "{0}NameDefinitions-{1}";

		public const string DefaultIconsFolderPath = "View/Sprites/UI/Races/";

		public const string IconName = "Icon_Race_{0}_On";

		public const string IconHoveredName = "Icon_Race_{0}_Hovered";
	}

	private Sprite cachedRaceSprite;

	private Sprite cachedRaceHoveredSprite;

	public string AnimatorName => $"{Id}_Template";

	public string Id { get; private set; }

	public string Name => Localizer.Get("RaceTooltip_" + Id);

	public bool OverrideUnitAnimator { get; private set; }

	public HashSet<string> PerksIds { get; private set; }

	public Dictionary<string, List<string>> PlayableUnitNames { get; private set; }

	public Sprite RaceSprite
	{
		get
		{
			if (cachedRaceSprite == null)
			{
				cachedRaceSprite = GetIcon();
			}
			return cachedRaceSprite;
		}
	}

	public Sprite RaceHoveredSprite
	{
		get
		{
			if (cachedRaceHoveredSprite == null)
			{
				cachedRaceHoveredSprite = GetIcon(isHoveredState: true);
			}
			return cachedRaceHoveredSprite;
		}
	}

	public List<UnitTraitDefinition.StatModifier> StatModifiers { get; private set; } = new List<UnitTraitDefinition.StatModifier>();


	public RaceDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		PlayableUnitNames = new Dictionary<string, List<string>>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("OverrideNames")))
		{
			DeserializeNames(item);
		}
		if (((XContainer)val).Element(XName.op_Implicit("OverrideUnitAnimator")) != null)
		{
			OverrideUnitAnimator = true;
		}
		PerksIds = new HashSet<string>();
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Perks"));
		if (val3 != null)
		{
			foreach (XElement item2 in ((XContainer)val3).Elements())
			{
				if (item2.IsNullOrEmpty())
				{
					CLoggerManager.Log((object)("A Perk in race " + Id + " is Empty !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else if (!PerksIds.Contains(item2.Value))
				{
					PerksIds.Add(item2.Value);
				}
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("StatModifiers"));
		if (val4 == null)
		{
			return;
		}
		foreach (XElement item3 in ((XContainer)val4).Elements(XName.op_Implicit("StatModifier")))
		{
			XAttribute val5 = item3.Attribute(XName.op_Implicit("Stat"));
			if (val5 == null)
			{
				Debug.LogError((object)("The StatModifier has no stat in " + Id + " RaceDefinition !"));
				continue;
			}
			XAttribute val6 = item3.Attribute(XName.op_Implicit("DescOverrideKey"));
			string descriptionOverrideKey = ((val6 != null) ? val6.Value : string.Empty);
			StatModifiers.Add(new UnitTraitDefinition.StatModifier((UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), val5.Value), float.Parse(item3.Value, NumberStyles.Float, CultureInfo.InvariantCulture), descriptionOverrideKey));
		}
	}

	public string GetDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		if (StatModifiers != null && StatModifiers.Count > 0)
		{
			int count = StatModifiers.Count;
			for (int i = 0; i < count; i++)
			{
				stringBuilder.Append(StatModifiers[i].GetDescription(getStylizedStatNames: true));
				if (i + 1 < count)
				{
					stringBuilder.AppendLine();
				}
			}
		}
		if (PerksIds.Count > 0)
		{
			int count2 = PerksIds.Count;
			int num = 0;
			foreach (string perksId in PerksIds)
			{
				if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(perksId, out var value))
				{
					stringBuilder2.Append(FormatPerkName(value));
					if (num + 1 < count2)
					{
						stringBuilder2.AppendLine();
					}
				}
				num++;
			}
		}
		return Localizer.Format("RaceTooltipDescription_" + Id, new object[2]
		{
			stringBuilder.ToString(),
			stringBuilder2.ToString()
		});
	}

	public List<string> GetNamesForGender(string gender)
	{
		if (PlayableUnitNames.TryGetValue(gender, out var value))
		{
			return value;
		}
		return PlayableUnitDatabase.GetNamesForGender(gender);
	}

	private void DeserializeNames(XElement xNames)
	{
		string value = xNames.Attribute(XName.op_Implicit("Gender")).Value;
		if (string.IsNullOrEmpty(value) || (!string.Equals(value, "Male", StringComparison.Ordinal) && !string.Equals(value, "Female", StringComparison.Ordinal)))
		{
			Debug.LogError((object)("The Gender for Names in RaceDefinition '" + Id + "' is invalid"));
		}
		string text = $"{Id}NameDefinitions-{value}";
		XAttribute val = xNames.Attribute(XName.op_Implicit("FileName"));
		if (val != null)
		{
			text = val.Value;
		}
		List<string> list = null;
		TextAsset val2 = ResourcePooler.LoadOnce<TextAsset>("TextAssets/Races/" + text, failSilently: false);
		if ((Object)(object)val2 != (Object)null)
		{
			list = new List<string>(val2.text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)).ToList();
		}
		if (list != null)
		{
			AddNames(value, list);
		}
	}

	private void AddNames(string gender, List<string> namesList)
	{
		if (PlayableUnitNames.ContainsKey(gender))
		{
			PlayableUnitNames[gender].AddRange(namesList);
		}
		else
		{
			PlayableUnitNames.Add(gender, namesList);
		}
	}

	private string FormatPerkName(PerkDefinition perkDefinition)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("<style=GoodNbOutlined>+</style> ");
		stringBuilder.Append(perkDefinition.ColorizedName);
		return stringBuilder.ToString();
	}

	private Sprite GetIcon(bool isHoveredState = false)
	{
		Sprite val = null;
		string iconName = GetIconName(isHoveredState, Id);
		val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Races/" + iconName, failSilently: false);
		if ((Object)(object)val == (Object)null)
		{
			iconName = GetIconName(isHoveredState, "Human");
			val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Races/" + iconName, failSilently: false);
		}
		return val;
	}

	private string GetIconName(bool isHoveredState, string raceId)
	{
		return string.Format(isHoveredState ? "Icon_Race_{0}_Hovered" : "Icon_Race_{0}_On", raceId);
	}
}
