using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Database.Meta;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Helpers;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tutorial;
using TheLastStand.View.WorldMap.Glyphs.Feedback;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphSelectionPanel : OraculumHub<GlyphSelectionPanel>
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<GlyphDisplay, bool> _003C_003E9__42_0;

		public static Func<GlyphDisplay, bool> _003C_003E9__49_0;

		public static Func<GlyphDisplay, bool> _003C_003E9__51_0;

		public static UnityAction _003C_003E9__54_0;

		public static Func<GlyphDefinition, bool> _003C_003E9__55_0;

		public static Func<GlyphDisplay, bool> _003C_003E9__71_0;

		public static Func<SelectedGlyphDisplay, bool> _003C_003E9__71_1;

		internal bool _003COnSelectedGlyphJoystickSubmit_003Eb__42_0(GlyphDisplay o)
		{
			return ((Component)o).gameObject.activeSelf;
		}

		internal bool _003CAwake_003Eb__49_0(GlyphDisplay o)
		{
			return !o.GlyphDefinition.IsCustom;
		}

		internal bool _003COnHubEnter_003Eb__51_0(GlyphDisplay o)
		{
			return ((Component)o).gameObject.activeSelf;
		}

		internal void _003CStart_003Eb__54_0()
		{
			OraculumHub<GlyphSelectionPanel>.Display(show: false);
		}

		internal bool _003CDeactivateCustomModeIfNeeded_003Eb__55_0(GlyphDefinition glyph)
		{
			return glyph.IsCustom;
		}

		internal bool _003CRefreshJoystickNavigation_003Eb__71_0(GlyphDisplay o)
		{
			return ((Component)o).gameObject.activeSelf;
		}

		internal bool _003CRefreshJoystickNavigation_003Eb__71_1(SelectedGlyphDisplay o)
		{
			if (((Component)o).gameObject.activeSelf)
			{
				return !o.DestroyingSelf;
			}
			return false;
		}
	}

	[SerializeField]
	private float scrollSensitivity = 0.1f;

	[SerializeField]
	private Material glyphAvailableMaterial;

	[SerializeField]
	private Material glyphSelectedMaterial;

	[SerializeField]
	private GlyphDisplay glyphDisplayPrefab;

	[SerializeField]
	private SelectedGlyphDisplay selectedGlyphDisplayPrefab;

	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private GlyphsHeader glyphsHeader;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TutorialPopup tutorialPopup;

	[SerializeField]
	private BetterToggle customModeToggle;

	[SerializeField]
	private RectTransform selectedGlyphDisplaysContainer;

	[SerializeField]
	private ScrollRect selectedGlyphsScrollRect;

	[SerializeField]
	private GameObject selectedGlyphsLeftButton;

	[SerializeField]
	private GameObject selectedGlyphsRightButton;

	[SerializeField]
	private RectTransform glyphDisplaysContainer;

	[SerializeField]
	private RectTransform glyphsViewport;

	[SerializeField]
	private Scrollbar glyphsScrollbar;

	[SerializeField]
	private List<AudioClip> activateClips;

	[SerializeField]
	private List<AudioClip> deactivateClips;

	[SerializeField]
	private List<AudioClip> errorClips;

	[SerializeField]
	private AudioSource audioSourceTemplate;

	[SerializeField]
	[Min(1f)]
	private int audioSourcesCount = 3;

	[SerializeField]
	private LayoutNavigationInitializer glyphsGridNavigationInitializer;

	[SerializeField]
	private float waitBetweenUnlockFeedback = 0.1f;

	private readonly List<GlyphDisplay> glyphDisplays = new List<GlyphDisplay>();

	private readonly List<SelectedGlyphDisplay> selectedGlyphDisplays = new List<SelectedGlyphDisplay>();

	private AudioSource[] audioSources;

	private int nextAudioSourceIndex;

	public bool AnySelectedGlyphDestroying { get; private set; }

	public Material GlyphAvailableMaterial => glyphAvailableMaterial;

	public Material GlyphSelectedMaterial => glyphSelectedMaterial;

	public void OnSelectedGlyphsLeftButtonClick()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		selectedGlyphsScrollRect.OnScroll(new PointerEventData((EventSystem)null)
		{
			scrollDelta = new Vector2(-1f, 0f)
		});
	}

	public void OnSelectedGlyphsRightButtonClick()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		selectedGlyphsScrollRect.OnScroll(new PointerEventData((EventSystem)null)
		{
			scrollDelta = new Vector2(1f, 0f)
		});
	}

	public void OnGlyphsTopButtonClick()
	{
		Scrollbar obj = glyphsScrollbar;
		obj.value -= scrollSensitivity;
	}

	public void OnGlyphsBotButtonClick()
	{
		Scrollbar obj = glyphsScrollbar;
		obj.value += scrollSensitivity;
	}

	public AudioSource GetNextAudioSource()
	{
		return audioSources[nextAudioSourceIndex++ % audioSources.Length];
	}

	public void RefreshAllGlyphs()
	{
		string[] source = (GlyphManager.Debug_GlyphsForceUnlock ? Array.Empty<string>() : TPSingleton<MetaUpgradesManager>.Instance.GetLockedGlyphIds());
		List<GlyphDefinition> list = new List<GlyphDefinition>();
		List<GlyphDefinition> list2 = new List<GlyphDefinition>();
		HashSet<UnlockedGlyphFeedback> hashSet = new HashSet<UnlockedGlyphFeedback>();
		int num = 0;
		bool flag = TPSingleton<WorldMapCityManager>.Instance.IsCurrentSelectedCityLastPlayedCity();
		foreach (KeyValuePair<string, GlyphDefinition> glyphDefinition in GlyphDatabase.GlyphDefinitions)
		{
			if (source.Contains(glyphDefinition.Key))
			{
				list2.Add(glyphDefinition.Value);
				continue;
			}
			if (!glyphDefinition.Value.IsCustom)
			{
				list.Add(glyphDefinition.Value);
				continue;
			}
			bool flag2 = TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Contains(glyphDefinition.Value.Id);
			bool flag3 = flag && TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Contains(glyphDefinition.Value.Id);
			Dictionary<string, int> value;
			bool isCompleted = TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.TryGetValue(glyphDefinition.Value.Id, out value) && value.ContainsKey(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id);
			glyphDisplays[num].Init(glyphDefinition.Value, locked: false, isCompleted, TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled && flag3, flag2);
			glyphDisplays[num].SetSelection(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(glyphDefinition.Value));
			if (flag2)
			{
				hashSet.Add(glyphDisplays[num].UnlockFeedback);
			}
			num++;
		}
		foreach (GlyphDefinition item in list)
		{
			bool flag4 = TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Contains(item.Id);
			bool triggerCompletionAnimation = flag && TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Contains(item.Id);
			Dictionary<string, int> value2;
			bool isCompleted2 = TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.TryGetValue(item.Id, out value2) && value2.ContainsKey(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id);
			glyphDisplays[num].Init(item, locked: false, isCompleted2, triggerCompletionAnimation, flag4);
			glyphDisplays[num].SetSelection(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(item));
			if (flag4)
			{
				hashSet.Add(glyphDisplays[num].UnlockFeedback);
			}
			num++;
		}
		foreach (GlyphDefinition item2 in list2)
		{
			glyphDisplays[num].Init(item2, locked: true, isCompleted: false, triggerCompletionAnimation: false, newlyUnlocked: false);
			glyphDisplays[num].SetSelection(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(item2));
			num++;
		}
		TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Clear();
		TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Clear();
		((MonoBehaviour)this).StartCoroutine(TriggerUnlockAnimations(hashSet));
	}

	public void OnSelectedGlyphJoystickSubmit(SelectedGlyphDisplay selectedGlyphDisplay)
	{
		if (selectedGlyphDisplays.Count == 1)
		{
			GlyphDisplay glyphDisplay = glyphDisplays.FirstOrDefault((GlyphDisplay o) => ((Component)o).gameObject.activeSelf);
			EventSystem.current.SetSelectedGameObject(((Object)(object)glyphDisplay != (Object)null) ? ((Component)glyphDisplay).gameObject : ((Component)customModeToggle).gameObject);
			RefreshJoystickNavigation();
			return;
		}
		for (int i = 0; i < selectedGlyphDisplays.Count; i++)
		{
			if ((Object)(object)selectedGlyphDisplays[i] == (Object)(object)selectedGlyphDisplay)
			{
				SelectedGlyphDisplay selectedGlyphDisplay2 = (((InputManager.JoystickConfig.HUDNavigation.SelectNextOmenOnUnselect && i < selectedGlyphDisplays.Count - 1) || i == 0) ? selectedGlyphDisplays[i + 1] : selectedGlyphDisplays[i - 1]);
				EventSystem.current.SetSelectedGameObject(((Component)selectedGlyphDisplay2).gameObject);
				break;
			}
		}
	}

	public void OnSelectedGlyphJoystickSelect(SelectedGlyphDisplay selectedGlyphDisplay)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Rect worldRect = RectTransformExtensions.GetWorldRect((RectTransform)((Component)selectedGlyphDisplay).transform);
		Rect worldRect2 = selectedGlyphsScrollRect.viewport.GetWorldRect();
		bool flag = false;
		while (((Rect)(ref worldRect)).center.x > ((Rect)(ref worldRect2)).max.x)
		{
			OnSelectedGlyphsRightButtonClick();
			worldRect = RectTransformExtensions.GetWorldRect((RectTransform)((Component)selectedGlyphDisplay).transform);
			flag = true;
		}
		while (((Rect)(ref worldRect)).center.x < ((Rect)(ref worldRect2)).min.x)
		{
			OnSelectedGlyphsLeftButtonClick();
			worldRect = RectTransformExtensions.GetWorldRect((RectTransform)((Component)selectedGlyphDisplay).transform);
			flag = true;
		}
		if (flag)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
		}
	}

	public void PlayErrorClip()
	{
		SoundManager.PlayAudioClip(GetNextAudioSource(), TPHelpers.RandomElement<AudioClip>(errorClips));
	}

	public bool SelectedGlyphDisplayExists(GlyphDefinition glyphDefinition)
	{
		return selectedGlyphDisplays.Any((SelectedGlyphDisplay x) => x.GlyphDefinition == glyphDefinition);
	}

	public void SelectGlyph(GlyphDisplay glyphDisplay, bool updateCity = true, bool playFeedback = true)
	{
		if (updateCity)
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityController.AddGlyph(glyphDisplay.GlyphDefinition);
			glyphsHeader.RefreshCityPoints();
		}
		glyphDisplay.SetSelection(selected: true);
		InstantiateSelectedGlyph(glyphDisplay.GlyphDefinition);
		RefreshSelectedGlyphsButtons();
		if (playFeedback)
		{
			glyphDisplay.TriggerSelectionFeedback();
			PlayGlyphClip(selected: true);
		}
	}

	public void UnselectGlyph(GlyphDisplay glyphDisplay, bool updateCity = true, bool playFeedback = true)
	{
		SelectedGlyphDisplay selectedGlyphDisplay = selectedGlyphDisplays.Find((SelectedGlyphDisplay o) => o.GlyphDefinition == glyphDisplay.GlyphDefinition);
		UnselectGlyph(selectedGlyphDisplay, glyphDisplay, updateCity, playFeedback);
	}

	public void UnselectGlyph(SelectedGlyphDisplay selectedGlyphDisplay, GlyphDisplay glyphDisplay = null, bool updateCity = true, bool playFeedback = true)
	{
		if (updateCity)
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityController.RemoveGlyph(selectedGlyphDisplay.GlyphDefinition);
			glyphsHeader.RefreshCityPoints();
		}
		if (glyphDisplay == null)
		{
			glyphDisplay = glyphDisplays.Find((GlyphDisplay o) => o.GlyphDefinition == selectedGlyphDisplay.GlyphDefinition);
		}
		glyphDisplay.SetSelection(selected: false);
		DestroySelectedGlyph(selectedGlyphDisplay, !playFeedback);
		if (playFeedback)
		{
			glyphDisplay.TriggerSelectionFeedback();
			PlayGlyphClip(selected: false);
		}
	}

	protected override void Awake()
	{
		((TPSingleton<GlyphSelectionPanel>)this).Awake();
		InstantiateGlyphs();
		if ((Object)(object)tutorialPopup != (Object)null)
		{
			tutorialPopup.SetSelectableAfterClose((Selectable)(object)glyphDisplays.FirstOrDefault((GlyphDisplay o) => !o.GlyphDefinition.IsCustom)?.JoystickSelectable);
		}
		((UnityEvent<bool>)(object)((Toggle)customModeToggle).onValueChanged).AddListener((UnityAction<bool>)ToggleCustomMode);
	}

	protected override void OnFadeToBlackComplete()
	{
		base.OnFadeToBlackComplete();
		if (base.Displayed)
		{
			RefreshContent();
			glyphsGridNavigationInitializer.InitNavigation(reset: true);
			if (TPSingleton<WorldMapCityManager>.Instance.IsCurrentSelectedCityLastPlayedCity())
			{
				TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Clear();
				TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Clear();
			}
			RefreshJoystickNavigation();
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnGlyphsPanelOpen);
		}
		else
		{
			DeactivateCustomModeIfNeeded();
			TPSingleton<GameConfigurationsView>.Instance.GlyphSelectionPreview.Refresh();
			TPSingleton<GameConfigurationsView>.Instance.RefreshBoxSize();
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<GameConfigurationsView>.Instance.JoystickSelectPanel();
			}
		}
	}

	protected override void OnHubEnter(Action onDisplayed = null)
	{
		base.OnHubEnter(onDisplayed);
		if (InputManager.IsLastControllerJoystick)
		{
			GlyphDisplay glyphDisplay = glyphDisplays.FirstOrDefault((GlyphDisplay o) => ((Component)o).gameObject.activeSelf);
			if ((Object)(object)glyphDisplay != (Object)null)
			{
				EventSystem.current.SetSelectedGameObject(((Component)glyphDisplay).gameObject);
			}
		}
	}

	protected override void OnFadeToBlackStarts()
	{
		base.OnFadeToBlackStarts();
		WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.GLYPHSELECTION);
		if (!base.Displayed)
		{
			TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.WorldMapMusic);
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeIn();
			}
		}
		else
		{
			for (int j = 0; j < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; j++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[j].FadeOut();
			}
		}
	}

	protected override void OnHubExit()
	{
		base.OnHubExit();
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.Hide();
		WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.FOCUSED);
	}

	protected override void Start()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		ButtonClickedEvent onClick = ((Button)leaveButton).onClick;
		object obj = _003C_003Ec._003C_003E9__54_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				OraculumHub<GlyphSelectionPanel>.Display(show: false);
			};
			_003C_003Ec._003C_003E9__54_0 = val;
			obj = (object)val;
		}
		((UnityEvent)onClick).AddListener((UnityAction)obj);
		CanvasHelper.ScaleCanvasTowards720P(canvasScaler);
		InitAudio();
		base.Start();
	}

	private void DeactivateCustomModeIfNeeded()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints() == 0 && !TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Any((GlyphDefinition glyph) => glyph.IsCustom))
		{
			TPSingleton<GlyphManager>.Instance.ToggleCustomMode(toggle: false);
			glyphsGridNavigationInitializer.InitNavigation(reset: true);
			RefreshJoystickNavigation();
		}
	}

	private void DestroySelectedGlyph(SelectedGlyphDisplay selectedGlyphDisplay, bool instantly = false)
	{
		AnySelectedGlyphDestroying = true;
		selectedGlyphDisplays.Remove(selectedGlyphDisplay);
		RefreshSelectedGlyphsNavigation();
		((MonoBehaviour)this).StartCoroutine(selectedGlyphDisplay.DestroySelf(RefreshSelectedGlyphsButtonsAfterAFrame, instantly));
	}

	private void InitAudio()
	{
		activateClips.Shuffle();
		deactivateClips.Shuffle();
		audioSources = (AudioSource[])(object)new AudioSource[audioSourcesCount];
		audioSources[0] = audioSourceTemplate;
		for (int i = 1; i < audioSources.Length; i++)
		{
			AudioSource val = Object.Instantiate<AudioSource>(audioSourceTemplate, ((Component)audioSourceTemplate).transform.parent);
			audioSources[i] = val;
		}
	}

	private void InstantiateGlyph(GlyphDefinition glyphDefinition, bool isLocked)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		bool num = TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(glyphDefinition);
		GlyphDisplay glyphDisplay = Object.Instantiate<GlyphDisplay>(glyphDisplayPrefab, (Transform)(object)glyphDisplaysContainer);
		glyphDisplay.Init(glyphDefinition, isLocked, isCompleted: false, triggerCompletionAnimation: false, newlyUnlocked: false);
		((Component)glyphDisplay).GetComponent<JoystickSelectable>().AddListenerOnSelect((UnityAction)delegate
		{
			GlyphSelectionPanel glyphSelectionPanel = this;
			Transform transform = ((Component)glyphDisplay).transform;
			glyphSelectionPanel.OnJoystickSelect((RectTransform)(object)((transform is RectTransform) ? transform : null));
		});
		glyphDisplays.Add(glyphDisplay);
		if (num)
		{
			SelectGlyph(glyphDisplay, updateCity: false, playFeedback: false);
		}
	}

	private void InstantiateGlyphs()
	{
		string[] lockedGlyphIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedGlyphIds();
		List<GlyphDefinition> list = new List<GlyphDefinition>();
		List<GlyphDefinition> list2 = new List<GlyphDefinition>();
		foreach (KeyValuePair<string, GlyphDefinition> glyphDefinition in GlyphDatabase.GlyphDefinitions)
		{
			if (lockedGlyphIds.Contains(glyphDefinition.Key))
			{
				list2.Add(glyphDefinition.Value);
			}
			else if (!glyphDefinition.Value.IsCustom)
			{
				list.Add(glyphDefinition.Value);
			}
			else
			{
				InstantiateGlyph(glyphDefinition.Value, isLocked: false);
			}
		}
		foreach (GlyphDefinition item in list)
		{
			InstantiateGlyph(item, isLocked: false);
		}
		foreach (GlyphDefinition item2 in list2)
		{
			InstantiateGlyph(item2, isLocked: true);
		}
	}

	private void InstantiateSelectedGlyph(GlyphDefinition glyphDefinition)
	{
		SelectedGlyphDisplay selectedGlyphDisplay = Object.Instantiate<SelectedGlyphDisplay>(selectedGlyphDisplayPrefab, (Transform)(object)selectedGlyphDisplaysContainer);
		selectedGlyphDisplay.Init(glyphDefinition);
		selectedGlyphDisplays.Add(selectedGlyphDisplay);
		RefreshSelectedGlyphsNavigation();
	}

	private void OnJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, glyphsViewport, glyphsScrollbar, 0.02f, 0f, 0.1f);
	}

	private void PlayGlyphClip(bool selected)
	{
		AudioClip val = GetNextClip(selected ? activateClips : deactivateClips);
		if ((Object)(object)val == (Object)null)
		{
			CLoggerManager.Log((object)"Could not find a valid glyph audio clip!", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			SoundManager.PlayAudioClip(GetNextAudioSource(), val);
		}
		static AudioClip GetNextClip(List<AudioClip> list)
		{
			AudioClip val2 = list[0];
			int index = Random.Range(2, list.Count);
			list.RemoveAt(0);
			list.Insert(index, val2);
			return val2;
		}
	}

	private void RefreshContent()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		glyphsHeader.RefreshCityName();
		for (int num = selectedGlyphDisplays.Count - 1; num >= 0; num--)
		{
			UnselectGlyph(selectedGlyphDisplays[num], null, updateCity: false, playFeedback: false);
		}
		List<GlyphDefinition> selectedGlyphs = selectedCity.GlyphsConfig.SelectedGlyphs;
		int i = 0;
		while (i < selectedGlyphs.Count)
		{
			GlyphDisplay glyphDisplay = glyphDisplays.Find((GlyphDisplay o) => o.GlyphDefinition == selectedGlyphs[i]);
			SelectGlyph(glyphDisplay, updateCity: false, playFeedback: false);
			int num2 = i + 1;
			i = num2;
		}
		((Toggle)customModeToggle).isOn = selectedCity.GlyphsConfig.CustomModeEnabled;
		glyphsHeader.InitCityPointImages();
		glyphsHeader.RefreshCityPoints();
		RefreshCustomMode();
		RefreshSelectedGlyphsButtons();
		RefreshAllGlyphs();
	}

	private void RefreshCustomMode()
	{
		bool customModeEnabled = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled;
		foreach (GlyphDisplay glyphDisplay in glyphDisplays)
		{
			if (!glyphDisplay.GlyphDefinition.IsCustom)
			{
				break;
			}
			((Component)glyphDisplay).gameObject.SetActive(customModeEnabled);
		}
	}

	private void RefreshSelectedGlyphsButtons()
	{
		((MonoBehaviour)this).StartCoroutine(RefreshSelectedGlyphsButtonsAfterAFrame());
	}

	private IEnumerator RefreshSelectedGlyphsButtonsAfterAFrame()
	{
		yield return null;
		AnySelectedGlyphDestroying = false;
		Rect rect = selectedGlyphsScrollRect.viewport.rect;
		float width = ((Rect)(ref rect)).width;
		rect = selectedGlyphsScrollRect.content.rect;
		bool flag = width < ((Rect)(ref rect)).width;
		selectedGlyphsLeftButton.SetActive(flag);
		selectedGlyphsRightButton.SetActive(flag);
		if (flag)
		{
			OnSelectedGlyphsRightButtonClick();
		}
		if (!base.OpeningOrClosing)
		{
			RefreshJoystickNavigation();
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
		}
	}

	private void ToggleCustomMode(bool toggle)
	{
		if (!toggle)
		{
			UnselectCustomGlyphs();
		}
		TPSingleton<GlyphManager>.Instance.ToggleCustomMode(toggle);
		RefreshContent();
		glyphsGridNavigationInitializer.InitNavigation(reset: true);
		RefreshJoystickNavigation();
	}

	private IEnumerator TriggerUnlockAnimations(HashSet<UnlockedGlyphFeedback> feedback)
	{
		foreach (UnlockedGlyphFeedback unlockedGlyphFeedback in feedback)
		{
			yield return SharedYields.WaitForSeconds(waitBetweenUnlockFeedback);
			unlockedGlyphFeedback.TriggerUnlockAnimation();
		}
	}

	private void UnselectCustomGlyphs()
	{
		for (int num = selectedGlyphDisplays.Count - 1; num >= 0; num--)
		{
			if (selectedGlyphDisplays[num].GlyphDefinition.IsCustom)
			{
				UnselectGlyph(selectedGlyphDisplays[num], null, updateCity: true, playFeedback: false);
			}
		}
	}

	private void RefreshSelectedGlyphsNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < selectedGlyphDisplays.Count; i++)
		{
			SelectedGlyphDisplay selectedGlyphDisplay = selectedGlyphDisplays[i];
			if (!selectedGlyphDisplay.DestroyingSelf && !((Object)(object)selectedGlyphDisplay.JoystickSelectable == (Object)null))
			{
				list.Add((Selectable)(object)selectedGlyphDisplay.JoystickSelectable);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Selectable selectable = list[j];
			selectable.SetMode((Mode)4);
			selectable.SetSelectOnLeft(null);
			selectable.SetSelectOnRight(null);
			if (j > 0)
			{
				selectable.SetSelectOnLeft(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				selectable.SetSelectOnRight(list[j + 1]);
			}
		}
	}

	private void RefreshJoystickNavigation()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)(object)customModeToggle).SetMode((Mode)4);
		((Selectable)(object)customModeToggle).ClearNavigation();
		Selectable val = (Selectable)(object)glyphDisplays.FirstOrDefault((GlyphDisplay o) => ((Component)o).gameObject.activeSelf)?.JoystickSelectable;
		Selectable val2 = (Selectable)(object)selectedGlyphDisplays.FirstOrDefault((SelectedGlyphDisplay o) => ((Component)o).gameObject.activeSelf && !o.DestroyingSelf)?.JoystickSelectable;
		for (int i = 0; i < selectedGlyphDisplays.Count; i++)
		{
			((Selectable)(object)selectedGlyphDisplays[i].JoystickSelectable).SetSelectOnDown(val);
			((Selectable)(object)selectedGlyphDisplays[i].JoystickSelectable).SetSelectOnUp((Selectable)(object)customModeToggle);
		}
		for (int j = 0; j < glyphDisplays.Count; j++)
		{
			Selectable joystickSelectable = (Selectable)(object)glyphDisplays[j].JoystickSelectable;
			Navigation navigation = joystickSelectable.navigation;
			if ((Object)(object)((Navigation)(ref navigation)).selectOnUp != (Object)null)
			{
				navigation = joystickSelectable.navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnUp != (Object)(object)customModeToggle)
				{
					continue;
				}
			}
			joystickSelectable.SetSelectOnUp((Selectable)(object)(((Object)(object)val2 != (Object)null) ? ((BetterToggle)(object)val2) : customModeToggle));
		}
		((Selectable)(object)customModeToggle).SetSelectOnDown(((Object)(object)val2 != (Object)null) ? val2 : val);
		((Selectable)(object)customModeToggle).SetSelectOnRight((Selectable)(object)closeButton);
	}
}
