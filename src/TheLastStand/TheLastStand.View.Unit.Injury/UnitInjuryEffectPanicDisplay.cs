using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryEffectPanicDisplay : MonoBehaviour
{
	[SerializeField]
	private Image injuryIcon;

	[SerializeField]
	private DataSpriteTable injuryIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private float panicMalus;

	public virtual void Init(float newPanicMalus, int injuryStage)
	{
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		panicMalus = newPanicMalus;
		RefreshTitle();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
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
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		((TMP_Text)titleText).text = string.Format("{0}{1} {2} {3}", (panicMalus >= 0f) ? "+" : string.Empty, panicMalus, AtlasIcons.PanicIcon, Localizer.Get("Panic_Name"));
	}
}
