using TPLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLastStand.Framework.Database;

public class DatabaseLoader : TPSingleton<DatabaseLoader>
{
	public delegate void OnDatabasesLoadedHandler();

	[SerializeField]
	[Tooltip("Prefabs or nested instances.")]
	private GameObject[] databases;

	public bool Initialized { get; private set; }

	public event OnDatabasesLoadedHandler OnDatabasesLoadedEvent;

	public void InstantiateDatabase()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (databases == null)
		{
			return;
		}
		GameObject[] array = databases;
		foreach (GameObject val in array)
		{
			IDatabase component = val.GetComponent<IDatabase>();
			if (!component.GetHasBeenDeserialized())
			{
				Scene scene = val.scene;
				if (((Scene)(ref scene)).rootCount == 0)
				{
					Object.Instantiate<GameObject>(val, ((Component)this).transform);
					continue;
				}
				component.Deserialize();
				component.SetHasBeenDeserialized(hasBeenDeserialized: true);
			}
		}
		Debug.Log((object)"#Application.#Databases deserialized!", (Object)(object)this);
		Initialized = true;
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid && databases != null)
		{
			InstantiateDatabase();
		}
	}
}
