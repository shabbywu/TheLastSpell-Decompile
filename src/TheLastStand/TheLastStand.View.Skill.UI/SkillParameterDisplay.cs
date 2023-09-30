using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillParameterDisplay : MonoBehaviour
{
	[SerializeField]
	protected TextMeshProUGUI effectBonusText;

	[SerializeField]
	protected TextMeshProUGUI effectNameText;

	[SerializeField]
	protected TextMeshProUGUI effectValueText;

	[SerializeField]
	protected Color effectValueColor = Color.white;

	[SerializeField]
	protected TextMeshProUGUI operatorSignText;

	[SerializeField]
	protected GameObject separator;

	protected string nameLocalizationKey;

	public virtual void Display(bool show)
	{
		if ((Object)(object)separator != (Object)null)
		{
			separator.SetActive(show);
		}
		((Component)this).gameObject.SetActive(show);
	}

	public float GetVerticalPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((Transform)((TMP_Text)effectValueText).rectTransform).localPosition.y;
	}

	public float GetPositionAtTheEndOfLine()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return ((Transform)((TMP_Text)effectValueText).rectTransform).localPosition.x + ((TMP_Text)effectValueText).rectTransform.sizeDelta.x;
	}

	public virtual void Refresh(string effectName, string effectValue, string overrideSign = "")
	{
		nameLocalizationKey = effectName;
		RefreshName();
		((TMP_Text)effectValueText).text = effectValue;
		if ((Object)(object)operatorSignText != (Object)null && !string.IsNullOrEmpty(overrideSign))
		{
			((TMP_Text)operatorSignText).text = overrideSign;
		}
		if ((Object)(object)effectBonusText != (Object)null)
		{
			((TMP_Text)effectBonusText).text = string.Empty;
		}
	}

	public virtual void Refresh(string effectName, string effectValue, Color? valueColor, string overrideSign = "")
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (valueColor.HasValue)
		{
			effectValueColor = valueColor.Value;
			RefreshColor();
		}
		Refresh(effectName, effectValue, overrideSign);
	}

	public virtual void Refresh(string effectName, string effectValue, int bonusValue, string overrideSign = "")
	{
		Refresh(effectName, effectValue, overrideSign);
		((TMP_Text)effectBonusText).text = ((bonusValue > 0) ? $"<style=GoodNb>+{bonusValue}</style>" : string.Empty);
	}

	protected virtual void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshName();
		}
	}

	protected virtual void RefreshColor()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)effectValueText).color = effectValueColor;
		if ((Object)(object)operatorSignText != (Object)null)
		{
			((Graphic)operatorSignText).color = effectValueColor;
		}
	}

	protected virtual void RefreshName()
	{
		((TMP_Text)effectNameText).text = Localizer.Get(nameLocalizationKey);
	}

	protected virtual void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected virtual void OnDestroy()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}
}
