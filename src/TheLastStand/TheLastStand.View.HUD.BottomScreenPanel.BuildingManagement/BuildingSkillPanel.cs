using System;
using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillPanel : BuildingCapacityPanel
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private BuildingSkillHighlight highlightRaycaster;

	[SerializeField]
	private TextMeshProUGUI nightUsesCountText;

	[SerializeField]
	private GameObject usesRemainingContainer;

	[SerializeField]
	private TextMeshProUGUI usesRemainingLeftText;

	[SerializeField]
	private DataColor noUsesRemainingColor;

	private bool displayed;

	private Color usesRemainingBaseColor;

	public BetterButton Button => button;

	public TheLastStand.Model.Skill.Skill Skill { get; set; }

	public TheLastStand.Model.Building.Building SkillOwner { get; set; }

	public event Action<BuildingSkillPanel> Clicked;

	public void Display(bool show)
	{
		displayed = show;
		((Component)base.BuildingCapacityRect).gameObject.SetActive(show);
	}

	public override void DisplayTooltip(bool show)
	{
		if (show)
		{
			SkillTooltip skillInfoPanel = SkillManager.SkillInfoPanel;
			skillInfoPanel.FollowElement.ChangeTarget(((Component)this).transform);
			skillInfoPanel.SetContent(Skill, SkillOwner.BattleModule);
			skillInfoPanel.DisplayInvalidityPanel = true;
			skillInfoPanel.Display();
		}
		else
		{
			SkillManager.SkillInfoPanel.Hide();
		}
	}

	public override void OnSkillPanelHovered(bool hover)
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill)
		{
			return;
		}
		if (hover)
		{
			DisplayTooltip(show: true);
			TileMapView.ClearTiles(TileMapView.ReachableTilesTilemap);
			if (BuildingManager.SelectedSkill != null)
			{
				if (BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.HasTargets)
				{
					BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.SaveTargets();
				}
				BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
			}
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner.BattleModule);
			BuildingManager.PreviewedSkill = Skill;
			return;
		}
		if (BuildingManager.PreviewedSkill != null)
		{
			BuildingManager.PreviewedSkill = null;
		}
		if (BuildingManager.SelectedSkill != Skill)
		{
			Skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		}
		if (BuildingManager.SelectedSkill != null)
		{
			BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(SkillOwner.BattleModule);
			BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.RestoreTargets();
		}
		DisplayTooltip(show: false);
	}

	public override void OnSkillHover(bool select)
	{
		base.OnSkillHover(select);
		if (select)
		{
			if (Button.Interactable)
			{
				BuildingManager.SelectedSkill = Skill;
				Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				SkillManager.RefreshSelectedSkillValidityOnTile(tile);
				BuildingManager.SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.DisplayAreaOfEffect(tile);
			}
		}
		else
		{
			BuildingManager.SelectedSkill = null;
		}
	}

	public override void Refresh()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (displayed)
		{
			Button.Interactable = Skill.SkillController.CanExecuteSkill(-1f, -1f, -1f, -1f, isStun: false);
			((Component)highlightRaycaster).gameObject.SetActive(!Button.Interactable);
			icon.sprite = SkillView.GetIconSprite(Skill.SkillDefinition.ArtId);
			((TMP_Text)nightUsesCountText).text = Skill.OverallUsesRemaining.ToString();
			((Graphic)nightUsesCountText).color = ((Skill.OverallUsesRemaining == 0) ? noUsesRemainingColor._Color : usesRemainingBaseColor);
			usesRemainingContainer.SetActive(Skill.SkillDefinition.UsesPerTurnCount > -1);
			if (usesRemainingContainer.activeSelf)
			{
				((TMP_Text)usesRemainingLeftText).text = $"{Skill.UsesPerTurnRemaining}/{Skill.SkillDefinition.UsesPerTurnCount}";
			}
		}
	}

	private void Button_Clicked()
	{
		((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).Log((object)("Building skill " + Skill.SkillDefinition.Id + " was clicked."), (CLogLevel)0, false, false);
		this.Clicked?.Invoke(this);
	}

	private void Button_OnPointerEnter()
	{
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
	}

	private void Awake()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if ((Object)(object)Button != (Object)null)
		{
			((UnityEvent)((Button)Button).onClick).AddListener(new UnityAction(Button_Clicked));
			Button.OnPointerEnterEvent.AddListener(new UnityAction(Button_OnPointerEnter));
		}
		usesRemainingBaseColor = ((Graphic)nightUsesCountText).color;
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)Button != (Object)null)
		{
			((UnityEvent)((Button)Button).onClick).RemoveListener(new UnityAction(Button_Clicked));
			((UnityEventBase)Button.OnPointerEnterEvent).RemoveAllListeners();
		}
	}
}
