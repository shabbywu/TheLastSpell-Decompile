using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.UnitManagement.UI;

public class MovePathCounterHUD : TPSingleton<MovePathCounterHUD>
{
	[SerializeField]
	private Transform movePathCountContainer;

	[SerializeField]
	private TextMeshProUGUI movePathCountText;

	[SerializeField]
	private DataColor movePathCountValidColor;

	[SerializeField]
	private DataColor movePathCountInvalidColor;

	[SerializeField]
	private LocalizedFont localizedFont;

	public MovePath MovePath { get; set; }

	public TextMeshProUGUI MovePathCountText => movePathCountText;

	public void ChangeCounterColor(Color newColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)movePathCountText).color = newColor;
	}

	public void DisplayMovePathCount(bool isDisplayed = true)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)movePathCountContainer == (Object)null)
		{
			return;
		}
		if (!isDisplayed || MovePath.Path.Count == 0)
		{
			((Component)movePathCountContainer).gameObject.SetActive(false);
			return;
		}
		Tile tile = MovePath.Path[MovePath.Path.Count - 1];
		movePathCountContainer.position = TileMapView.GetWorldPosition(tile);
		((TMP_Text)movePathCountText).text = (MovePath.Path.Count - 1).ToString();
		((Component)movePathCountContainer).gameObject.SetActive(true);
		LocalizedFont obj = localizedFont;
		if (obj != null)
		{
			obj.RefreshFont();
		}
	}

	public void SetMovePathCountValidation(bool isValid)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		ChangeCounterColor(isValid ? movePathCountValidColor._Color : movePathCountInvalidColor._Color);
	}

	protected override void Awake()
	{
		base.Awake();
		if ((Object)(object)movePathCountContainer == (Object)null && (Object)(object)movePathCountText != (Object)null)
		{
			movePathCountContainer = ((TMP_Text)movePathCountText).transform;
		}
	}
}
