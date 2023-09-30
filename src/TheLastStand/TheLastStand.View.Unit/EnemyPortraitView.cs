using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class EnemyPortraitView : MonoBehaviour
{
	[SerializeField]
	protected Image unitPortraitImage;

	public EnemyUnit EnemyUnit { get; set; }

	public void RefreshPortrait()
	{
		((Behaviour)unitPortraitImage).enabled = EnemyUnit != null;
		if (EnemyUnit != null)
		{
			unitPortraitImage.sprite = EnemyUnit.UiSprite;
		}
	}
}
