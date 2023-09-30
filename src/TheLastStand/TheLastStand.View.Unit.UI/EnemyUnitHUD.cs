using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class EnemyUnitHUD : UnitHUD
{
	[SerializeField]
	protected Image highlightWithArmorImage;

	[SerializeField]
	protected Canvas highlightWithArmorCanvas;

	[SerializeField]
	protected Sprite baseHighlightSprite;

	[SerializeField]
	protected Sprite stunHighlightSprite;

	[SerializeField]
	protected GameObject stunFeedbackNoArmor;

	[SerializeField]
	protected GameObject stunFeedbackArmor;

	[SerializeField]
	protected GameObject panicSign;

	public EnemyUnit EnemyUnit => base.Unit as EnemyUnit;

	public override bool Highlight
	{
		get
		{
			if ((Object)(object)highlightImage != (Object)null)
			{
				return ((Behaviour)highlightImage).enabled;
			}
			return false;
		}
		set
		{
			if ((Object)(object)highlightImage != (Object)null)
			{
				((Behaviour)highlightCanvas).enabled = (value || EnemyUnit.IsStunned) && !ShouldArmorBeDisplayed;
			}
			if ((Object)(object)highlightWithArmorImage != (Object)null)
			{
				((Behaviour)highlightWithArmorCanvas).enabled = (value || EnemyUnit.IsStunned) && ShouldArmorBeDisplayed;
				highlightWithArmorImage.sprite = (EnemyUnit.IsStunned ? stunHighlightSprite : baseHighlightSprite);
			}
		}
	}

	public override void DisplayIconAndTileFeedback(bool show)
	{
		if (!show || (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits && !base.Unit.IsDead))
		{
			base.DisplayIconAndTileFeedback(show);
			if (poisonDeathFeedback.activeSelf)
			{
				panicSign.SetActive(false);
			}
			else if (EnemyUnit.ShouldCausePanic)
			{
				panicSign.SetActive(show && !base.Unit.IsStunned);
				TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, base.Unit.OccupiedTiles, (show && !base.Unit.IsStunned) ? "View/Tiles/Feedbacks/PanicOnEnemy" : null);
			}
		}
	}

	public override void DisplayIconFeedback(bool show = true)
	{
		base.DisplayIconFeedback(show);
		panicSign.SetActive(show && !base.Unit.WillDieByPoison && !base.Unit.IsStunned && EnemyUnit.ShouldCausePanic);
	}

	public override void ForceHideHUD()
	{
		base.ForceHideHUD();
		((Behaviour)highlightWithArmorCanvas).enabled = false;
		((Behaviour)highlightCanvas).enabled = false;
		stunFeedbackArmor.SetActive(false);
		stunFeedbackNoArmor.SetActive(false);
		panicSign.SetActive(false);
	}

	public override void RefreshStatuses()
	{
		if (base.Unit != null)
		{
			highlightImage.sprite = (base.Unit.IsStunned ? stunHighlightSprite : baseHighlightSprite);
			Highlight = base.Unit.IsStunned;
			if ((Object)(object)stunFeedbackArmor != (Object)null)
			{
				stunFeedbackArmor.SetActive(base.Unit.IsStunned && base.DisplayArmor);
			}
			if ((Object)(object)stunFeedbackNoArmor != (Object)null)
			{
				stunFeedbackNoArmor.SetActive(base.Unit.IsStunned && !base.DisplayArmor);
			}
			base.RefreshStatuses();
		}
	}
}
