using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Menus;

public class OpenURLButton : MonoBehaviour
{
	[SerializeField]
	private string urlToOpen = string.Empty;

	public void OpenURL()
	{
		if (urlToOpen == string.Empty)
		{
			((CLogger<ApplicationManager>)TPSingleton<ApplicationManager>.Instance).LogWarning((object)"There is no url to upen on that button", (CLogLevel)1, true, false);
		}
		else
		{
			Application.OpenURL(urlToOpen);
		}
	}
}
