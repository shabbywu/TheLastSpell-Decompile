using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.WorldMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Apocalypse;

public class ApocalypseView : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public static class Constants
	{
		public const string ApocalypseDifficultyNormal = "WorldMap_ApocalypseDifficulty_Normal";

		public const string ApocalypseDifficultyApocalypse = "WorldMap_ApocalypseDifficulty_Apocalypse";

		public const string ApocalypseDescriptionUnavailable = "WorldMap_ApocalypseDescription_Unavailable";

		public const string ApocalypseDescriptionPrefix = "WorldMap_ApocalypseDescription_";

		public const string ApocalypseDamnedSoulsModifierFormat = "WorldMap_ApocalypseDamnedSoulsModifier";

		public const string ApocalypseLevelImagePrefixPath = "View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_";

		public const string FlameAnimationIdle = "WorldMapFlamesIdle";

		public const string FlameAnimationUnsuccess = "WorldMapFlamesUnsuccess";

		public const string FlameAnimationDisabled = "WorldMapFlamesDisabled";
	}

	public bool AlreadySuccessful;

	public bool Available;

	public ApocalypseDefinition ApocalypseDefinition;

	[SerializeField]
	private Image apocalypseIndexImage;

	[SerializeField]
	private TextMeshProUGUI apocalypseTitle;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Animator flameAnimator;

	[SerializeField]
	private BetterToggleGauge toggleGauge;

	[SerializeField]
	private GameObject damnedSoulsModifierPanel;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsModifierText;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	public E_BetterToggleGaugeState State
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)toggleGauge != (Object)null))
			{
				return (E_BetterToggleGaugeState)0;
			}
			return toggleGauge.State;
		}
	}

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void ContinueAnimations()
	{
		flameAnimator.speed = 1f;
		toggleGauge.Animator.speed = 1f;
	}

	public void Init(BetterToggleGaugeGroup group)
	{
		Available = TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable >= ApocalypseDefinition.Id;
		toggleGauge.Init(ApocalypseDefinition.Id, group);
		if (ApocalypseDefinition.Id != 0)
		{
			apocalypseIndexImage.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_" + ApocalypseDefinition.Id.ToString("00"), false);
			((TMP_Text)apocalypseTitle).text = Localizer.Get("WorldMap_ApocalypseDifficulty_Apocalypse");
		}
		else
		{
			((Component)apocalypseIndexImage).gameObject.SetActive(false);
			((TMP_Text)apocalypseTitle).text = Localizer.Get("WorldMap_ApocalypseDifficulty_Normal");
		}
		damnedSoulsModifierPanel.SetActive(false);
		InitApocalypse();
	}

	public void InitApocalypse()
	{
		ChangeFlame();
		if (!Available)
		{
			toggleGauge.SetState((E_BetterToggleGaugeState)0);
			((TMP_Text)description).text = Localizer.Get("WorldMap_ApocalypseDescription_Unavailable");
			((Component)apocalypseIndexImage).gameObject.SetActive(false);
			((TMP_Text)apocalypseTitle).text = string.Empty;
			return;
		}
		if (ApocalypseDefinition.Id != 0)
		{
			toggleGauge.SetState((E_BetterToggleGaugeState)1);
			((Component)apocalypseIndexImage).gameObject.SetActive(true);
			((TMP_Text)apocalypseTitle).text = Localizer.Get("WorldMap_ApocalypseDifficulty_Apocalypse");
		}
		((TMP_Text)description).text = Localizer.Get("WorldMap_ApocalypseDescription_" + ApocalypseDefinition.Id.ToString("00"));
	}

	public void OnSelect(BaseEventData eventData)
	{
		GameConfigurationsView instance = TPSingleton<GameConfigurationsView>.Instance;
		Transform transform = ((Component)this).transform;
		instance.AdjustScrollView((RectTransform)(object)((transform is RectTransform) ? transform : null));
	}

	public void PauseAnimations()
	{
		flameAnimator.speed = 0f;
		toggleGauge.Animator.speed = 0f;
	}

	public void Refresh()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		Available = TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable >= ApocalypseDefinition.Id;
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxApocalypsePassed >= ApocalypseDefinition.Id)
		{
			((Graphic)apocalypseTitle).color = new Color(((Graphic)apocalypseTitle).color.r, ((Graphic)apocalypseTitle).color.g, ((Graphic)apocalypseTitle).color.b, 1f);
			((Graphic)apocalypseIndexImage).color = new Color(((Graphic)apocalypseIndexImage).color.r, ((Graphic)apocalypseIndexImage).color.g, ((Graphic)apocalypseIndexImage).color.b, 1f);
		}
		else
		{
			((Graphic)apocalypseTitle).color = new Color(((Graphic)apocalypseTitle).color.r, ((Graphic)apocalypseTitle).color.g, ((Graphic)apocalypseTitle).color.b, 0.25f);
			((Graphic)apocalypseIndexImage).color = new Color(((Graphic)apocalypseIndexImage).color.r, ((Graphic)apocalypseIndexImage).color.g, ((Graphic)apocalypseIndexImage).color.b, 0.25f);
		}
		AlreadySuccessful = TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && ApocalypseDefinition.Id <= TPSingleton<WorldMapCityManager>.Instance.SelectedCity.MaxApocalypsePassed;
		InitApocalypse();
	}

	private void ChangeFlame()
	{
		if (!Available)
		{
			flameAnimator.Play("WorldMapFlamesDisabled", 0, Random.value);
		}
		else
		{
			flameAnimator.Play(AlreadySuccessful ? "WorldMapFlamesIdle" : "WorldMapFlamesUnsuccess", 0, Random.value);
		}
	}

	private void Start()
	{
		((UnityEvent<E_BetterToggleGaugeState>)(object)toggleGauge.OnStateHasChanged).AddListener((UnityAction<E_BetterToggleGaugeState>)OnToggleClicked);
	}

	private void OnToggleClicked(E_BetterToggleGaugeState state)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		TPSingleton<GameConfigurationsView>.Instance.OnApocalypseSelectionHasChanged(GameConfigurationsView.IsThereAnApocalypseSelected());
		if ((int)state != 3)
		{
			if (ApocalypseDefinition.Id > 0 && WorldMapUIManager.GetApocalypseSelectedLevel() == ApocalypseDefinition.Id)
			{
				damnedSoulsModifierPanel.SetActive(true);
				((TMP_Text)damnedSoulsModifierText).text = string.Format(Localizer.Get("WorldMap_ApocalypseDamnedSoulsModifier"), ApocalypseDefinition.Id * ApocalypseDatabase.ConfigurationDefinition.DamnedSoulsPercentagePerLevel);
			}
			else
			{
				damnedSoulsModifierPanel.SetActive(false);
			}
		}
	}
}
