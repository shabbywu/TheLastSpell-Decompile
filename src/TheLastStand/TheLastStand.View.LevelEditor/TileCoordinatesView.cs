using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class TileCoordinatesView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI coordinatesText;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private Vector2 offset = Vector2.zero;

	private void OnToggleValueChanged(bool value)
	{
		((Component)coordinatesText).gameObject.SetActive(value);
	}

	private void Awake()
	{
		if ((Object)(object)toggle != (Object)null)
		{
			((UnityEvent<bool>)(object)toggle.onValueChanged).AddListener((UnityAction<bool>)delegate(bool value)
			{
				OnToggleValueChanged(value);
			});
		}
	}

	private void Update()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile == null)
		{
			((Behaviour)coordinatesText).enabled = false;
			return;
		}
		((Behaviour)coordinatesText).enabled = true;
		((TMP_Text)coordinatesText).text = $"{TPSingleton<GameManager>.Instance.Game.Cursor.Tile.X},{TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Y}";
		Vector3 cellCenterWorldPosition = TileMapView.GetCellCenterWorldPosition(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
		((TMP_Text)coordinatesText).transform.position = cellCenterWorldPosition + Vector2.op_Implicit(offset);
	}
}
