using UnityEngine;

namespace TheLastStand.View.Unit;

public class EnemyAttackFeedback : MonoBehaviour
{
	[SerializeField]
	[Range(0.1f, 10f)]
	private float speed = 0.4f;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private int destroyTimer = 20;

	private int spawnTimer = 1;

	public EnemyUnitView EnemyUnitView { get; set; }

	public SpriteRenderer SpriteRenderer => spriteRenderer;

	public Vector3 TargetPosition { get; set; }

	private void Update()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (--destroyTimer == 0)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else if (!(Vector3.Distance(((Component)this).transform.position, TargetPosition) < 0.1f) && --spawnTimer == 0)
		{
			EnemyAttackFeedback enemyAttackFeedback = Object.Instantiate<EnemyAttackFeedback>(EnemyUnitView.EnemyAttackFeedbackPrefab);
			((Component)enemyAttackFeedback).transform.position = Vector3.MoveTowards(((Component)this).transform.position, TargetPosition, speed);
			enemyAttackFeedback.TargetPosition = TargetPosition;
			((Component)enemyAttackFeedback).transform.localScale = ((Component)this).transform.localScale;
			enemyAttackFeedback.SpriteRenderer.color = spriteRenderer.color;
			enemyAttackFeedback.EnemyUnitView = EnemyUnitView;
		}
	}
}
