using System;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View;

public class GameObjectEnablerOnLocalize : MonoBehaviour
{
	[SerializeField]
	private GameObject gameObjectToEnable;

	[SerializeField]
	private string[] targetLanguages;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		OnLocalize();
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			return;
		}
		gameObjectToEnable.SetActive(false);
		for (int i = 0; i < targetLanguages.Length; i++)
		{
			if (targetLanguages[i] == Localizer.language)
			{
				gameObjectToEnable.SetActive(true);
				break;
			}
		}
	}
}
