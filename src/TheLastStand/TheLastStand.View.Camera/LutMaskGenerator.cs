using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(Camera))]
public class LutMaskGenerator : TPSingleton<LutMaskGenerator>
{
	[SerializeField]
	private AmplifyColorEffect amplifyColor;

	[SerializeField]
	private bool useScreenResolution = true;

	[SerializeField]
	private bool useCustomResolution;

	[SerializeField]
	protected int maskWidth = 1920;

	[SerializeField]
	protected int maskHeigth = 1080;

	private int width;

	private int height;

	private RenderTexture maskTexture;

	private Camera cam;

	private List<MaskableBehaviour> maskablesObjects;

	private RenderTexture activeRTBackup;

	public List<MaskableBehaviour> MaskableObjects
	{
		get
		{
			if (maskablesObjects == null)
			{
				maskablesObjects = new List<MaskableBehaviour>();
			}
			return maskablesObjects;
		}
	}

	protected void OnEnable()
	{
		Init();
		TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent += OnResolutionChange;
		UpdateRenderTexture();
		UpdateCameraProperties();
	}

	private void OnResolutionChange(Resolution resolution)
	{
		UpdateRenderTexture();
		UpdateCameraProperties();
	}

	protected void OnDisable()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.OnResolutionChangeEvent -= OnResolutionChange;
		}
		Cleanup();
	}

	public void Init()
	{
		if ((Object)(object)cam == (Object)null)
		{
			cam = ((Component)this).GetComponent<Camera>();
		}
		maskablesObjects = new List<MaskableBehaviour>();
	}

	private void UpdateRenderTexture()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		if (!((Object)(object)cam == (Object)null) && !((Object)(object)amplifyColor == (Object)null))
		{
			int num;
			int num2;
			if (useCustomResolution)
			{
				num = maskWidth;
				num2 = maskHeigth;
			}
			else if (useScreenResolution)
			{
				num = Screen.width;
				num2 = Screen.height;
			}
			else
			{
				num = (int)((float)cam.pixelWidth + 0.5f);
				num2 = (int)((float)cam.pixelHeight + 0.5f);
			}
			if ((Object)(object)maskTexture == (Object)null || width != num || height != num2)
			{
				width = num;
				height = num2;
				Cleanup();
				maskTexture = new RenderTexture(width, height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)1)
				{
					hideFlags = (HideFlags)61,
					name = "MaskTexture"
				};
				maskTexture.antiAliasing = ((QualitySettings.antiAliasing <= 0) ? 1 : QualitySettings.antiAliasing);
				maskTexture.autoGenerateMips = false;
				maskTexture.Create();
			}
			if ((Object)(object)amplifyColor != (Object)null && (Object)(object)((AmplifyColorBase)amplifyColor).MaskTexture != (Object)(object)maskTexture)
			{
				((AmplifyColorBase)amplifyColor).MaskTexture = (Texture)(object)maskTexture;
			}
		}
	}

	private void UpdateCameraProperties()
	{
		if (!((Object)(object)cam == (Object)null))
		{
			cam.targetTexture = maskTexture;
		}
	}

	private void Cleanup()
	{
		if ((Object)(object)maskTexture != (Object)null)
		{
			if ((Object)(object)cam != (Object)null && (Object)(object)cam.targetTexture == (Object)(object)maskTexture)
			{
				cam.targetTexture = null;
			}
			Object.DestroyImmediate((Object)(object)maskTexture);
		}
	}

	private void EnableMaskableMaterials(bool enableMasking)
	{
		Shader.SetGlobalFloat("_UseMaskingColor", (float)(enableMasking ? 1 : 0));
		foreach (MaskableBehaviour maskableObject in MaskableObjects)
		{
			maskableObject.SwitchMaterial(enableMasking);
		}
	}

	private void OnPreRender()
	{
		if (!((Object)(object)maskTexture == (Object)null))
		{
			activeRTBackup = RenderTexture.active;
			RenderTexture.active = maskTexture;
			EnableMaskableMaterials(enableMasking: true);
		}
	}

	private void OnPostRender()
	{
		if (!((Object)(object)maskTexture == (Object)null))
		{
			EnableMaskableMaterials(enableMasking: false);
			RenderTexture.active = activeRTBackup;
		}
	}
}
