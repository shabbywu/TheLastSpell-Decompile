using System;
using System.IO;
using Steamworks;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Modding;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using UnityEngine.Events;

namespace TheLastStand.Manager.SteamWorkshop;

public class SteamModLoader : ModLoader<SteamModLoader>
{
	public class Constants
	{
		public const int MaxItemsPerRequest = 50;
	}

	private CallResult<SteamUGCQueryCompleted_t> onGetSubscribedItemsQueryCompletedResult;

	private CallResult<SteamUGCQueryCompleted_t> onGetPublishedItemsQueryCompletedResult;

	private UGCQueryHandle_t getPublishedItemsQueryHandle;

	private UGCQueryHandle_t getSubscribedItemsQueryHandle;

	private bool publishedModsLoaded;

	private uint publishedItemspagesIndex = 1u;

	private uint subscribedItemsPagesIndex = 1u;

	private bool subscribedModsLoaded;

	private EUGCMatchingUGCType workshopItemType;

	public override bool ModsLoaded
	{
		get
		{
			if (subscribedModsLoaded)
			{
				return publishedModsLoaded;
			}
			return false;
		}
	}

	public UnityEvent OnPublishedItemsLoaded { get; set; } = new UnityEvent();


	public override void Init()
	{
		if (!initialized)
		{
			onGetSubscribedItemsQueryCompletedResult = CallResult<SteamUGCQueryCompleted_t>.Create((APIDispatchDelegate<SteamUGCQueryCompleted_t>)OnGetSubscribedItemsQueryCompleted);
			onGetPublishedItemsQueryCompletedResult = CallResult<SteamUGCQueryCompleted_t>.Create((APIDispatchDelegate<SteamUGCQueryCompleted_t>)OnGetPublishedItemsQueryCompleted);
			initialized = true;
		}
	}

	public bool CanUserUploadMod(Mod mod)
	{
		return ModManager.PublishedMods.Find((Mod x) => x.WorkshopId == mod.WorkshopId) != null;
	}

	public override void LoadMods()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		subscribedModsLoaded = false;
		subscribedItemsPagesIndex = 1u;
		CSteamID steamID = SteamUser.GetSteamID();
		getSubscribedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(((CSteamID)(ref steamID)).GetAccountID(), (EUserUGCList)6, (EUGCMatchingUGCType)0, (EUserUGCListSortOrder)0, AppId_t.Invalid, SteamUtils.GetAppID(), subscribedItemsPagesIndex);
		SteamAPICall_t val = SteamUGC.SendQueryUGCRequest(getSubscribedItemsQueryHandle);
		onGetSubscribedItemsQueryCompletedResult.Set(val, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
		LoadPublishedMods();
	}

	public void LoadPublishedMods()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		ModManager.PublishedMods.Clear();
		publishedModsLoaded = false;
		publishedItemspagesIndex = 1u;
		CSteamID steamID = SteamUser.GetSteamID();
		getPublishedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(((CSteamID)(ref steamID)).GetAccountID(), (EUserUGCList)0, (EUGCMatchingUGCType)0, (EUserUGCListSortOrder)0, AppId_t.Invalid, SteamUtils.GetAppID(), publishedItemspagesIndex);
		SteamAPICall_t val = SteamUGC.SendQueryUGCRequest(getPublishedItemsQueryHandle);
		onGetPublishedItemsQueryCompletedResult.Set(val, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
	}

