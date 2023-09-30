using TPLib;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuBackground : MonoBehaviour
{
	[SerializeField]
	private Image background;

	private Vector2 nativeSize;

	private void OnResolutionChanged(Resolution resolution)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if ((float)((Resolution)(ref resolution)).height >= 1440f)
		{
			((Graphic)background).rectTransform.sizeDelta = nativeSize * 4f;
		}
		else if ((float)((Resolution)(ref resolution)).height >= 1080f)
		{
			((Graphic)background).rectTransform.sizeDelta = nativeSize * 3f;
		}
		else
		{
			((Graphic)background).rectTransform.sizeDelta = nativeSize * 2f;
		}
	}

	private void OnDestroy()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChanged;
		}
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = background.sprite.rect;
		nativeSize = ((Rect)(ref rect)).size;
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChanged;
		OnResolutionChanged(TPSingleton<SettingsManager>.Instance.Settings.Resolution);
	}
}
