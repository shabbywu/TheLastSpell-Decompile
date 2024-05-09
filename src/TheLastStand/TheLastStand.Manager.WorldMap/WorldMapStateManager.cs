using TPLib;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.MetaShops;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.Glyphs;
using TheLastStand.View.WorldMap.ItemRestriction;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapStateManager : TPSingleton<WorldMapStateManager>
{
	public enum WorldMapState
	{
		DEFAULT,
		EXPLORATION,
		FOCUSED,
		GLYPHSELECTION
	}

	public WorldMapState CurrentState { get; private set; }

	public static void SetState(WorldMapState state)
	{
		TPSingleton<WorldMapStateManager>.Instance.CurrentState = state;
		WorldMapCityManager.OnStateChange();
		WorldMapCameraView.OnStateChange();
		TPSingleton<GameConfigurationsView>.Instance.OnStateChange();
		TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.OnStateChange();
		if (state == WorldMapState.FOCUSED)
		{
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWorldMapCitySelected);
		}
	}

	private void Update()
	{
		if (ApplicationManager.CurrentStateName != "WorldMap" || !TPSingleton<CanvasFadeManager>.Instance.FadeIsOver || TPSingleton<OraculumView>.Instance.Displayed || TPSingleton<OraculumView>.Instance.OpeningOrClosing || TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing || ACameraView.IsZooming || GenericConsent.IsWaitingForInput())
		{
			return;
		}
		if (InputManager.GetButtonDown(23))
		{
			if (TryClosingWeaponRestrictionsPanel())
			{
				return;
			}
			switch (CurrentState)
			{
			case WorldMapState.FOCUSED:
				TPSingleton<GameConfigurationsView>.Instance.OnCloseButtonClicked();
				break;
			case WorldMapState.GLYPHSELECTION:
			{
				OraculumHub<GlyphSelectionPanel>.Display(show: false);
				for (int j = 0; j < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; j++)
				{
					TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[j].FadeIn();
				}
				break;
			}
			default:
			{
				for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
				{
					TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeOut(TPSingleton<CanvasFadeManager>.Instance.FadeDuration);
				}
				TPSingleton<GameConfigurationsView>.Instance.OnBackButtonClicked();
				break;
			}
			}
		}
		else if (InputManager.GetButtonDown(137) && !TryClosingWeaponRestrictionsPanel() && CurrentState == WorldMapState.FOCUSED)
		{
			TPSingleton<GameConfigurationsView>.Instance.OnCloseButtonClicked();
		}
	}

	private bool TryClosingWeaponRestrictionsPanel()
	{
		if (TPSingleton<WeaponRestrictionsPanel>.Instance.Displayed)
		{
			if (TPSingleton<WeaponRestrictionsPanel>.Instance.CanClosePanel)
			{
				TPSingleton<WeaponRestrictionsPanel>.Instance.Close();
			}
			return true;
		}
		return false;
	}
}
