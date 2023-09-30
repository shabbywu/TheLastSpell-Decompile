using System.Collections.Generic;
using System.IO;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using TheLastStand.Model.Modding.Module;

namespace TheLastStand.Controller.Modding.Module;

public class FontModuleController : ModuleController
{
	public FontModule FontModule => module as FontModule;

	public List<ModdedFontAssembly> FontAssemblies { get; } = new List<ModdedFontAssembly>();


	public FontModuleController(DirectoryInfo directory)
		: base(directory)
	{
		module = new FontModule(this, directory);
		if (FontModule.FontConfigDefinition == null)
		{
			return;
		}
		for (int i = 0; i < FontModule.FontConfigDefinition.FontPackDefinitions.Count; i++)
		{
			ModdedFontAssembly moddedFontAssembly = new ModdedFontAssembly(FontModule.FontConfigDefinition.FontPackDefinitions[i], directory);
			if (((FontAssembly)moddedFontAssembly).FontAssets.Count > 0)
			{
				FontAssemblies.Add(moddedFontAssembly);
			}
			else
			{
				((CLogger<ModManager>)TPSingleton<ModManager>.Instance).LogError((object)("This FontPack " + FontModule.FontConfigDefinition.FontPackDefinitions[i].Id + " can't be loaded due to inexistant font files !"), (CLogLevel)1, true, true);
			}
		}
	}

	public override string ToString()
	{
		string text = "<b>Fonts Module</b> : \r\n   - Loaded FontPacks : \r\n";
		for (int i = 0; i < FontAssemblies.Count; i++)
		{
			text = text + "      " + $"* {FontAssemblies[i]}\r\n";
		}
		return text;
	}
}
