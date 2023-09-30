using System.Collections.Generic;
using Steamworks;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Modding;
using TheLastStand.Manager.SteamWorkshop;
using TheLastStand.Model.Modding;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Modding;

public class ModItemView : MonoBehaviour
{
	public static class Constants
	{
		public const string ModNotUsed = "Modding_ModNotUsed";

		public const string UpdateInSteam = "Modding_UpdateInSteam";

		public const string UploadInSteam = "Modding_UploadInSteam";

		public const string ModLocationSteam = "Modding_ModLocation_Steam";

		public const string ModLocationLocal = "Modding_ModLocation_Local";

		public const string ItemNoTitle = "Modding_ItemNoTitle";

		public const string ItemAuthor = "Modding_ItemAuthor";

		public const string ItemNoAuthor = "Modding_ItemNoAuthor";

		public const string SteamWorkshopId = "Modding_SteamWorkshopId";

		public const string JumpLine = "\r\n";

		public const string ModdingAbortUploadTitleKey = "Modding_AbortUploadTitle";

		public const string ModdingUploadErrorTitleKey = "Modding_UploadErrorTitle";

		public const string ModdingUnsupportedErrorLabelKey = "Modding_UnsupportedErrorLabel";

		public const string ModdingAbortUploadPrefixKey = "Modding_AbortUpload_";

		public const string ModdingUpdatingItemKey = "Modding_UpdatingItem";

		public const string ModdingUploadingItemKey = "Modding_UploadingItem";
	}

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private BetterButton uploadButton;

	[SerializeField]
	private TextMeshProUGUI uploadButtonLabel;

	[SerializeField]
	private Image checkImage;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private TextMeshProUGUI uploadingLabel;

	[SerializeField]
	private RawTextTooltipDisplayer rawTextTooltipDisplayer;

	[SerializeField]
	private GenericTooltipDisplayer genericTooltipDisplayer;

	[SerializeField]
	private DataColor activeDataColor;

	[SerializeField]
	private DataColor inactiveDataColor;

	[SerializeField]
	private List<FontAssemblyLabel> modifiableFonts = new List<FontAssemblyLabel>();

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	private Mod mod;

	public List<FontAssemblyLabel> ModifiableFonts => modifiableFonts;

	public void Refresh()
	{
		Refresh(mod);
	}

