using System;
using PortraitAPI;
using PortraitAPI.Layers;
using PortraitAPI.Misc;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.UI;
using TheLastStand.Database.Unit;
using TheLastStand.View.Camera;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PortraitCodePopup : ACustomizationPopup
{
	public override void Close()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.PortraitCodePanel.IsEditingCode = false;
		base.Close();
	}

	public override void OnCloseButtonClicked()
	{
		Close();
	}

	public override void OnValidateButtonClicked()
	{
		CodeData val = default(CodeData);
		if (CodeGenerator.TryDecode(inputField.text, ref val))
		{
			if (inputField.text.Length == 12)
			{
				val.SetColorDatas(IEnumerableExtension.Clone(TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentIndexByColorType));
			}
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.UpdateTextureHandlersOnChangePortraitCode(val);
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.UpdateColorHandlersOnChangePortraitCode(val);
			CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<PlayableUnitCustomisationPanel>.Instance);
			TPSingleton<PlayableUnitCustomisationPanel>.Instance.PortraitCodePanel.IsEditingCode = false;
			Close();
		}
	}

	public override void OnValueChanged(string value)
	{
		string text = value;
		foreach (char c in text)
		{
			if (!char.IsNumber(c) && !char.IsLetter(c))
			{
				value = previousValue;
				inputField.SetTextWithoutNotify(value);
				break;
			}
		}
		previousValue = value;
		base.OnValueChanged(value);
	}

	public void Refresh(string code)
	{
		inputField.text = code;
	}

	protected override E_ErrorCause CheckValidity(string value)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected I4, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		if (value.Length < 30 && value.Length != 12)
		{
			((TMP_Text)errorText).text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.MinSize, new object[1] { 30 });
			return E_ErrorCause.MinSize;
		}
		E_ErrorCause e_ErrorCause = base.CheckValidity(value);
		if (e_ErrorCause != 0)
		{
			return e_ErrorCause;
		}
		CodeData val = default(CodeData);
		if (!CodeGenerator.TryDecode(value, ref val))
		{
			((TMP_Text)errorText).text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.InvalidLayerValue, Array.Empty<object>());
			return E_ErrorCause.InvalidLayerValue;
		}
		for (int i = 0; i < val.CodeSectionDatas.Count; i++)
		{
			E_LayerType val2 = (E_LayerType)i;
			if (val.CodeSectionDatas[val2].LayerIndex != -1 && !LayerManagement.IsThisIndexAvailable(val.CodeSectionDatas[val2].Gender, val2, val.CodeSectionDatas[val2].LayerIndex))
			{
				((TMP_Text)errorText).text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.InvalidLayerValue, Array.Empty<object>());
				return E_ErrorCause.InvalidLayerValue;
			}
		}
		for (int j = 0; j < val.CodeColorDatas.Count; j++)
		{
			E_ColorTypes val3 = (E_ColorTypes)j;
			bool flag = true;
			switch ((int)val3)
			{
			case 0:
				if (val.CodeColorDatas[val3].Index >= PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.Count || val.CodeColorDatas[val3].Index < 0)
				{
					flag = false;
				}
				break;
			case 1:
				if (val.CodeColorDatas[val3].Index >= PlayableUnitDatabase.PlayableUnitHairColorDefinitions.Count || val.CodeColorDatas[val3].Index < 0)
				{
					flag = false;
				}
				break;
			case 2:
				if (val.CodeColorDatas[val3].Index >= PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.Count || val.CodeColorDatas[val3].Index < 0)
				{
					flag = false;
				}
				break;
			case 3:
				if (val.CodeColorDatas[val3].Index >= PlayableUnitDatabase.PortraitBackgroundColors.Count || val.CodeColorDatas[val3].Index < 0)
				{
					flag = false;
				}
				break;
			}
			if (!flag)
			{
				((TMP_Text)errorText).text = Localizer.Format("HeroCustomization_CustoPopupError_" + E_ErrorCause.InvalidLayerValue, Array.Empty<object>());
				return E_ErrorCause.InvalidLayerValue;
			}
		}
		return e_ErrorCause;
	}
}
