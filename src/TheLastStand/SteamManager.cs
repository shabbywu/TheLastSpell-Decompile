using System;
using System.Text;
using Steamworks;
using TheLastStand;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
	[SerializeField]
	private uint appId = 1105670u;

	[SerializeField]
	private uint demoAppId = 1297280u;

	[SerializeField]
	private bool isDemo;

	protected static SteamManager s_instance;

	protected static bool s_EverInitialized;

	protected bool m_bInitialized;

	protected bool m_bInitializationFailed;

	protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	public string ConsoleDebugLog = "Steam";

	protected static SteamManager Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)s_instance == (Object)null)
			{
				return new GameObject("SteamManager").AddComponent<SteamManager>();
			}
			return s_instance;
		}
	}

	public static bool Initialized => Instance.m_bInitialized;

	public static bool InitializationFailed => Instance.m_bInitializationFailed;

	protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning((object)pchDebugText);
	}

	protected virtual void Awake()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)s_instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		s_instance = this;
		if (s_EverInitialized)
		{
			throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
		if (!Packsize.Test())
		{
			OnSteamInitialisationFailed("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			return;
		}
		if (!DllCheck.Test())
		{
			OnSteamInitialisationFailed("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
			return;
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(new AppId_t(isDemo ? demoAppId : appId)))
			{
				OnSteamInitialisationFailed("RestartAppIfNecessary() returned true");
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			OnSteamInitialisationFailed("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex);
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			OnSteamInitialisationFailed("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
			return;
		}
		s_EverInitialized = true;
		Log("Initialized", (Object)(object)this);
		Analytics.SendGameStartEvent();
	}

	protected virtual void OnEnable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if ((Object)(object)s_instance == (Object)null)
		{
			s_instance = this;
		}
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected void OnSteamInitialisationFailed(string failMessage)
	{
		LogError(failMessage, (Object)(object)this);
		m_bInitializationFailed = true;
		Application.Quit();
	}

	protected virtual void OnDestroy()
	{
		if (!((Object)(object)s_instance != (Object)(object)this))
		{
			s_instance = null;
			if (m_bInitialized)
			{
				Log("Shutting down", (Object)(object)this);
				SteamAPI.Shutdown();
			}
		}
	}

	protected virtual void Update()
	{
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	public void Log(string log)
	{
		Debug.Log((object)("#" + ConsoleDebugLog + "#" + log));
	}

	public void Log(string log, Object context)
	{
		Debug.Log((object)("#" + ConsoleDebugLog + "#" + log), context);
	}

	public void LogError(string log)
	{
		Debug.LogError((object)("#" + ConsoleDebugLog + "#" + log));
	}

	public void LogError(string log, Object context)
	{
		Debug.LogError((object)("#" + ConsoleDebugLog + "#" + log), context);
	}

	public void LogWarning(string log)
	{
		Debug.LogWarning((object)("#" + ConsoleDebugLog + "#" + log));
	}

	public void LogWarning(string log, Object context)
	{
		Debug.LogWarning((object)("#" + ConsoleDebugLog + "#" + log), context);
	}
}
