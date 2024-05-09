using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Unit;

public class PlayableUnitGhostView : PlayableUnitView
{
	[SerializeField]
	private DataColor validColor;

	[SerializeField]
	private DataColor invalidColor;

	public void ChangeColors(bool isValid)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		ChangeColors(isValid ? validColor._Color : invalidColor._Color);
	}

	public void Display(bool displayed)
	{
		if (((Component)this).gameObject.activeSelf != displayed)
		{
			if (displayed)
			{
				RefreshBodyParts();
			}
			((Component)this).gameObject.SetActive(displayed);
			if (!base.AreAnimationsInitialized)
			{
				InitAndStartAnimations(playSpawnAnim: false);
			}
		}
	}

	public void FollowMouse()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = TileMapView.GetWorldPosition(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
		((Component)this).transform.position = new Vector3(worldPosition.x, worldPosition.y, ((Component)this).transform.position.z);
	}

	public override void PrepareForSnapshot()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base.PrepareForSnapshot();
		ChangeColors(Color.white);
		if (!base.AreAnimationsInitialized)
		{
			InitAndStartAnimations(playSpawnAnim: false);
		}
	}

	protected void ChangeColors(Color col)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < bodyPartViews.Length; i++)
		{
			bodyPartViews[i].Tint(col);
		}
	}

	protected override void InitHud()
	{
	}
}
