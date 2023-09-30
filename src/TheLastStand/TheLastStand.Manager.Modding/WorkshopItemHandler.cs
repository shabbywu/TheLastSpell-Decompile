using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using TPLib;
using TPLib.Log;
using TheLastStand.Model.Modding;
using TheLastStand.View.Modding;
using UnityEngine.Events;

namespace TheLastStand.Manager.Modding;

public static class WorkshopItemHandler
{
	public enum E_AbortUploadReason
	{
		None,
		Unsupported,
		ThumbnailSizeExceeded,
		ThumbnailWrongFileType,
		MissingManifestFile,
		MissingModTitle,
		BadVersion,
		NoModules
	}

	public class Constants
	{
		public const string SteamWorkshopIdFileName = "/steam_workshop.txt";
	}

	public class OnUploadSteamItemEvent : UnityEvent<SubmitItemUpdateResult_t>
	{
	}

	public class OnCheckCreatedItemsByUserEvent : UnityEvent<List<ulong>>
	{
	}

	private static bool initialized = false;

	private static CallResult<CreateItemResult_t> onCreateItemCallbackResult = null;

	private static CallResult<DeleteItemResult_t> onDeleteItemCallbackResult = null;

	private static CallResult<SubmitItemUpdateResult_t> onSubmitItemCreationUpdateResultCallResult = null;

	private static CallResult<SubmitItemUpdateResult_t> onSubmitItemUpdateResultCallResult = null;

	private static UGCUpdateHandle_t updateHandle = default(UGCUpdateHandle_t);

	public static OnCheckCreatedItemsByUserEvent OnCheckCreatedItemsByUser = new OnCheckCreatedItemsByUserEvent();

	public static OnUploadSteamItemEvent OnUploadSteamItem = new OnUploadSteamItemEvent();

	public static OnUploadSteamItemEvent OnFailUploadSteamItem = new OnUploadSteamItemEvent();

	public static Mod ModToUpload { get; set; }

