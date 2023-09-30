using TPLib;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Camera;

public class MaskableBehaviour : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("_maskMaterial")]
	protected Material maskMaterial;

	protected LutMaskGenerator maskGenerator;

	private Material backupMaterial;

	private Renderer myRenderer;

	private Coroutine coroutine;

	protected void Start()
	{
		Init();
	}

	protected void OnEnable()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			Init();
		}
	}

	public void Init()
	{
		maskGenerator = TPSingleton<LutMaskGenerator>.Instance;
		if ((Object)(object)maskGenerator == (Object)null)
		{
			TPDebug.LogError((object)"Mask Generator not found !", (Object)(object)this);
			((Behaviour)this).enabled = false;
			return;
		}
		myRenderer = ((Component)this).GetComponent<Renderer>();
		if ((Object)(object)myRenderer == (Object)null)
		{
			TPDebug.LogError((object)"No component with material found on this MaskableBehaviour", (Object)(object)this);
		}
		else
		{
			backupMaterial = myRenderer.material;
		}
	}

	private void OnBecameInvisible()
	{
		if (((Behaviour)this).enabled)
		{
			maskGenerator.MaskableObjects.Remove(this);
		}
	}

	private void OnBecameVisible()
	{
		if (((Behaviour)this).enabled)
		{
			if (coroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(coroutine);
				coroutine = null;
			}
			if (!maskGenerator.MaskableObjects.Contains(this))
			{
				maskGenerator.MaskableObjects.Add(this);
			}
		}
	}

	public void SwitchMaterial(bool useMaskMat = true)
	{
		myRenderer.material = (useMaskMat ? maskMaterial : backupMaterial);
	}
}
