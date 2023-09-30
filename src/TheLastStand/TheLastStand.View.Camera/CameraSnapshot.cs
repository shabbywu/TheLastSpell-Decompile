using TPLib;
using UnityEngine;
using UnityEngine.U2D;

namespace TheLastStand.View.Camera;

[RequireComponent(typeof(Camera))]
public class CameraSnapshot : TPSingleton<CameraSnapshot>
{
	public static class Constants
	{
		public static readonly int SnaptshotLayer;

		public static readonly Vector3 OffsetFromTarget;

		private const string SnaptshotLayerName = "Unit Snapshot";

		static Constants()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			OffsetFromTarget = new Vector3(0f, 0f, -1f);
			SnaptshotLayer = LayerMask.NameToLayer("Unit Snapshot");
		}
	}

	[SerializeField]
	private Camera cam;

	[SerializeField]
	private Vector2Int renderTextureDimensions = new Vector2Int(128, 128);

	private Transform parentBackup;

	[SerializeField]
	[HideInInspector]
	private PixelPerfectCamera pixelPerfectCamera;

	private RenderTexture renderTexture;

	public Sprite TakeSnapshot(ISnapshotable target)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		if (target == null)
		{
			TPDebug.LogError((object)"TakeSnapshot() can't work with a null target. Aborting...", (Object)(object)this);
			return null;
		}
		target.PrepareForSnapshot();
		parentBackup = ((Component)this).transform.parent;
		((Component)this).transform.SetParent(target.SnapshotPosition, false);
		((Component)this).transform.localPosition = Constants.OffsetFromTarget;
		((Behaviour)cam).enabled = true;
		cam.Render();
		((Behaviour)cam).enabled = false;
		((Component)this).transform.SetParent(parentBackup, false);
		target.OnSnapshotFinished();
		Texture2D val = new Texture2D(((Texture)renderTexture).width, ((Texture)renderTexture).height, (TextureFormat)4, false)
		{
			filterMode = (FilterMode)0,
			anisoLevel = 0
		};
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture;
		Rect val2 = default(Rect);
		((Rect)(ref val2))._002Ector(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height);
		val.ReadPixels(val2, 0, 0);
		val.Apply();
		RenderTexture.active = active;
		return Sprite.Create(val, val2, new Vector2(0.5f, 0.5f), 100f);
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void CreateRenderTexture()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		int num = (((Object)(object)pixelPerfectCamera != (Object)null) ? pixelPerfectCamera.refResolutionX : ((Vector2Int)(ref renderTextureDimensions)).x);
		int num2 = (((Object)(object)pixelPerfectCamera != (Object)null) ? pixelPerfectCamera.refResolutionY : ((Vector2Int)(ref renderTextureDimensions)).y);
		renderTexture = new RenderTexture(num, num2, 0, (RenderTextureFormat)0)
		{
			filterMode = (FilterMode)0,
			autoGenerateMips = false,
			useMipMap = false,
			anisoLevel = 0
		};
		cam.targetTexture = renderTexture;
	}

	private void Init()
	{
		if ((Object)(object)cam == (Object)null)
		{
			cam = ((Component)this).GetComponent<Camera>();
		}
		CreateRenderTexture();
	}
}
