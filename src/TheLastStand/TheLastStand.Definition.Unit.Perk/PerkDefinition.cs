using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class PerkDefinition : LocalizableDefinition
{
	public class ModdedPerkDefinitionData
	{
		public readonly string IconFolderExternalPath;

		public readonly string IconFileName;

		public static readonly (int width, int height) IconDimensions = (42, 42);

		public string IconExternalFullPath => Path.Combine(IconFolderExternalPath, IconFileName);

		public ModdedPerkDefinitionData(string iconFolderExternalPath, string iconFileName)
		{
			IconFolderExternalPath = iconFolderExternalPath;
			IconFileName = iconFileName;
		}
	}

	private Sprite cachedPerkSprite;

	public bool DisplayBonusBeforePurchase { get; private set; }

	public bool DisplayInHUD { get; private set; }

	public Node HudBonus { get; private set; }

	public Node HudBuffer { get; private set; }

	public List<Node> HoverRanges { get; private set; }

	public bool IsModded => ModdingData != null;

	public ModdedPerkDefinitionData ModdingData { get; set; }

	public string Name => Localizer.Get("PerkName_" + Id);

	public string Id { get; private set; }

	public PerkDataConditionsDefinition GreyOutConditionsDefinition { get; private set; }

	public PerkDataConditionsDefinition HighlightConditionsDefinition { get; private set; }

	public PerkDataConditionsDefinition FeedbackActivationConditionsDefinition { get; private set; }

	public string PerkEffectsInformations => "PerkEffectInformations_" + Id;

	public bool PerkEffectsInformationsExist { get; private set; }

	public List<APerkModuleDefinition> PerkModuleDefinitions { get; private set; }

	public Sprite PerkSprite
	{
		get
		{
			if (cachedPerkSprite == null)
			{
				cachedPerkSprite = GetIcon();
			}
			return cachedPerkSprite;
		}
	}

	public HashSet<CompendiumEntryDefinition> CompendiumEntries { get; private set; }

	public List<Tuple<string, int>> SkillsToShow { get; private set; }

	public PerkDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement xTokenVariables = obj.Element(XName.op_Implicit("TokenVariables"));
		DeserializeTokenVariables(xTokenVariables);
		XElement container2 = obj.Element(XName.op_Implicit("LocArguments"));
		base.Deserialize((XContainer)(object)container2);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		PerkEffectsInformationsExist = Localizer.Exists(PerkEffectsInformations);
		CompendiumEntries = new HashSet<CompendiumEntryDefinition>();
		HoverRanges = new List<Node>();
		SkillsToShow = new List<Tuple<string, int>>();
		XElement val2 = obj.Element(XName.op_Implicit("View"));
		if (val2 != null)
		{
			DeserializeView(val2);
		}
		XElement xModules = obj.Element(XName.op_Implicit("Modules"));
		DeserializeModules(xModules);
	}

	private void DeserializeModules(XElement xModules)
	{
		PerkModuleDefinitions = new List<APerkModuleDefinition>();
		foreach (XElement item in ((XContainer)xModules).Elements())
		{
			switch (item.Name.LocalName)
			{
			case "BufferModule":
				PerkModuleDefinitions.Add(new BufferModuleDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			case "GaugeModule":
				PerkModuleDefinitions.Add(new GaugeModuleDefinition((XContainer)(object)item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log((object)("Tried to Deserialize an unimplemented PerkModule: " + item.Name.LocalName), (LogType)0, (CLogLevel)2, true, "PerkDefinition", false);
				break;
			}
		}
	}

	private void DeserializeView(XElement xView)
	{
		XElement val = ((XContainer)xView).Element(XName.op_Implicit("CompendiumEntries"));
		if (val != null)
		{
			foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("CompendiumEntry")))
			{
				CompendiumEntries.Add(new CompendiumEntryDefinition((XContainer)(object)item));
			}
		}
		XElement val2 = ((XContainer)xView).Element(XName.op_Implicit("SkillsToShow"));
		if (val2 != null)
		{
			foreach (XElement item2 in ((XContainer)val2).Elements(XName.op_Implicit("SkillToShow")))
			{
				XAttribute val3 = item2.Attribute(XName.op_Implicit("Id"));
				XAttribute val4 = item2.Attribute(XName.op_Implicit("OverallUses"));
				int result = -1;
				if (val4 != null && !int.TryParse(val4.Value, out result))
				{
					CLoggerManager.Log((object)"Could not parse OverallUses attribute into an int", (LogType)0, (CLogLevel)2, true, "PerkDefinition", false);
					result = -1;
				}
				SkillsToShow.Add(new Tuple<string, int>(val3.Value, result));
			}
		}
		DisplayBonusBeforePurchase = ((XContainer)xView).Element(XName.op_Implicit("DisplayBonusBeforePurchase")) != null;
		XElement val5 = ((XContainer)xView).Element(XName.op_Implicit("DisplayInHUD"));
		DisplayInHUD = val5 != null;
		if (val5 != null)
		{
			XElement val6 = ((XContainer)val5).Element(XName.op_Implicit("Bonus"));
			if (val6 != null)
			{
				XAttribute val7 = val6.Attribute(XName.op_Implicit("Value"));
				HudBonus = Parser.Parse(val7.Value, base.TokenVariables);
			}
			XElement val8 = ((XContainer)val5).Element(XName.op_Implicit("Buffer"));
			if (val8 != null)
			{
				XAttribute val9 = val8.Attribute(XName.op_Implicit("Value"));
				HudBuffer = Parser.Parse(val9.Value, base.TokenVariables);
			}
			GreyOutConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)val5).Element(XName.op_Implicit("GreyOut")), base.TokenVariables);
			HighlightConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)val5).Element(XName.op_Implicit("Highlight")), base.TokenVariables);
			XElement val10 = ((XContainer)val5).Element(XName.op_Implicit("HoverDisplay"));
			if (val10 != null)
			{
				foreach (XElement item3 in ((XContainer)val10).Elements(XName.op_Implicit("HoverRange")))
				{
					XAttribute val11 = item3.Attribute(XName.op_Implicit("Range"));
					HoverRanges.Add(Parser.Parse(val11.Value, base.TokenVariables));
				}
			}
		}
		FeedbackActivationConditionsDefinition = new PerkDataConditionsDefinition((XContainer)(object)((XContainer)xView).Element(XName.op_Implicit("FeedbackActivationConditions")), base.TokenVariables);
	}

	public string GetAdditionDescription(InterpreterContext interpreterContext)
	{
		if (base.LocArguments == null)
		{
			return Localizer.Get(PerkEffectsInformations);
		}
		return Localizer.Format(PerkEffectsInformations, GetArguments(interpreterContext));
	}

	public string GetDescription(InterpreterContext interpreterContext)
	{
		if (base.LocArguments == null)
		{
			return Localizer.Get("PerkDescription_" + Id);
		}
		return Localizer.Format("PerkDescription_" + Id, GetArguments(interpreterContext));
	}

	public Sprite GetIcon()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = null;
		if (IsModded)
		{
			if (File.Exists(ModdingData.IconExternalFullPath))
			{
				byte[] array = File.ReadAllBytes(ModdingData.IconExternalFullPath);
				Texture2D val2 = new Texture2D(ModdedPerkDefinitionData.IconDimensions.width, ModdedPerkDefinitionData.IconDimensions.height, (TextureFormat)5, false);
				ImageConversion.LoadImage(val2, array);
				((Object)val2).name = Path.GetFileNameWithoutExtension(ModdingData.IconFileName);
				val = Sprite.Create(val2, new Rect(0f, 0f, (float)((Texture)val2).width, (float)((Texture)val2).height), new Vector2(0.5f, 0.5f));
			}
		}
		else
		{
			val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Perks/" + Id, failSilently: false);
		}
		if ((Object)(object)val == (Object)null)
		{
			val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Perks/Default", failSilently: false);
		}
		return val;
	}
}
