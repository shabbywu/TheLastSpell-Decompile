using UnityEngine;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(Camera))]
public class RippleEffect : MonoBehaviour
{
	public class Constants
	{
		public const string ReflectionShaderParameter = "_Reflection";

		public const string Param1ShaderParameter = "_Params1";

		public const string Param2ShaderParameter = "_Params2";
	}

	private class Droplet
	{
		private Vector2 position;

		private float time;

		private bool timeScaleDependent;

		public Droplet()
		{
			time = 1000f;
		}

		public Droplet(bool timeScaleDependent)
		{
			time = 1000f;
			this.timeScaleDependent = timeScaleDependent;
		}

		public void Reset(float screenX, float screenY)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			position = new Vector2(screenX, screenY);
			time = 0f;
		}

		public void Update()
		{
			time += (timeScaleDependent ? Time.deltaTime : Time.unscaledDeltaTime);
		}

		public Vector4 MakeShaderParameter(float aspect)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			return new Vector4(position.x * aspect, position.y, time, 0f);
		}
	}

	[SerializeField]
	private Shader shader;

	[SerializeField]
	private AnimationCurve waveform = new AnimationCurve((Keyframe[])(object)new Keyframe[11]
	{
		new Keyframe(0f, 0.5f, 0f, 0f),
		new Keyframe(0.05f, 1f, 0f, 0f),
		new Keyframe(0.15f, 0.1f, 0f, 0f),
		new Keyframe(0.25f, 0.8f, 0f, 0f),
		new Keyframe(0.35f, 0.3f, 0f, 0f),
		new Keyframe(0.45f, 0.6f, 0f, 0f),
		new Keyframe(0.55f, 0.4f, 0f, 0f),
		new Keyframe(0.65f, 0.55f, 0f, 0f),
		new Keyframe(0.75f, 0.46f, 0f, 0f),
		new Keyframe(0.85f, 0.52f, 0f, 0f),
		new Keyframe(0.99f, 0.5f, 0f, 0f)
	});

	[SerializeField]
	[Range(0.01f, 1f)]
	private float refractionStrength = 0.5f;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float reflectionStrength = 0.7f;

	[SerializeField]
	[Range(1f, 3f)]
	private float waveSpeed = 1.25f;

	[SerializeField]
	private Color reflectionColor = Color.gray;

	[SerializeField]
	private bool timeScaleDependent;

	private Camera mainCamera;

	private Droplet[] droplets;

	private Texture2D gradTexture;

	private Material material;

	private int nextDropletIndex;

	public int NextDropletIndex
	{
		get
		{
			return nextDropletIndex;
		}
		set
		{
			nextDropletIndex = ++nextDropletIndex % droplets.Length;
		}
	}

	public void RippleAtWorldPosition(Vector3 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = mainCamera.WorldToViewportPoint(pos);
		Emit(val.x, val.y);
	}

	public void RippleAtWorldPosition(float x, float y)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = mainCamera.WorldToViewportPoint(new Vector3(x, y));
		Emit(val.x, val.y);
	}

	private void Awake()
	{
		Initialize();
	}

	[ContextMenu("Preview Ripple")]
	private void DebugPreview()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		RippleAtWorldPosition(((Component)Camera.main).transform.position.x, ((Component)Camera.main).transform.position.y);
	}

	private void Emit(float x, float y)
	{
		droplets[NextDropletIndex++].Reset(x, y);
	}

	private void Initialize()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		mainCamera = ((Component)this).GetComponent<Camera>();
		droplets = new Droplet[5]
		{
			new Droplet(timeScaleDependent),
			new Droplet(timeScaleDependent),
			new Droplet(timeScaleDependent),
			new Droplet(timeScaleDependent),
			new Droplet(timeScaleDependent)
		};
		gradTexture = new Texture2D(2048, 1, (TextureFormat)1, false)
		{
			wrapMode = (TextureWrapMode)1,
			filterMode = (FilterMode)1
		};
		for (int i = 0; i < ((Texture)gradTexture).width; i++)
		{
			float num = waveform.Evaluate(1f / (float)((Texture)gradTexture).width * (float)i);
			gradTexture.SetPixel(i, 0, new Color(num, num, num, num));
		}
		gradTexture.Apply();
		material = new Material(shader)
		{
			hideFlags = (HideFlags)52
		};
		material.SetTexture("_GradTex", (Texture)(object)gradTexture);
		UpdateShaderParameters();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit((Texture)(object)source, destination, material);
	}

	private void Update()
	{
		for (int num = droplets.Length - 1; num >= 0; num--)
		{
			droplets[num].Update();
		}
		UpdateShaderParameters();
	}

	private void UpdateShaderParameters()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < droplets.Length; i++)
		{
			material.SetVector($"_Drop{i + 1}", droplets[i].MakeShaderParameter(mainCamera.aspect));
		}
		material.SetColor("_Reflection", reflectionColor);
		material.SetVector("_Params1", new Vector4(mainCamera.aspect, 1f, 1f / waveSpeed, 0f));
		material.SetVector("_Params2", new Vector4(1f, 1f / mainCamera.aspect, refractionStrength, reflectionStrength));
	}
}
