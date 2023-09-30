using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class UISizePanel : MonoBehaviour
{
	[SerializeField]
	private UIScalerSlider uiSizeSlider;

	[SerializeField]
	private RectTransform windowSizeReference;

	private Vector2 referenceSize = Vector2.zero;

	public void Refresh()
	{
		uiSizeSlider.RefreshValue(TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale, applyMultiplier: true);
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = windowSizeReference.rect;
		referenceSize = ((Rect)(ref rect)).size;
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += delegate(Resolution res)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				uiSizeSlider.AutoSetValueRange(res, referenceSize);
			};
		}
		Refresh();
	}

	private void Start()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		uiSizeSlider.AutoSetValueRange(TPSingleton<SettingsManager>.Instance.Settings.Resolution, referenceSize);
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= delegate(Resolution res)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				uiSizeSlider.AutoSetValueRange(res, referenceSize);
			};
		}
	}
}
