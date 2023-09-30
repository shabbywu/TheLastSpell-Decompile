using UnityEngine;

namespace TheLastStand.View;

public class DisableSelf : MonoBehaviour
{
	public void Disable()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
