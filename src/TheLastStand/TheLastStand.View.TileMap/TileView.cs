using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Building;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.TileMap;

public class TileView : MonoBehaviour, ITileObjectView
{
	public static class Constants
	{
		public static class LocalizationPrefixes
		{
			public const string GroundNamePrefix = "GroundName_";

			public const string GroundDescriptionPrefix = "GroundDescription_";
		}

		public static class Sprites
		{
			public const string GroundPortraitImagePrefix = "View/Sprites/UI/Grounds/Portraits/64px/GroundsPortraits_";
		}
	}

	[SerializeField]
	private Transform deadBodyParent;

	private SkillTargetingMark skillTargetingMark;

	private bool hovered;

	private bool selected;

	public Tile Tile { get; set; }

	public bool Hovered
	{
		get
		{
			return hovered;
		}
		set
		{
			if (hovered != value)
			{
				hovered = value;
				RefreshCursorFeedback();
			}
		}
	}

	public bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			if (selected != value)
			{
				selected = value;
				RefreshCursorFeedback();
			}
		}
	}

	public GameObject GameObject => ((Component)this).gameObject;

	public bool HoveredOrSelected
	{
		get
		{
			if (!Hovered)
			{
				return Selected;
			}
			return true;
		}
	}

	public void ToggleSkillTargeting(bool display)
	{
		if (display)
		{
			bool hover = (Object)(object)GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered() == (Object)null && TPSingleton<GameManager>.Instance.Game.Cursor.Tile == Tile;
			OnSkillTargetHover(hover);
			UnityEvent onShow = skillTargetingMark.OnShow;
			if (onShow != null)
			{
				onShow.Invoke();
			}
		}
		else
		{
			ReleaseSkillTargeting();
			skillTargetingMark = null;
		}
	}

	public void RefreshCursorFeedback()
	{
	}

	public void OnSkillTargetHover(bool hover)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)skillTargetingMark == (Object)null)
		{
			skillTargetingMark = ObjectPooler.GetPooledComponent<SkillTargetingMark>("SkillTargetingMarkSprite", SkillManager.SkillTargetingMarkSpritePrefab, (Transform)null, false);
			((Component)skillTargetingMark).transform.SetParent(((Component)this).transform);
			((Component)skillTargetingMark).transform.localPosition = Vector3.up * 0.5f * TileMapView.Grid.cellSize.y;
			((Component)skillTargetingMark).transform.localScale = Vector3.one;
		}
		if (!((Component)skillTargetingMark).gameObject.activeInHierarchy)
		{
			((Component)skillTargetingMark).gameObject.SetActive(true);
		}
		skillTargetingMark.SetHoverAnimatorState(hover);
	}

	public void InstantiateDeadBody(EnemyUnit enemy)
	{
		EnemyUnitDeadBodyView enemyUnitDeadBodyView = Object.Instantiate<EnemyUnitDeadBodyView>(TileMapView.DeadBodyPrefab, deadBodyParent);
		enemyUnitDeadBodyView.Tile = Tile;
		Tile.EnemyUnitDeadBodyViews.Add(enemyUnitDeadBodyView);
		enemyUnitDeadBodyView.Init(enemy);
	}

	public void InstantiateDeadBuilding(BuildingDefinition building)
	{
		BuildingCorpseView buildingCorpseView = Object.Instantiate<BuildingCorpseView>(TileMapView.DeadBuildingPrefab, deadBodyParent);
		buildingCorpseView.Tile = Tile;
		Tile.DeadBuildingViews.Add(buildingCorpseView);
		buildingCorpseView.Init(building);
	}

	public void ReleaseSkillTargeting()
	{
		if (!((Object)(object)skillTargetingMark == (Object)null))
		{
			UnityEvent onHide = skillTargetingMark.OnHide;
			if (onHide != null)
			{
				onHide.Invoke();
			}
			ObjectPooler.SetPoolAsParent(((Component)skillTargetingMark).gameObject, "SkillTargetingMarkSprite");
		}
	}

	public Sprite GetPortraitSprite()
	{
		string id = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Grounds/Portraits/64px/GroundsPortraits_" + Tile.GroundDefinition.Id + "_" + id, false) ?? ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Grounds/Portraits/64px/GroundsPortraits_" + Tile.GroundDefinition.Id, false);
	}
}
