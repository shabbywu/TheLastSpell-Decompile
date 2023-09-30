using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.View.Building;

public class DamagedBuildingFlameView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private Animator animator;

	private bool isUnloading;

	public Animator Animator => animator;

	public SpriteRenderer SpriteRenderer => spriteRenderer;

	private void Start()
	{
		SceneManager.sceneUnloaded += delegate
		{
			isUnloading = true;
		};
	}

	private void OnDestroy()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!isUnloading)
		{
			Scene scene = ((Component)this).gameObject.scene;
			_ = ((Scene)(ref scene)).isLoaded;
		}
	}
}
