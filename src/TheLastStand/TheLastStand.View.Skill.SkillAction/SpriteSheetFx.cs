using Sirenix.OdinInspector;
using TheLastStand.Framework.Animation;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction;

[RequireComponent(typeof(SingleAnimPlayer))]
public class SpriteSheetFx : SerializedMonoBehaviour, IDisplayableEffect
{
	[SerializeField]
	private SingleAnimPlayer animPlayer;

	public Coroutine Display()
	{
		((Component)this).gameObject.SetActive(true);
		return animPlayer.Play();
	}

	public void Init(Tile targetTile)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = TileMapView.GetWorldPosition(targetTile);
	}

	public void Init(Vector3 worldPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = worldPos;
	}

	protected void Awake()
	{
		if ((Object)(object)animPlayer == (Object)null)
		{
			animPlayer = ((Component)this).GetComponent<SingleAnimPlayer>();
		}
		((Component)this).gameObject.SetActive(false);
	}
}
