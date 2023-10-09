using System;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.Manager;

public class SpritesLocalizeManager : Manager<SpritesLocalizeManager>
{
	[SerializeField]
	private SpritesLocalizedDictionnary spritesLocalizedDictionnary;

	private string CurrentLanguage => Localizer.language;

	public Sprite Get(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (spritesLocalizedDictionnary.SpritesByLanguages.TryGetValue(CurrentLanguage, out var value))
		{
			if (value.TryFind((SpritesLocalizedDictionnary.KeySprite x) => x.Key == key, out var value2))
			{
				return value2.Sprite;
			}
			return null;
		}
		if (string.IsNullOrEmpty(CurrentLanguage))
		{
			((CLogger<SpritesLocalizeManager>)this).LogError((object)"CurrentLanguage has no value !", (CLogLevel)1, true, true);
		}
		else
		{
			((CLogger<SpritesLocalizeManager>)this).LogError((object)(CurrentLanguage + " : this language isn't present in dictonnary !"), (CLogLevel)1, true, true);
		}
		return null;
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
	}
}