	private void CreateMod(SteamUGCDetails_t item)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		ulong num = default(ulong);
		string path = default(string);
		uint num2 = default(uint);
		if (SteamUGC.GetItemInstallInfo(item.m_nPublishedFileId, ref num, ref path, 1024u, ref num2))
		{
			Mod mod = new ModController(new DirectoryInfo(path), item).Mod;
			if (mod.Version >= ModManager.ModMinVersion && mod.Modules.Count > 0)
			{
				((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)("[STEAM] <b>" + mod.ToString() + "</b> is correctly installed ! Adding it to Subscribed Mods list." + mod.ModulesToString()), (CLogLevel)0, false, false);
				ModManager.SubscribedMods.Add(mod);
				return;
			}
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"[STEAM] <b>{mod}</b> is outdated (Mod Version : {mod.Version}) ! (Min Version supported : {ModManager.ModMinVersion}, Max Version supported : {ModManager.ModVersion}) Adding it to Outdated Mods list.", (CLogLevel)0, false, false);
			mod.IsIncompatible = true;
			ModManager.OutdatedMods.Add(mod);
		}
	}

	private void OnGetPublishedItemsQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"Found {param.m_unNumResultsReturned} published mods by this user ! (Page : {publishedItemspagesIndex})", (CLogLevel)2, false, false);
		SteamUGCDetails_t value = default(SteamUGCDetails_t);
		for (uint num = 0u; num < param.m_unNumResultsReturned; num++)
		{
			SteamUGC.GetQueryUGCResult(getPublishedItemsQueryHandle, num, ref value);
			Mod mod = new ModController(null, value).Mod;
			ModManager.PublishedMods.Add(mod);
			text = text + " - " + mod.ToString() + mod.ModulesToString();
			if (num != param.m_unNumResultsReturned - 1)
			{
				text += "\r\n";
			}
		}
		((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"Published mods by this user (Page : {publishedItemspagesIndex}): \r\n{text}", (CLogLevel)0, false, false);
		SteamUGC.ReleaseQueryUGCRequest(getPublishedItemsQueryHandle);
		if (param.m_unNumResultsReturned == 50 && param.m_unTotalMatchingResults > 50)
		{
			publishedModsLoaded = false;
			publishedItemspagesIndex++;
			CSteamID steamID = SteamUser.GetSteamID();
			getPublishedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(((CSteamID)(ref steamID)).GetAccountID(), (EUserUGCList)0, workshopItemType, (EUserUGCListSortOrder)0, AppId_t.Invalid, SteamUtils.GetAppID(), subscribedItemsPagesIndex);
			SteamAPICall_t val = SteamUGC.SendQueryUGCRequest(getPublishedItemsQueryHandle);
			onGetPublishedItemsQueryCompletedResult.Set(val, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
		}
		else
		{
			publishedModsLoaded = true;
			UnityEvent onPublishedItemsLoaded = OnPublishedItemsLoaded;
			if (onPublishedItemsLoaded != null)
			{
				onPublishedItemsLoaded.Invoke();
			}
		}
	}

	private void OnGetSubscribedItemsQueryCompleted(SteamUGCQueryCompleted_t param, bool bIOFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		PublishedFileId_t[] array = (PublishedFileId_t[])(object)new PublishedFileId_t[param.m_unNumResultsReturned];
		SteamUGC.GetSubscribedItems(array, (uint)array.Length);
		((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"Found {param.m_unNumResultsReturned} subscribed mods from steam ! (Page : {subscribedItemsPagesIndex})", (CLogLevel)2, false, false);
		SteamUGCDetails_t val = default(SteamUGCDetails_t);
		ulong num2 = default(ulong);
		string path = default(string);
		uint num3 = default(uint);
		for (uint num = 0u; num < array.Length; num++)
		{
			SteamUGC.GetQueryUGCResult(getSubscribedItemsQueryHandle, num, ref val);
			EItemState val2 = (EItemState)SteamUGC.GetItemState(array[num]);
			SteamUGC.GetItemInstallInfo(array[num], ref num2, ref path, 1024u, ref num3);
			if (((Enum)val2).HasFlag((Enum)(object)(EItemState)4) && Directory.Exists(path))
			{
				CreateMod(val);
			}
			else
			{
				((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"This mod {val.m_rgchTitle}(Steam Workshop Id : {val.m_nPublishedFileId}) isn't correctly installed !", (CLogLevel)0, false, false);
			}
		}
		SteamUGC.ReleaseQueryUGCRequest(getSubscribedItemsQueryHandle);
		if (param.m_unNumResultsReturned == 50 && param.m_unTotalMatchingResults > 50)
		{
			subscribedModsLoaded = false;
			subscribedItemsPagesIndex++;
			CSteamID steamID = SteamUser.GetSteamID();
			getSubscribedItemsQueryHandle = SteamUGC.CreateQueryUserUGCRequest(((CSteamID)(ref steamID)).GetAccountID(), (EUserUGCList)0, workshopItemType, (EUserUGCListSortOrder)0, AppId_t.Invalid, SteamUtils.GetAppID(), subscribedItemsPagesIndex);
			SteamAPICall_t val3 = SteamUGC.SendQueryUGCRequest(getSubscribedItemsQueryHandle);
			onGetSubscribedItemsQueryCompletedResult.Set(val3, (APIDispatchDelegate<SteamUGCQueryCompleted_t>)null);
		}
		else
		{
			subscribedModsLoaded = true;
		}
	}
}
