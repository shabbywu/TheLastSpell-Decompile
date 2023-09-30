using System.IO;
using TPLib.Localization.Fonts;
using TPLib.Localization.ScriptableObjects;
using TheLastStand.Definition.Modding;

namespace TheLastStand.Model.Modding;

public class ModdedFontAssembly : FontAssembly
{
	public ModdedFontAssembly(FontAssemblyDefinition fontAssemblyDefinition, DirectoryInfo directory)
		: base((EditorFontAssembly)null)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		((FontAssembly)this).Id = fontAssemblyDefinition.Id;
		for (int i = 0; i < fontAssemblyDefinition.FontAssetsDefinition.Count; i++)
		{
			FontAssets val = new FontAssets(fontAssemblyDefinition.FontAssetsDefinition[i].Importance, ((FontAssembly)this).Id, directory, fontAssemblyDefinition.FontAssetsDefinition[i].FontPath);
			if (val.IsValid())
			{
				((FontAssembly)this).FontAssets.Add(val);
			}
		}
	}
}
