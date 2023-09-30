using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Dev.View;

public class BonePilePercentagesView : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Vector2 offset = Vector2.zero;

	public void Toggle(bool state)
	{
		((Behaviour)canvas).enabled = state;
	}

	private void Awake()
	{
		Toggle(state: false);
	}

	private void Update()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)canvas).enabled || !TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
		{
			return;
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile != null)
		{
			string text = $"{tile.Id} (city distance={tile.DistanceToCity})\n";
			if (TPSingleton<EnemyUnitManager>.Instance.BonePilesPercentages.TryGetValue(tile, out var value))
			{
				foreach (KeyValuePair<string, int> item in value)
				{
					text += $"{item.Key}: {item.Value}%\n";
				}
			}
			else
			{
				text += "No percentage";
			}
			((TMP_Text)this.text).text = text;
			((Behaviour)this.text).enabled = true;
			Vector3 cellCenterWorldPosition = TileMapView.GetCellCenterWorldPosition(tile);
			((TMP_Text)this.text).transform.position = cellCenterWorldPosition + Vector2.op_Implicit(offset);
		}
		else
		{
			((Behaviour)this.text).enabled = false;
		}
	}
}
