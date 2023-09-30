using System.Collections.Generic;
using System.IO;
using Steamworks;
using TMPro;
using TPLib.Localization.Fonts;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using TheLastStand.Model.Modding.Module;
using UnityEngine;

namespace TheLastStand.Controller.Modding;

public class ModController
{
	public Mod Mod { get; private set; }

	public ModController(DirectoryInfo directory, SteamUGCDetails_t? steamItem = null)
	{
		Mod = new Mod(this, directory, steamItem);
	}

	public bool CanBeUploaded(out WorkshopItemHandler.E_AbortUploadReason uploadErrorReason)
	{
		if (!Mod.HasManifest)
		{
			uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.MissingManifestFile;
			return false;
		}
		if (string.IsNullOrEmpty(Mod.Title))
		{
			uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.MissingModTitle;
			return false;
		}
		if (Mod.Location != 0)
		{
			uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.Unsupported;
			return false;
		}
		if (Mod.Version < ModManager.ModMinVersion || Mod.Version > ModManager.ModVersion)
		{
			uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.BadVersion;
			return false;
		}
		if (Mod.Modules == null || Mod.Modules.Count == 0)
		{
			uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.NoModules;
			return false;
		}
		if (Mod.Thumbnail != null)
		{
			if (!IsThumbnailExtensionAuthorized(Mod.Thumbnail))
			{
				uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.ThumbnailWrongFileType;
				return false;
			}
			if (Mod.Thumbnail.Length > 1000000)
			{
				uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.ThumbnailSizeExceeded;
				return false;
			}
		}
		uploadErrorReason = WorkshopItemHandler.E_AbortUploadReason.None;
		return true;
	}

	public bool HasModule<T>() where T : ModdingModule
	{
		for (int i = 0; i < Mod.Modules.Count; i++)
		{
			if (Mod.Modules[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetModule<T>(out T module) where T : ModdingModule
	{
		for (int i = 0; i < Mod.Modules.Count; i++)
		{
			if (Mod.Modules[i] is T val)
			{
				module = val;
				return true;
			}
		}
		module = null;
		return false;
	}

	public Style GetFontTags()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		if (TryGetModule<FontModule>(out var module) && module.FontConfigDefinition.UseFontAssemblyInModList)
		{
			FontAssembly val = null;
			foreach (List<FontAssembly> value in FontManager.EveryFontAssemblies.Values)
			{
				for (int i = 0; i < value.Count; i++)
				{
					FontAssembly val2 = value[i];
					if (val2.Id == module.FontConfigDefinition.FontAssemblyToUseInModList)
					{
						val = val2;
					}
				}
			}
			if (val == null)
			{
				return new Style((string)null, (string)null, (string)null);
			}
			Material val3 = default(Material);
			TMP_FontAsset val4 = default(TMP_FontAsset);
			string text = default(string);
			if (val.FontAssets.Find((FontAssets x) => x.Importance == 1).Id != "Default_1" && FontManager.TryGetFontAndMaterial("Outline", 1, val, ref val3, ref val4, ref text))
			{
				return new Style("<font=\"" + ((Object)val4).name + "\">", "</font>");
			}
		}
		return new Style((string)null, (string)null, (string)null);
	}

	private bool IsThumbnailExtensionAuthorized(FileInfo thumbnail)
	{
		if (!(thumbnail.Extension == ".png") && !(thumbnail.Extension == ".jpg") && !(thumbnail.Extension == ".jpeg"))
		{
			return thumbnail.Extension == ".gif";
		}
		return true;
	}
}
