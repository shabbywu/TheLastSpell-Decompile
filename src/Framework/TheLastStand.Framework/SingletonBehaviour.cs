using UnityEngine;

namespace TheLastStand.Framework;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	[SerializeField]
	private bool verbose;

	public static T Instance
	{
		get
		{
			if ((Object)(object)instance == (Object)null)
			{
				instance = Object.FindObjectOfType<T>();
			}
			return instance;
		}
	}

	public bool Verbose => verbose;
}