	public void Refresh(Mod mod)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		this.mod = mod;
		Style fontTags = mod.ModController.GetFontTags();
		rawTextTooltipDisplayer.TargetTooltip = TPSingleton<ModsView>.Instance.RawTextTooltip;
		RefreshLabel(fontTags);
		((Behaviour)checkImage).enabled = mod.IsUsed && !mod.IsIncompatible;
		RefreshSteamUploadButton();
		rawTextTooltipDisplayer.Text = string.Empty;
		if (!mod.IsUsed)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 40f);
			if (mod.Location == Mod.E_ModLocation.STEAM)
			{
				rawTextTooltipDisplayer.Text = Localizer.Get("Modding_ModNotUsed") + "\r\n\r\n";
			}
		}
		RawTextTooltipDisplayer obj = rawTextTooltipDisplayer;
		obj.Text = obj.Text + fontTags.OpenTag + mod.Description + fontTags.CloseTag;
		AddSteamWorkshopIdInTooltip();
		RawTextTooltipDisplayer obj2 = rawTextTooltipDisplayer;
		obj2.Text = obj2.Text + "\r\n" + ((mod.Version < ModManager.ModMinVersion) ? Localizer.Format("Modding_ModBadVersion", new object[1] { mod.Version }) : Localizer.Format("Modding_ModVersion", new object[1] { mod.Version }));
		RefreshModifiableFonts();
	}

	private void RefreshLabel(Style style)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)text).text = Localizer.Get((mod.Location == Mod.E_ModLocation.STEAM) ? "Modding_ModLocation_Steam" : "Modding_ModLocation_Local") + " ";
		TextMeshProUGUI obj = text;
		((TMP_Text)obj).text = ((TMP_Text)obj).text + ((mod.Title != string.Empty) ? (style.OpenTag + mod.Title + style.OpenTag) : Localizer.Get("Modding_ItemNoTitle")) + " ";
		TextMeshProUGUI obj2 = text;
		((TMP_Text)obj2).text = ((TMP_Text)obj2).text + ((mod.Author != string.Empty) ? Localizer.Format("Modding_ItemAuthor", new object[1] { style.OpenTag + mod.Author + style.OpenTag }) : Localizer.Get("Modding_ItemNoAuthor"));
		if (mod.IsIncompatible)
		{
			TextMeshProUGUI obj3 = text;
			((TMP_Text)obj3).text = ((TMP_Text)obj3).text + " " + Localizer.Get("Modding_Incompatible_Label");
		}
		((Graphic)text).color = (mod.IsUsed ? activeDataColor._Color : inactiveDataColor._Color);
	}

	private void OnItemUploaded(SubmitItemUpdateResult_t param)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		((UnityEvent<SubmitItemUpdateResult_t>)WorkshopItemHandler.OnUploadSteamItem).RemoveListener((UnityAction<SubmitItemUpdateResult_t>)OnItemUploaded);
		ModLoader<SteamModLoader>.Instance.OnPublishedItemsLoaded.AddListener(new UnityAction(OnPublishedItemsLoaded));
		ModLoader<SteamModLoader>.Instance.LoadPublishedMods();
	}

	private void OnItemUploadFailed(SubmitItemUpdateResult_t param)
	{
		((UnityEvent<SubmitItemUpdateResult_t>)WorkshopItemHandler.OnFailUploadSteamItem).RemoveListener((UnityAction<SubmitItemUpdateResult_t>)OnItemUploadFailed);
		string textLocKey = "Modding_UnsupportedErrorLabel";
		GenericPopUp.Open("Modding_UploadErrorTitle", textLocKey);
		((Component)uploadingLabel).gameObject.SetActive(false);
		Refresh(mod);
		TPSingleton<ModsView>.Instance.UnlockRaycasts();
		TPSingleton<ModsView>.Instance.IsUploadingMod = false;
	}

	private void OnPublishedItemsLoaded()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		ModLoader<SteamModLoader>.Instance.OnPublishedItemsLoaded.RemoveListener(new UnityAction(OnPublishedItemsLoaded));
		((Component)uploadingLabel).gameObject.SetActive(false);
		Refresh(mod);
		TPSingleton<ModsView>.Instance.UnlockRaycasts();
		TPSingleton<ModsView>.Instance.IsUploadingMod = false;
	}

	private void RefreshSteamUploadButton()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		if (mod.Location == Mod.E_ModLocation.LOCAL)
		{
			((TMP_Text)uploadButtonLabel).text = Localizer.Get(mod.HasAWorkshopId ? "Modding_UpdateInSteam" : "Modding_UploadInSteam");
			((UnityEvent)((Button)uploadButton).onClick).RemoveListener(new UnityAction(OpenChangeNotePopup));
			((UnityEvent)((Button)TPSingleton<ModsView>.Instance.SubmitChangeNoteButton).onClick).RemoveListener(new UnityAction(UploadItemToSteam));
			if (ModLoader<SteamModLoader>.Instance.CanUserUploadMod(mod))
			{
				if (ModManager.IsModder)
				{
					((Component)uploadButton).gameObject.SetActive(true);
					((UnityEvent)((Button)uploadButton).onClick).AddListener(new UnityAction(OpenChangeNotePopup));
					((UnityEvent)((Button)TPSingleton<ModsView>.Instance.SubmitChangeNoteButton).onClick).AddListener(new UnityAction(UploadItemToSteam));
				}
				else
				{
					((Component)uploadButton).gameObject.SetActive(false);
				}
			}
			else if (ModManager.IsModder)
			{
				if (!mod.HasAWorkshopId)
				{
					((Component)uploadButton).gameObject.SetActive(true);
					((UnityEvent)((Button)uploadButton).onClick).AddListener(new UnityAction(OpenChangeNotePopup));
					((UnityEvent)((Button)TPSingleton<ModsView>.Instance.SubmitChangeNoteButton).onClick).AddListener(new UnityAction(UploadItemToSteam));
				}
				else
				{
					((Selectable)uploadButton).interactable = false;
					genericTooltipDisplayer.SetTargetTooltip(TPSingleton<ModsView>.Instance.WarningTooltip);
					((Component)genericTooltipDisplayer).gameObject.SetActive(true);
				}
			}
			else
			{
				((Component)uploadButton).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)uploadButton).gameObject.SetActive(false);
		}
	}

	private void OpenChangeNotePopup()
	{
		TPSingleton<ModsView>.Instance.OpenChangeNotePopup();
	}

	private void UploadItemToSteam()
	{
		if (!mod.ModController.CanBeUploaded(out var uploadErrorReason))
		{
			GenericPopUp.Open("Modding_AbortUploadTitle", "Modding_AbortUpload_" + uploadErrorReason);
			return;
		}
		((UnityEvent<SubmitItemUpdateResult_t>)WorkshopItemHandler.OnUploadSteamItem).AddListener((UnityAction<SubmitItemUpdateResult_t>)OnItemUploaded);
		((UnityEvent<SubmitItemUpdateResult_t>)WorkshopItemHandler.OnFailUploadSteamItem).AddListener((UnityAction<SubmitItemUpdateResult_t>)OnItemUploadFailed);
		TPSingleton<ModsView>.Instance.LockRaycasts();
		TPSingleton<ModsView>.Instance.IsUploadingMod = true;
		if (mod.HasAWorkshopId)
		{
			WorkshopItemHandler.ModifyItem(mod, TPSingleton<ModsView>.Instance.GetChangeNoteValue());
			((TMP_Text)uploadingLabel).text = Localizer.Get("Modding_UpdatingItem");
		}
		else
		{
			WorkshopItemHandler.CreateItem(mod);
			((TMP_Text)uploadingLabel).text = Localizer.Get("Modding_UploadingItem");
		}
		((Component)uploadingLabel).gameObject.SetActive(true);
		((Component)uploadButton).gameObject.SetActive(false);
	}

	private void AddSteamWorkshopIdInTooltip()
	{
		if (ModManager.IsModder && mod.HasAWorkshopId)
		{
			rawTextTooltipDisplayer.Text += "\r\n\r\n";
			rawTextTooltipDisplayer.Text += Localizer.Format("Modding_SteamWorkshopId", new object[1] { mod.WorkshopId });
		}
	}

	public void RefreshModifiableFonts()
	{
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
	}
}
