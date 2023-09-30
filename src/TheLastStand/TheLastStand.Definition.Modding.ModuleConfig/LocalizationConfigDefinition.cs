using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Localization.ScriptableObjects;
using TPLib.Log;

namespace TheLastStand.Definition.Modding.ModuleConfig;

public class LocalizationConfigDefinition : ModuleConfigDefinition
{
	private class LocalizationConfigConstants
	{
		public const string FontAssembliesBindingsElementName = "FontAssembliesBindings";

		public const string LanguagesAndFontAssembliesElementName = "LanguagesAndFontAssemblies";

		public const string LanguageIdsElementName = "LanguageIds";

		public const string FontAssemblyIdsElementName = "FontAssemblyIds";

		public const string FontAssemblyIdElementName = "FontAssemblyId";

		public const string CustomLanguageFormat = "{0} [CUSTOM]";
	}

	private Dictionary<string, List<string>> languagesLinkedToFontAssemblies = new Dictionary<string, List<string>>();

	public Dictionary<string, List<string>> LanguageLinkedToFontPack => languagesLinkedToFontAssemblies;

	public LocalizationConfigDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("FontAssembliesBindings"));
		if (val == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("LanguagesAndFontAssemblies")))
		{
			XElement val2 = ((XContainer)item).Element(XName.op_Implicit("LanguageIds"));
			XElement val3 = ((XContainer)item).Element(XName.op_Implicit(FontSettings.LanguageCanSupportsMultipleFontAssembly ? "FontAssemblyIds" : "FontAssemblyId"));
			string[] array = RetrieveStrings(val2.Value);
			string[] array2 = RetrieveStrings(val3.Value);
			for (int i = 0; i < array.Length; i++)
			{
				string language = array[i];
				if (Localizer.knownLanguages.FirstOrDefault((string x) => x == language) == null)
				{
					language = $"{language} [CUSTOM]";
					if (!languagesLinkedToFontAssemblies.ContainsKey(language))
					{
						languagesLinkedToFontAssemblies.Add(language, new List<string>());
					}
				}
				else if (!languagesLinkedToFontAssemblies.ContainsKey(language))
				{
					languagesLinkedToFontAssemblies.Add(language, new List<string>());
				}
				for (int j = 0; j < array2.Length; j++)
				{
					if (!languagesLinkedToFontAssemblies[language].Contains(array2[j]))
					{
						languagesLinkedToFontAssemblies[language].Add(array2[j]);
					}
				}
			}
		}
	}

	private string[] RetrieveStrings(string value)
	{
		string[] array = value.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Replace("\n", string.Empty);
			array[i] = array[i].Replace("\r", string.Empty);
			array[i] = array[i].Replace("\t", string.Empty);
			array[i] = array[i].Trim();
		}
		if (array.Length > 1 && FontSettings.LanguageCanSupportsMultipleFontAssembly)
		{
			((CLogger<FontManager>)(object)TPSingleton<FontManager>.Instance).Log((object)"LanguagesAndFontAssemblies can't supports multiple FontAssembblies !", (CLogLevel)1, false, false);
			Array.Resize(ref array, 1);
		}
		return array;
	}
}
