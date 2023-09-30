using TPLib;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.View.Unit;

public class SelectedUnitFeedback : MonoBehaviour
{
	[SerializeField]
	private Color playableUnitColorFeedback;

	[SerializeField]
	private Color enemyUnitColorFeedback;

	[SerializeField]
	private FollowPosition followPosition;

	[SerializeField]
	private SpriteRenderer groundFxRenderer;

	[SerializeField]
	private SpriteRenderer haloFxRenderer;

	[SerializeField]
	private Transform feedbackTransform;

	private TheLastStand.Model.Unit.Unit unit;

	private int? groundFxRendererInitOrder;

	private int? haloFxRendererInitOrder;

	public TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			Refresh();
		}
	}

	public void Display(bool display)
	{
		if (display)
		{
			followPosition.SetPosition();
		}
		((Component)followPosition).gameObject.SetActive(display);
	}

	public void Refresh()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		followPosition.Target = ((Component)this.unit.UnitView).transform;
		int valueOrDefault = groundFxRendererInitOrder.GetValueOrDefault();
		if (!groundFxRendererInitOrder.HasValue)
		{
			valueOrDefault = ((Renderer)groundFxRenderer).sortingOrder;
			groundFxRendererInitOrder = valueOrDefault;
		}
		valueOrDefault = haloFxRendererInitOrder.GetValueOrDefault();
		if (!haloFxRendererInitOrder.HasValue)
		{
			valueOrDefault = ((Renderer)haloFxRenderer).sortingOrder;
			haloFxRendererInitOrder = valueOrDefault;
		}
		TheLastStand.Model.Unit.Unit unit = this.unit;
		if (!(unit is PlayableUnit playableUnit))
		{
			if (unit is EnemyUnit enemyUnit)
			{
				int? sortingOrderOverride = enemyUnit.EnemyUnitTemplateDefinition.SortingOrderOverride;
				if (sortingOrderOverride.HasValue)
				{
					((Renderer)groundFxRenderer).sortingOrder = groundFxRendererInitOrder.Value + sortingOrderOverride.Value - 30;
					((Renderer)haloFxRenderer).sortingOrder = haloFxRendererInitOrder.Value + sortingOrderOverride.Value - 30;
				}
				else
				{
					((Renderer)groundFxRenderer).sortingOrder = groundFxRendererInitOrder.Value;
					((Renderer)haloFxRenderer).sortingOrder = haloFxRendererInitOrder.Value;
				}
				groundFxRenderer.color = enemyUnitColorFeedback;
				haloFxRenderer.color = enemyUnitColorFeedback;
			}
		}
		else
		{
			((Renderer)groundFxRenderer).sortingOrder = groundFxRendererInitOrder.Value;
			((Renderer)haloFxRenderer).sortingOrder = haloFxRendererInitOrder.Value;
			groundFxRenderer.color = playableUnit.PortraitColor._Color;
			haloFxRenderer.color = playableUnitColorFeedback;
		}
		feedbackTransform.localScale = this.unit.UnitTemplateDefinition.SelectedFeedbackSize;
	}
}
