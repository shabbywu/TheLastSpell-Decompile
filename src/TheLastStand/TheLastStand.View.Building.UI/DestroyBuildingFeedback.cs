using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class DestroyBuildingFeedback : MonoBehaviour
{
	[SerializeField]
	[Range(0.01f, 0.1f)]
	private float destroySpeed = 0.05f;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Image progressImage;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private Color startColor = Color.white;

	[SerializeField]
	private Color endColor = Color.red;

	[SerializeField]
	private Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f);

	[SerializeField]
	private Vector3 endScale = new Vector3(1f, 1f, 1f);

	private float destroyProgress;

	private TheLastStand.Model.Building.Building currentBuilding;

	private bool hasDestroyedBuildingOnTile;

	public void Toggle(bool toggle)
	{
		((Component)this).gameObject.SetActive(toggle);
		if (!toggle)
		{
			Reset(null);
		}
	}

	protected virtual void Awake()
	{
		Toggle(toggle: false);
	}

	protected virtual void Update()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			hasDestroyedBuildingOnTile = false;
		}
		if (hasDestroyedBuildingOnTile || TPSingleton<ConstructionManager>.Instance.Construction.DestroyMode == TheLastStand.Model.Building.Construction.E_DestroyMode.None)
		{
			return;
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile?.Building != null && (tile.Building.BlueprintModule.IsIndestructible || !tile.Building.DamageableModule.IsDead))
		{
			if (tile.Building != currentBuilding)
			{
				Reset(tile.Building);
			}
			if (!tile.Building.BuildingDefinition.ConstructionModuleDefinition.IsDemolishable)
			{
				return;
			}
			if (InputManager.GetButton(24))
			{
				destroyProgress = Mathf.Clamp(destroyProgress + destroySpeed, 0f, 1f);
			}
			else
			{
				Reset(tile.Building);
			}
			Color color = Color.Lerp(startColor, endColor, destroyProgress);
			progressImage.fillAmount = destroyProgress;
			((Graphic)progressImage).color = color;
			if (!(destroyProgress > 0f))
			{
				return;
			}
			Vector3 localScale = Vector3.Lerp(startScale, endScale, destroyProgress);
			((TMP_Text)progressText).text = $"{(int)(100f * destroyProgress)}%";
			((Graphic)progressText).color = color;
			((TMP_Text)progressText).transform.localScale = localScale;
			((Graphic)text).color = color;
			((TMP_Text)text).transform.localScale = localScale;
			((Behaviour)progressText).enabled = true;
			if (destroyProgress >= 1f)
			{
				hasDestroyedBuildingOnTile = true;
				if (tile.Building.BuildingController.DamageableModuleController != null)
				{
					tile.Building.BuildingController.DamageableModuleController.Demolish();
				}
				else
				{
					BuildingManager.DestroyBuilding(tile, updateView: true, addDeadBuilding: false, triggerEvent: false, triggerOnDeathEvent: false);
				}
				Reset(null);
			}
		}
		else
		{
			Reset(null);
		}
	}

	private void Reset(TheLastStand.Model.Building.Building building)
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		destroyProgress = 0f;
		currentBuilding = building;
		if (currentBuilding != null)
		{
			if (currentBuilding.BuildingDefinition.ConstructionModuleDefinition.IsDemolishable)
			{
				((TMP_Text)text).text = Localizer.Format("ConstructionPanel_DestroyNoCostTip", new object[1] { currentBuilding.BuildingDefinition.Name });
				((Behaviour)text).enabled = true;
				((Behaviour)progressText).enabled = true;
				((Behaviour)progressImage).enabled = true;
			}
			else
			{
				((TMP_Text)text).text = Localizer.Format("ConstructionPanel_CantDestroy", new object[1] { currentBuilding.BuildingDefinition.Name });
				((Behaviour)text).enabled = true;
				((Behaviour)progressText).enabled = false;
				((Behaviour)progressImage).enabled = false;
			}
		}
		else
		{
			((Behaviour)text).enabled = false;
			((Behaviour)progressText).enabled = false;
			((Behaviour)progressImage).enabled = false;
		}
		((Graphic)text).color = startColor;
		((TMP_Text)text).transform.localScale = startScale;
	}
}
