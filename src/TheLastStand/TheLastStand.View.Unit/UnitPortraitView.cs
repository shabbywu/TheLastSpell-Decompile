using TPLib;
using TheLastStand.Controller;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class UnitPortraitView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public static class Constants
	{
		public const string PortraitPath = "View/Sprites/UI/Units/Portaits/Playable Unit/Foreground/";

		public const string PortraitBackgroundPath = "View/Sprites/UI/Units/Portaits/Playable Unit/Background/";
	}

	[SerializeField]
	private UnitStatDisplay unitHealthStatDefinition;

	[SerializeField]
	private UnitStatDisplay unitMovePointStatDefinition;

	[SerializeField]
	private UnitStatDisplay unitActionPointStatDefinition;

	[SerializeField]
	protected BetterToggle unitportraitToggle;

	[SerializeField]
	protected Image unitPortraitImage;

	[SerializeField]
	protected Image unitPortraitBoxHovered;

	[SerializeField]
	protected Image unitPortraitBGImage;

	[SerializeField]
	protected SkillTargetingMark skillTargetingMark;

	[SerializeField]
	private Material material;

	private static readonly int SwapTex = Shader.PropertyToID("_SwapTex");

	public PlayableUnit PlayableUnit;

	private bool isInited;

	private Texture2D currentTexture;

	public BetterToggle UnitPortraitToggle => unitportraitToggle;

	public SkillTargetingMark SkillTargetingMark => skillTargetingMark;

	public virtual void DisplayUnitPortraitBoxHovered(bool value)
	{
		if ((Object)(object)unitPortraitBoxHovered != (Object)null)
		{
			((Behaviour)unitPortraitBoxHovered).enabled = value;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		DisplayUnitPortraitBoxHovered(value: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		DisplayUnitPortraitBoxHovered(value: false);
	}

	public void OnUnitPortraitClick()
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		if (TileObjectSelectionManager.IsProcessingASelection || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits))
		{
			return;
		}
		if ((TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill) && PlayableUnitManager.SelectedSkill != null)
		{
			ValidTargets validTargets = PlayableUnitManager.SelectedSkill.SkillDefinition.ValidTargets;
			if (validTargets != null && validTargets.PlayableUnits)
			{
				return;
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill && TileObjectSelectionManager.HasUnitSelected && TileObjectSelectionManager.SelectedUnit is PlayableUnit unit)
		{
			GameView.TopScreenPanel.UnitPortraitsPanel.ToggleSelectedUnit(unit);
			return;
		}
		BetterToggle betterToggle = unitportraitToggle;
		if ((betterToggle == null || ((Toggle)betterToggle).isOn) && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingExecutingAction)
		{
			if (TileObjectSelectionManager.SelectedUnit != PlayableUnit)
			{
				if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
				{
					PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
				}
				if (PlayableUnitManager.SelectedSkill != null)
				{
					PlayableUnitManager.SelectedSkill = null;
				}
				TileObjectSelectionManager.SetSelectedPlayableUnit(PlayableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(((Component)PlayableUnit.UnitView).transform.position));
				switch (TPSingleton<GameManager>.Instance.Game.State)
				{
				case Game.E_State.CharacterSheet:
					TPSingleton<CharacterSheetPanel>.Instance.Refresh();
					break;
				case Game.E_State.Construction:
					GameController.SetState(Game.E_State.Management);
					break;
				}
				if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles)
				{
					PlayableUnit.PlayableUnitController.ComputeReachableTiles();
				}
			}
			else
			{
				ACameraView.MoveTo(((Component)PlayableUnit.UnitView).transform);
			}
		}
		else
		{
			BetterToggle betterToggle2 = unitportraitToggle;
			if ((betterToggle2 == null || !((Toggle)betterToggle2).group.AnyTogglesOn()) && TileObjectSelectionManager.HasUnitSelected && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingAction && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingExecutingAction)
			{
				TileObjectSelectionManager.DeselectUnit();
			}
		}
	}

	public void OnUnitPortraitHoverEnter()
	{
		GameView.TopScreenPanel.UnitPortraitsPanel.SetPortraitIsHovered(this);
	}

	public void OnUnitPortraitHoverExit()
	{
		GameView.TopScreenPanel.UnitPortraitsPanel.SetPortraitIsHovered();
	}

	public void RefreshPortrait()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		((Behaviour)unitPortraitImage).enabled = PlayableUnit != null;
		((Behaviour)unitPortraitBGImage).enabled = PlayableUnit != null;
		if (PlayableUnit != null)
		{
			unitPortraitBGImage.sprite = PlayableUnit.PortraitBackgroundSprite;
			((Graphic)unitPortraitBGImage).color = PlayableUnit.PortraitColor._Color;
			unitPortraitImage.sprite = PlayableUnit.PortraitSprite;
			Texture texture = PlayableUnit.PlayableUnitView.ColorSwapPortraitMaterial.GetTexture(SwapTex);
			Texture obj = ((texture is Texture2D) ? texture : null);
			if ((Object)(object)currentTexture == (Object)null)
			{
				currentTexture = new Texture2D(100, 1, (TextureFormat)4, false, false)
				{
					filterMode = (FilterMode)0,
					wrapMode = (TextureWrapMode)1
				};
			}
			Graphics.CopyTexture(obj, (Texture)(object)currentTexture);
			currentTexture.Apply();
			material.SetTexture(SwapTex, (Texture)(object)currentTexture);
		}
	}

	public virtual void RefreshStats()
	{
		if (!isInited && PlayableUnit != null)
		{
			Init();
		}
		if ((Object)(object)unitHealthStatDefinition != (Object)null)
		{
			unitHealthStatDefinition.Refresh();
		}
		if ((Object)(object)unitActionPointStatDefinition != (Object)null)
		{
			unitActionPointStatDefinition.Refresh();
		}
		if ((Object)(object)unitMovePointStatDefinition != (Object)null)
		{
			unitMovePointStatDefinition.Refresh();
		}
	}

	protected virtual void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		material = new Material(material);
		((Graphic)unitPortraitImage).material = material;
		if ((Object)(object)currentTexture == (Object)null)
		{
			currentTexture = new Texture2D(100, 1, (TextureFormat)4, false, false)
			{
				filterMode = (FilterMode)0,
				wrapMode = (TextureWrapMode)1
			};
		}
		if (!isInited && PlayableUnit != null)
		{
			Init();
		}
	}

	private void Init()
	{
		if ((Object)(object)unitHealthStatDefinition != (Object)null)
		{
			unitHealthStatDefinition.TargetUnit = PlayableUnit;
			unitHealthStatDefinition.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
			unitHealthStatDefinition.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		}
		if ((Object)(object)unitActionPointStatDefinition != (Object)null)
		{
			unitActionPointStatDefinition.TargetUnit = PlayableUnit;
			unitActionPointStatDefinition.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
			unitActionPointStatDefinition.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal];
		}
		if ((Object)(object)unitMovePointStatDefinition != (Object)null)
		{
			unitMovePointStatDefinition.TargetUnit = PlayableUnit;
			unitMovePointStatDefinition.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
			unitMovePointStatDefinition.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal];
		}
		if ((Object)(object)unitPortraitBoxHovered != (Object)null)
		{
			((Behaviour)unitPortraitBoxHovered).enabled = false;
		}
		isInited = true;
	}
}
