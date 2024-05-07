using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.UI;
using TheLastStand.Helpers;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.ItemRestriction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapUIManager : Manager<WorldMapUIManager>
{
	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private Button metaShopsButton;

	[SerializeField]
	private OpenWeaponRestrictionsView openWeaponRestrictionsView;

	private AudioSource audioSource;

	public static AudioSource AudioSource
	{
		get
		{
			if ((Object)(object)TPSingleton<WorldMapUIManager>.Instance.audioSource == (Object)null)
			{
				TPSingleton<WorldMapUIManager>.Instance.audioSource = ((Component)TPSingleton<WorldMapUIManager>.Instance).GetComponent<AudioSource>();
			}
			return TPSingleton<WorldMapUIManager>.Instance.audioSource;
		}
	}

	public static bool CanAccessMetaShops
	{
		get
		{
			if (!MetaShopsManager.OraculumForceAccess)
			{
				return TPSingleton<WorldMapCityManager>.Instance.Cities.Any((WorldMapCity city) => !city.CityDefinition.IsTutorialMap && city.RefreshIsVisible() && city.RefreshIsUnlocked());
			}
			return true;
		}
	}

	public static bool CanOpenWeaponRestrictionsPanel => TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsAvailable;

	public static int GetApocalypseSelectedLevel()
	{
		if (TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines.Count == 0)
		{
			return 0;
		}
		for (int num = TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines[num].State == BetterToggleGauge.E_BetterToggleGaugeState.Selected)
			{
				return TPSingleton<GameConfigurationsView>.Instance.ApocalypseLines[num].ApocalypseDefinition.Id;
			}
		}
		return 0;
	}

	public static void OnReturnedFromOraculum()
	{
		TPSingleton<WorldMapUIManager>.Instance.openWeaponRestrictionsView.DisplayOrRefresh();
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWorldMapOpen);
	}

	public void PlayAudioClip(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(AudioSource, audioClip);
	}

	public void PlayAudioClipWithoutInterrupting(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(AudioSource, audioClip, 0f, doNotInterrupt: true);
	}

	private void OnMetaShopsButtonClicked()
	{
		if (MetaShopsManager.CanOpenShops())
		{
			TPSingleton<MetaShopsManager>.Instance.OpenShops();
		}
	}

	private void RefreshMetaShopsButtonAvailability()
	{
		((Component)metaShopsButton).gameObject.SetActive(CanAccessMetaShops);
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)metaShopsButton.onClick).AddListener(new UnityAction(OnMetaShopsButtonClicked));
		RefreshMetaShopsButtonAvailability();
		CanvasHelper.ScaleCanvas(canvasScaler, allowDecimals: false);
	}

	protected override void OnDestroy()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		((CLogger<WorldMapUIManager>)this).OnDestroy();
		((UnityEvent)metaShopsButton.onClick).RemoveListener(new UnityAction(OnMetaShopsButtonClicked));
	}
}