	public static void CreateItem(Mod mod)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		ModToUpload = mod;
		if (SteamManager.Initialized && ModToUpload != null)
		{
			if (!initialized)
			{
				Init();
			}
			SteamAPICall_t val = SteamUGC.CreateItem(SteamUtils.GetAppID(), (EWorkshopFileType)0);
			onCreateItemCallbackResult.Set(val, (APIDispatchDelegate<CreateItemResult_t>)null);
		}
	}

	public static void DeleteItem(PublishedFileId_t publishedFileId_T)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Initialized)
		{
			SteamAPICall_t val = SteamUGC.DeleteItem(publishedFileId_T);
			onDeleteItemCallbackResult.Set(val, (APIDispatchDelegate<DeleteItemResult_t>)null);
		}
	}

	public static void ModifyItem(Mod mod, string changeNote = null)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		ModToUpload = mod;
		if (SteamManager.Initialized && ModToUpload != null)
		{
			if (!initialized)
			{
				Init();
			}
			updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), new PublishedFileId_t(ModToUpload.WorkshopId));
			if (ModToUpload.MustOverrideSteamDatas)
			{
				SteamUGC.SetItemTitle(updateHandle, ModToUpload.Title);
				SteamUGC.SetItemDescription(updateHandle, ModToUpload.Description);
			}
			if (ModToUpload.Thumbnail != null)
			{
				SteamUGC.SetItemPreview(updateHandle, ModToUpload.Thumbnail.FullName);
			}
			SteamUGC.SetItemContent(updateHandle, ModToUpload.DirectoryInfo.FullName);
			SteamAPICall_t val = SteamUGC.SubmitItemUpdate(updateHandle, SteamFriends.GetPersonaName() + " : " + (changeNote ?? $"Update of this mod ({ModToUpload.WorkshopId}) ! Mod Title : {ModToUpload.Title}."));
			onSubmitItemUpdateResultCallResult.Set(val, (APIDispatchDelegate<SubmitItemUpdateResult_t>)null);
		}
	}

	private static void Init()
	{
		onCreateItemCallbackResult = CallResult<CreateItemResult_t>.Create((APIDispatchDelegate<CreateItemResult_t>)OnCreateItemResult);
		onDeleteItemCallbackResult = CallResult<DeleteItemResult_t>.Create((APIDispatchDelegate<DeleteItemResult_t>)OnDeleteItemResult);
		onSubmitItemCreationUpdateResultCallResult = CallResult<SubmitItemUpdateResult_t>.Create((APIDispatchDelegate<SubmitItemUpdateResult_t>)OnSubmitItemCreationUpdateResult);
		onSubmitItemUpdateResultCallResult = CallResult<SubmitItemUpdateResult_t>.Create((APIDispatchDelegate<SubmitItemUpdateResult_t>)OnSubmitItemUpdateResult);
	}

	private static async void CreateSteamWorkshopFile(PublishedFileId_t m_nPublishedFileId, Action onCompleted)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		FileStream file = File.Open(ModToUpload.DirectoryInfo?.ToString() + "/steam_workshop.txt", FileMode.OpenOrCreate, FileAccess.Write);
		StreamWriter writer = new StreamWriter(file);
		await writer.WriteLineAsync(m_nPublishedFileId.m_PublishedFileId.ToString().Replace("\0", string.Empty));
		writer.Close();
		file.Close();
		onCompleted?.Invoke();
	}

	private static void OnCreateItemResult(CreateItemResult_t createItemResult, bool bIOFailure)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		string changeNoteValue = TPSingleton<ModsView>.Instance.GetChangeNoteValue();
		updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), createItemResult.m_nPublishedFileId);
		SteamUGC.SetItemTitle(updateHandle, ModToUpload.Title);
		SteamUGC.SetItemDescription(updateHandle, ModToUpload.Description);
		SteamUGC.SetItemContent(updateHandle, ModToUpload.DirectoryInfo.FullName);
		if (ModToUpload.Thumbnail != null)
		{
			SteamUGC.SetItemPreview(updateHandle, ModToUpload.Thumbnail.FullName);
		}
		SteamAPICall_t val = SteamUGC.SubmitItemUpdate(updateHandle, SteamFriends.GetPersonaName() + " : " + (changeNoteValue ?? $"Creation of this mod ({createItemResult.m_nPublishedFileId.m_PublishedFileId}) ! Mod Title : {ModToUpload.Title}."));
		onSubmitItemCreationUpdateResultCallResult.Set(val, (APIDispatchDelegate<SubmitItemUpdateResult_t>)null);
	}

	private static void OnDeleteItemResult(DeleteItemResult_t deleteItemResult, bool bIOFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((int)deleteItemResult.m_eResult == 1)
		{
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"This item({deleteItemResult.m_nPublishedFileId}) is deleted !", (CLogLevel)2, false, false);
		}
		else
		{
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).LogWarning((object)$"Failed to delete this item({deleteItemResult.m_nPublishedFileId}) !", (CLogLevel)2, true, false);
		}
	}

	private static void OnSubmitItemCreationUpdateResult(SubmitItemUpdateResult_t submitItemUpdateResult, bool bIOFailure)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if ((int)submitItemUpdateResult.m_eResult == 1)
		{
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"This item ({submitItemUpdateResult.m_nPublishedFileId}) is published !", (CLogLevel)2, false, false);
			CreateSteamWorkshopFile(submitItemUpdateResult.m_nPublishedFileId, delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				((UnityEvent<SubmitItemUpdateResult_t>)OnUploadSteamItem).Invoke(submitItemUpdateResult);
			});
		}
		else
		{
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).LogWarning((object)$"Failed to publish this item ({submitItemUpdateResult.m_nPublishedFileId}) ! Reason : {submitItemUpdateResult.m_eResult}", (CLogLevel)2, true, false);
			((UnityEvent<SubmitItemUpdateResult_t>)OnFailUploadSteamItem)?.Invoke(submitItemUpdateResult);
			DeleteItem(submitItemUpdateResult.m_nPublishedFileId);
		}
	}

	private static void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t submitItemUpdateResult, bool bIOFailure)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if ((int)submitItemUpdateResult.m_eResult == 1)
		{
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).Log((object)$"This item ({submitItemUpdateResult.m_nPublishedFileId}) is updated !", (CLogLevel)2, false, false);
			((UnityEvent<SubmitItemUpdateResult_t>)OnUploadSteamItem).Invoke(submitItemUpdateResult);
		}
		else
		{
			((CLogger<ModManager>)TPSingleton<ModManager>.Instance).LogWarning((object)$"Failed to update this item ({submitItemUpdateResult.m_nPublishedFileId}) ! Reason : {submitItemUpdateResult.m_eResult}", (CLogLevel)2, true, false);
			((UnityEvent<SubmitItemUpdateResult_t>)OnFailUploadSteamItem)?.Invoke(submitItemUpdateResult);
		}
	}
}
