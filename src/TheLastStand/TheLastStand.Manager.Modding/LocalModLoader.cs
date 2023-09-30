using System;
using System.IO;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Modding;
using TheLastStand.Model.Modding;
using UnityEngine;

namespace TheLastStand.Manager.Modding;

public class LocalModLoader : ModLoader<LocalModLoader>
{
	public static string ModsFolderPath = Application.persistentDataPath + "/Mods/";

	public override void Init()
	{
	}

	public override void LoadMods()
	{
		if (!Directory.Exists(ModsFolderPath))
		{
			return;
		}
		DirectoryInfo[] directories = new DirectoryInfo(ModsFolderPath).GetDirectories();
		((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"Found {directories.Length} mods from local storage !", (CLogLevel)2, false, false);
		foreach (DirectoryInfo directoryInfo in directories)
		{
			if (directoryInfo.GetDirectories() != null && directoryInfo.GetFiles() != null)
			{
				Mod mod = null;
				try
				{
					mod = new ModController(directoryInfo).Mod;
				}
				catch (Exception arg)
				{
					((CLogger<ModManager>)TPSingleton<ModManager>.Instance).LogError((object)$"An unknown error occured during loading of a mod ! Exception : {arg}", (CLogLevel)1, true, true);
				}
				if (mod != null && mod.HasManifest)
				{
					if (mod.Modules.Count > 0 && mod.Version >= ModManager.ModMinVersion)
					{
						((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)($"[LOCAL] <b>{mod}</b> is correctly installed ! Adding it to Subscribed Mods list." + mod.ModulesToString()), (CLogLevel)0, false, false);
						ModManager.SubscribedMods.Add(mod);
						continue;
					}
					((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"[LOCAL] <b>{mod}</b> is outdated (Mod Version : {mod.Version}) ! (Min Version supported : {ModManager.ModMinVersion}, Max Version supported : {ModManager.ModVersion}) Adding it to Outdated Mods list.", (CLogLevel)0, false, false);
					mod.IsIncompatible = true;
					ModManager.OutdatedMods.Add(mod);
				}
				else
				{
					((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)("A local mod can't be loaded because there is no manifest.xml file or there is no valid modules ! (Path : " + directoryInfo.FullName + ")"), (CLogLevel)0, false, false);
				}
			}
			else
			{
				((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)("A local mod can't be loaded ! (Path : " + directoryInfo.FullName + ")"), (CLogLevel)0, false, false);
			}
		}
	}
}
