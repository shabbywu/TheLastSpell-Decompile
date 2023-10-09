using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

[AddComponentMenu("UI/BetterImage", 11)]
public class BetterImage : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
{
	public enum Type
	{
		Simple,
		Sliced,
		Tiled,
		Filled
	}

	public enum FillMethod
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360
	}

	public enum OriginHorizontal
	{
		Left,
		Right
	}

	public enum OriginVertical
	{
		Bottom,
		Top
	}

	public enum Origin90
	{
		BottomLeft,
		TopLeft,
		TopRight,
		BottomRight
	}

	public enum Origin180
	{
		Bottom,
		Left,
		Top,
		Right
	}

	public enum Origin360
	{
		Bottom,
		Right,
		Top,
		Left
	}

	protected static Material s_ETC1DefaultUI = null;

	[FormerlySerializedAs("m_Frame")]
	[SerializeField]
	private Sprite m_Sprite;

	[NonSerialized]
	private Sprite m_OverrideSprite;

	[SerializeField]
	private Type m_Type;

	[SerializeField]
	private bool m_PreserveAspect;

	[SerializeField]
	private bool m_FillCenter = true;

	[SerializeField]
	private FillMethod m_FillMethod = FillMethod.Radial360;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_FillAmount = 1f;

	[SerializeField]
	private Vector2 m_FillAmountBoundaries = new Vector2(0f, 1f);

	[SerializeField]
	private bool m_FillClockwise = true;

	[SerializeField]
	private int m_FillOrigin;

	private float m_AlphaHitTestMinimumThreshold;

	private bool m_Tracked;

	[SerializeField]
	private bool m_UseSpriteMesh;

	private static readonly Vector2[] s_VertScratch = (Vector2[])(object)new Vector2[4];

	private static readonly Vector2[] s_UVScratch = (Vector2[])(object)new Vector2[4];

	private static readonly Vector3[] s_Xy = (Vector3[])(object)new Vector3[4];

	private static readonly Vector3[] s_Uv = (Vector3[])(object)new Vector3[4];

	private static List<BetterImage> m_TrackedTexturelessImages = new List<BetterImage>();

	private static bool s_Initialized;

	public Sprite sprite
	{
		get
		{
			return m_Sprite;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_Sprite, value))
			{
				((Graphic)this).SetAllDirty();
				TrackSprite();
			}
		}
	}

	public Sprite overrideSprite
	{
		get
		{
			return activeSprite;
		}
		set
		{
			if (SetPropertyUtility.SetClass(ref m_OverrideSprite, value))
			{
				((Graphic)this).SetAllDirty();
				TrackSprite();
			}
		}
	}

	private Sprite activeSprite
	{
		get
		{
			if (!((Object)(object)m_OverrideSprite != (Object)null))
			{
				return sprite;
			}
			return m_OverrideSprite;
		}
	}

	public Type type
	{
		get
		{
			return m_Type;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_Type, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public bool preserveAspect
	{
		get
		{
			return m_PreserveAspect;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_PreserveAspect, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public bool fillCenter
	{
		get
		{
			return m_FillCenter;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillCenter, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public FillMethod fillMethod
	{
		get
		{
			return m_FillMethod;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillMethod, value))
			{
				((Graphic)this).SetVerticesDirty();
				m_FillOrigin = 0;
			}
		}
	}

	public float fillAmount
	{
		get
		{
			return m_FillAmount;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillAmount, Mathf.Clamp01(value)))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public Vector2 fillAmountBoundaries
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_FillAmountBoundaries;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if (SetPropertyUtility.SetStruct<Vector2>(ref m_FillAmountBoundaries, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public bool fillClockwise
	{
		get
		{
			return m_FillClockwise;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillClockwise, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public int fillOrigin
	{
		get
		{
			return m_FillOrigin;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_FillOrigin, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	[Obsolete("eventAlphaThreshold has been deprecated. Use eventMinimumAlphaThreshold instead (UnityUpgradable) -> alphaHitTestMinimumThreshold")]
	public float eventAlphaThreshold
	{
		get
		{
			return 1f - alphaHitTestMinimumThreshold;
		}
		set
		{
			alphaHitTestMinimumThreshold = 1f - value;
		}
	}

	public float alphaHitTestMinimumThreshold
	{
		get
		{
			return m_AlphaHitTestMinimumThreshold;
		}
		set
		{
			m_AlphaHitTestMinimumThreshold = value;
		}
	}

	public bool useSpriteMesh
	{
		get
		{
			return m_UseSpriteMesh;
		}
		set
		{
			if (SetPropertyUtility.SetStruct(ref m_UseSpriteMesh, value))
			{
				((Graphic)this).SetVerticesDirty();
			}
		}
	}

	public static Material defaultETC1GraphicMaterial
	{
		get
		{
			if ((Object)(object)s_ETC1DefaultUI == (Object)null)
			{
				s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
			}
			return s_ETC1DefaultUI;
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if ((Object)(object)activeSprite == (Object)null)
			{
				if ((Object)(object)((Graphic)this).material != (Object)null && (Object)(object)((Graphic)this).material.mainTexture != (Object)null)
				{
					return ((Graphic)this).material.mainTexture;
				}
				return (Texture)(object)Graphic.s_WhiteTexture;
			}
			return (Texture)(object)activeSprite.texture;
		}
	}

	public bool hasBorder
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)activeSprite != (Object)null)
			{
				Vector4 border = activeSprite.border;
				return ((Vector4)(ref border)).sqrMagnitude > 0f;
			}
			return false;
		}
	}

	public float pixelsPerUnit
	{
		get
		{
			float num = 100f;
			if (Object.op_Implicit((Object)(object)activeSprite))
			{
				num = activeSprite.pixelsPerUnit;
			}
			float num2 = 100f;
			if (Object.op_Implicit((Object)(object)((Graphic)this).canvas))
			{
				num2 = ((Graphic)this).canvas.referencePixelsPerUnit;
			}
			return num / num2;
		}
	}

	public override Material material
	{
		get
		{
			if ((Object)(object)((Graphic)this).m_Material != (Object)null)
			{
				return ((Graphic)this).m_Material;
			}
			if (Object.op_Implicit((Object)(object)activeSprite) && (Object)(object)activeSprite.associatedAlphaSplitTexture != (Object)null)
			{
				return defaultETC1GraphicMaterial;
			}
			return ((Graphic)this).defaultMaterial;
		}
		set
		{
			((Graphic)this).material = value;
		}
	}

	public virtual float minWidth => 0f;

	public virtual float preferredWidth
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)activeSprite == (Object)null)
			{
				return 0f;
			}
			if (type == Type.Sliced || type == Type.Tiled)
			{
				return DataUtility.GetMinSize(activeSprite).x / pixelsPerUnit;
			}
			Rect rect = activeSprite.rect;
			return ((Rect)(ref rect)).size.x / pixelsPerUnit;
		}
	}

	public virtual float flexibleWidth => -1f;

	public virtual float minHeight => 0f;

	public virtual float preferredHeight
	{
		get
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)activeSprite == (Object)null)
			{
				return 0f;
			}
			if (type == Type.Sliced || type == Type.Tiled)
			{
				return DataUtility.GetMinSize(activeSprite).y / pixelsPerUnit;
			}
			Rect rect = activeSprite.rect;
			return ((Rect)(ref rect)).size.y / pixelsPerUnit;
		}
	}

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => 0;

	protected BetterImage()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)this).useLegacyMeshGeneration = false;
	}

	public virtual void OnBeforeSerialize()
	{
	}

	public virtual void OnAfterDeserialize()
	{
		if (m_FillOrigin < 0)
		{
			m_FillOrigin = 0;
		}
		else if (m_FillMethod == FillMethod.Horizontal && m_FillOrigin > 1)
		{
			m_FillOrigin = 0;
		}
		else if (m_FillMethod == FillMethod.Vertical && m_FillOrigin > 1)
		{
			m_FillOrigin = 0;
		}
		else if (m_FillOrigin > 3)
		{
			m_FillOrigin = 0;
		}
		m_FillAmount = Mathf.Clamp(m_FillAmount, 0f, 1f);
	}

	private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float num = spriteSize.x / spriteSize.y;
		float num2 = ((Rect)(ref rect)).width / ((Rect)(ref rect)).height;
		if (num > num2)
		{
			float height = ((Rect)(ref rect)).height;
			((Rect)(ref rect)).height = ((Rect)(ref rect)).width * (1f / num);
			((Rect)(ref rect)).y = ((Rect)(ref rect)).y + (height - ((Rect)(ref rect)).height) * ((Graphic)this).rectTransform.pivot.y;
		}
		else
		{
			float width = ((Rect)(ref rect)).width;
			((Rect)(ref rect)).width = ((Rect)(ref rect)).height * num;
			((Rect)(ref rect)).x = ((Rect)(ref rect)).x + (width - ((Rect)(ref rect)).width) * ((Graphic)this).rectTransform.pivot.x;
		}
	}

	private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = (((Object)(object)activeSprite == (Object)null) ? Vector4.zero : DataUtility.GetPadding(activeSprite));
		_003F val2;
		if (!((Object)(object)activeSprite == (Object)null))
		{
			Rect rect = activeSprite.rect;
			float width = ((Rect)(ref rect)).width;
			rect = activeSprite.rect;
			val2 = new Vector2(width, ((Rect)(ref rect)).height);
		}
		else
		{
			val2 = Vector2.zero;
		}
		Vector2 val3 = (Vector2)val2;
		Rect rect2 = ((Graphic)this).GetPixelAdjustedRect();
		int num = Mathf.RoundToInt(val3.x);
		int num2 = Mathf.RoundToInt(val3.y);
		Vector4 val4 = default(Vector4);
		((Vector4)(ref val4))._002Ector(val.x / (float)num, val.y / (float)num2, ((float)num - val.z) / (float)num, ((float)num2 - val.w) / (float)num2);
		if (shouldPreserveAspect && ((Vector2)(ref val3)).sqrMagnitude > 0f)
		{
			PreserveSpriteAspectRatio(ref rect2, val3);
		}
		((Vector4)(ref val4))._002Ector(((Rect)(ref rect2)).x + ((Rect)(ref rect2)).width * val4.x, ((Rect)(ref rect2)).y + ((Rect)(ref rect2)).height * val4.y, ((Rect)(ref rect2)).x + ((Rect)(ref rect2)).width * val4.z, ((Rect)(ref rect2)).y + ((Rect)(ref rect2)).height * val4.w);
		return val4;
	}

	public override void SetNativeSize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)activeSprite != (Object)null)
		{
			Rect rect = activeSprite.rect;
			float num = ((Rect)(ref rect)).width / pixelsPerUnit;
			rect = activeSprite.rect;
			float num2 = ((Rect)(ref rect)).height / pixelsPerUnit;
			((Graphic)this).rectTransform.anchorMax = ((Graphic)this).rectTransform.anchorMin;
			((Graphic)this).rectTransform.sizeDelta = new Vector2(num, num2);
			((Graphic)this).SetAllDirty();
		}
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		if ((Object)(object)activeSprite == (Object)null)
		{
			((Graphic)this).OnPopulateMesh(toFill);
			return;
		}
		switch (type)
		{
		case Type.Simple:
			if (!useSpriteMesh)
			{
				GenerateSimpleSprite(toFill, m_PreserveAspect);
			}
			else
			{
				GenerateSprite(toFill, m_PreserveAspect);
			}
			break;
		case Type.Sliced:
			GenerateSlicedSprite(toFill);
			break;
		case Type.Tiled:
			GenerateTiledSprite(toFill);
			break;
		case Type.Filled:
			GenerateFilledSprite(toFill, m_PreserveAspect);
			break;
		}
	}

	private void TrackSprite()
	{
		if ((Object)(object)activeSprite != (Object)null && (Object)(object)activeSprite.texture == (Object)null)
		{
			TrackImage(this);
			m_Tracked = true;
		}
	}

	protected override void OnEnable()
	{
		((MaskableGraphic)this).OnEnable();
		TrackSprite();
	}

	protected override void OnDisable()
	{
		((MaskableGraphic)this).OnDisable();
		if (m_Tracked)
		{
			UnTrackImage(this);
		}
	}

	protected override void UpdateMaterial()
	{
		((Graphic)this).UpdateMaterial();
		if ((Object)(object)activeSprite == (Object)null)
		{
			((Graphic)this).canvasRenderer.SetAlphaTexture((Texture)null);
			return;
		}
		Texture2D associatedAlphaSplitTexture = activeSprite.associatedAlphaSplitTexture;
		if ((Object)(object)associatedAlphaSplitTexture != (Object)null)
		{
			((Graphic)this).canvasRenderer.SetAlphaTexture((Texture)(object)associatedAlphaSplitTexture);
		}
	}

	private void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		Vector4 drawingDimensions = GetDrawingDimensions(lPreserveAspect);
		Vector4 val = (((Object)(object)activeSprite != (Object)null) ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero);
		Color color = ((Graphic)this).color;
		vh.Clear();
		vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.y), Color32.op_Implicit(color), Vector4.op_Implicit(new Vector2(val.x, val.y)));
		vh.AddVert(new Vector3(drawingDimensions.x, drawingDimensions.w), Color32.op_Implicit(color), Vector4.op_Implicit(new Vector2(val.x, val.w)));
		vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.w), Color32.op_Implicit(color), Vector4.op_Implicit(new Vector2(val.z, val.w)));
		vh.AddVert(new Vector3(drawingDimensions.z, drawingDimensions.y), Color32.op_Implicit(color), Vector4.op_Implicit(new Vector2(val.z, val.y)));
		vh.AddTriangle(0, 1, 2);
		vh.AddTriangle(2, 3, 0);
	}

	private void GenerateSprite(VertexHelper vh, bool lPreserveAspect)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = activeSprite.rect;
		float width = ((Rect)(ref rect)).width;
		rect = activeSprite.rect;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(width, ((Rect)(ref rect)).height);
		Vector2 val2 = activeSprite.pivot / val;
		Vector2 pivot = ((Graphic)this).rectTransform.pivot;
		Rect rect2 = ((Graphic)this).GetPixelAdjustedRect();
		if (lPreserveAspect & (((Vector2)(ref val)).sqrMagnitude > 0f))
		{
			PreserveSpriteAspectRatio(ref rect2, val);
		}
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))._002Ector(((Rect)(ref rect2)).width, ((Rect)(ref rect2)).height);
		Bounds bounds = activeSprite.bounds;
		Vector3 size = ((Bounds)(ref bounds)).size;
		Vector2 val4 = (pivot - val2) * val3;
		Color color = ((Graphic)this).color;
		vh.Clear();
		Vector2[] vertices = activeSprite.vertices;
		Vector2[] uv = activeSprite.uv;
		for (int i = 0; i < vertices.Length; i++)
		{
			vh.AddVert(new Vector3(vertices[i].x / size.x * val3.x - val4.x, vertices[i].y / size.y * val3.y - val4.y), Color32.op_Implicit(color), Vector4.op_Implicit(new Vector2(uv[i].x, uv[i].y)));
		}
		ushort[] triangles = activeSprite.triangles;
		for (int j = 0; j < triangles.Length; j += 3)
		{
			vh.AddTriangle((int)triangles[j], (int)triangles[j + 1], (int)triangles[j + 2]);
		}
	}

	private void GenerateSlicedSprite(VertexHelper toFill)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		if (!hasBorder)
		{
			GenerateSimpleSprite(toFill, lPreserveAspect: false);
			return;
		}
		Vector4 val;
		Vector4 val2;
		Vector4 val4;
		Vector4 val3;
		if ((Object)(object)activeSprite != (Object)null)
		{
			val = DataUtility.GetOuterUV(activeSprite);
			val2 = DataUtility.GetInnerUV(activeSprite);
			val3 = DataUtility.GetPadding(activeSprite);
			val4 = activeSprite.border;
		}
		else
		{
			val = Vector4.zero;
			val2 = Vector4.zero;
			val3 = Vector4.zero;
			val4 = Vector4.zero;
		}
		Rect pixelAdjustedRect = ((Graphic)this).GetPixelAdjustedRect();
		Vector4 adjustedBorders = GetAdjustedBorders(val4 / pixelsPerUnit, pixelAdjustedRect);
		val3 /= pixelsPerUnit;
		s_VertScratch[0] = new Vector2(val3.x, val3.y);
		s_VertScratch[3] = new Vector2(((Rect)(ref pixelAdjustedRect)).width - val3.z, ((Rect)(ref pixelAdjustedRect)).height - val3.w);
		s_VertScratch[1].x = adjustedBorders.x;
		s_VertScratch[1].y = adjustedBorders.y;
		s_VertScratch[2].x = ((Rect)(ref pixelAdjustedRect)).width - adjustedBorders.z;
		s_VertScratch[2].y = ((Rect)(ref pixelAdjustedRect)).height - adjustedBorders.w;
		for (int i = 0; i < 4; i++)
		{
			s_VertScratch[i].x += ((Rect)(ref pixelAdjustedRect)).x;
			s_VertScratch[i].y += ((Rect)(ref pixelAdjustedRect)).y;
		}
		s_UVScratch[0] = new Vector2(val.x, val.y);
		s_UVScratch[1] = new Vector2(val2.x, val2.y);
		s_UVScratch[2] = new Vector2(val2.z, val2.w);
		s_UVScratch[3] = new Vector2(val.z, val.w);
		toFill.Clear();
		for (int j = 0; j < 3; j++)
		{
			int num = j + 1;
			for (int k = 0; k < 3; k++)
			{
				if (m_FillCenter || j != 1 || k != 1)
				{
					int num2 = k + 1;
					AddQuad(toFill, new Vector2(s_VertScratch[j].x, s_VertScratch[k].y), new Vector2(s_VertScratch[num].x, s_VertScratch[num2].y), Color32.op_Implicit(((Graphic)this).color), new Vector2(s_UVScratch[j].x, s_UVScratch[k].y), new Vector2(s_UVScratch[num].x, s_UVScratch[num2].y));
				}
			}
		}
	}

	private void GenerateTiledSprite(VertexHelper toFill)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_0907: Unknown result type (might be due to invalid IL or missing references)
		//IL_090c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0915: Unknown result type (might be due to invalid IL or missing references)
		//IL_091c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0921: Unknown result type (might be due to invalid IL or missing references)
		//IL_0927: Unknown result type (might be due to invalid IL or missing references)
		//IL_092c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0931: Unknown result type (might be due to invalid IL or missing references)
		//IL_0933: Unknown result type (might be due to invalid IL or missing references)
		//IL_0935: Unknown result type (might be due to invalid IL or missing references)
		//IL_093a: Unknown result type (might be due to invalid IL or missing references)
		//IL_093c: Unknown result type (might be due to invalid IL or missing references)
		//IL_093e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0751: Unknown result type (might be due to invalid IL or missing references)
		//IL_0756: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0766: Unknown result type (might be due to invalid IL or missing references)
		//IL_076b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Unknown result type (might be due to invalid IL or missing references)
		//IL_0776: Unknown result type (might be due to invalid IL or missing references)
		//IL_077b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0781: Unknown result type (might be due to invalid IL or missing references)
		//IL_0787: Unknown result type (might be due to invalid IL or missing references)
		//IL_078c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_079a: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0801: Unknown result type (might be due to invalid IL or missing references)
		//IL_0813: Unknown result type (might be due to invalid IL or missing references)
		//IL_081a: Unknown result type (might be due to invalid IL or missing references)
		//IL_081f: Unknown result type (might be due to invalid IL or missing references)
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0834: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_083f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0844: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_084f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0862: Unknown result type (might be due to invalid IL or missing references)
		//IL_0868: Unknown result type (might be due to invalid IL or missing references)
		//IL_0877: Unknown result type (might be due to invalid IL or missing references)
		//IL_087e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_0896: Unknown result type (might be due to invalid IL or missing references)
		//IL_089d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_068b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0700: Unknown result type (might be due to invalid IL or missing references)
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0713: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0725: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_0649: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val;
		Vector4 val2;
		Vector2 val4;
		Vector4 val3;
		if ((Object)(object)activeSprite != (Object)null)
		{
			val = DataUtility.GetOuterUV(activeSprite);
			val2 = DataUtility.GetInnerUV(activeSprite);
			val3 = activeSprite.border;
			Rect rect = activeSprite.rect;
			val4 = ((Rect)(ref rect)).size;
		}
		else
		{
			val = Vector4.zero;
			val2 = Vector4.zero;
			val3 = Vector4.zero;
			val4 = Vector2.one * 100f;
		}
		Rect pixelAdjustedRect = ((Graphic)this).GetPixelAdjustedRect();
		float num = (val4.x - val3.x - val3.z) / pixelsPerUnit;
		float num2 = (val4.y - val3.y - val3.w) / pixelsPerUnit;
		val3 = GetAdjustedBorders(val3 / pixelsPerUnit, pixelAdjustedRect);
		Vector2 val5 = default(Vector2);
		((Vector2)(ref val5))._002Ector(val2.x, val2.y);
		Vector2 val6 = default(Vector2);
		((Vector2)(ref val6))._002Ector(val2.z, val2.w);
		float x = val3.x;
		float num3 = ((Rect)(ref pixelAdjustedRect)).width - val3.z;
		float y = val3.y;
		float num4 = ((Rect)(ref pixelAdjustedRect)).height - val3.w;
		toFill.Clear();
		Vector2 uvMax = val6;
		if (num <= 0f)
		{
			num = num3 - x;
		}
		if (num2 <= 0f)
		{
			num2 = num4 - y;
		}
		if ((Object)(object)activeSprite != (Object)null && (hasBorder || activeSprite.packed || (int)((Texture)activeSprite.texture).wrapMode != 0))
		{
			long num5 = 0L;
			long num6 = 0L;
			if (m_FillCenter)
			{
				num5 = (long)Math.Ceiling((num3 - x) / num);
				num6 = (long)Math.Ceiling((num4 - y) / num2);
				double num7 = 0.0;
				num7 = ((!hasBorder) ? ((double)(num5 * num6) * 4.0) : (((double)num5 + 2.0) * ((double)num6 + 2.0) * 4.0));
				if (num7 > 65000.0)
				{
					Debug.LogError((object)("Too many sprite tiles on BetterImage \"" + ((Object)this).name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat."), (Object)(object)this);
					double num8 = ((!hasBorder) ? ((double)num5 / (double)num6) : (((double)num5 + 2.0) / ((double)num6 + 2.0)));
					double num9 = Math.Sqrt(16250.0 / num8);
					double num10 = num9 * num8;
					if (hasBorder)
					{
						num9 -= 2.0;
						num10 -= 2.0;
					}
					num5 = (long)Math.Floor(num9);
					num6 = (long)Math.Floor(num10);
					num = (num3 - x) / (float)num5;
					num2 = (num4 - y) / (float)num6;
				}
			}
			else if (hasBorder)
			{
				num5 = (long)Math.Ceiling((num3 - x) / num);
				num6 = (long)Math.Ceiling((num4 - y) / num2);
				if (((double)(num6 + num5) + 2.0) * 2.0 * 4.0 > 65000.0)
				{
					Debug.LogError((object)("Too many sprite tiles on BetterImage \"" + ((Object)this).name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat."), (Object)(object)this);
					double num11 = (double)num5 / (double)num6;
					double num12 = (16250.0 - 4.0) / (2.0 * (1.0 + num11));
					double d = num12 * num11;
					num5 = (long)Math.Floor(num12);
					num6 = (long)Math.Floor(d);
					num = (num3 - x) / (float)num5;
					num2 = (num4 - y) / (float)num6;
				}
			}
			else
			{
				num6 = (num5 = 0L);
			}
			if (m_FillCenter)
			{
				for (long num13 = 0L; num13 < num6; num13++)
				{
					float num14 = y + (float)num13 * num2;
					float num15 = y + (float)(num13 + 1) * num2;
					if (num15 > num4)
					{
						uvMax.y = val5.y + (val6.y - val5.y) * (num4 - num14) / (num15 - num14);
						num15 = num4;
					}
					uvMax.x = val6.x;
					for (long num16 = 0L; num16 < num5; num16++)
					{
						float num17 = x + (float)num16 * num;
						float num18 = x + (float)(num16 + 1) * num;
						if (num18 > num3)
						{
							uvMax.x = val5.x + (val6.x - val5.x) * (num3 - num17) / (num18 - num17);
							num18 = num3;
						}
						AddQuad(toFill, new Vector2(num17, num14) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(num18, num15) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), val5, uvMax);
					}
				}
			}
			if (!hasBorder)
			{
				return;
			}
			uvMax = val6;
			for (long num19 = 0L; num19 < num6; num19++)
			{
				float num20 = y + (float)num19 * num2;
				float num21 = y + (float)(num19 + 1) * num2;
				if (num21 > num4)
				{
					uvMax.y = val5.y + (val6.y - val5.y) * (num4 - num20) / (num21 - num20);
					num21 = num4;
				}
				AddQuad(toFill, new Vector2(0f, num20) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(x, num21) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val.x, val5.y), new Vector2(val5.x, uvMax.y));
				AddQuad(toFill, new Vector2(num3, num20) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(((Rect)(ref pixelAdjustedRect)).width, num21) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val6.x, val5.y), new Vector2(val.z, uvMax.y));
			}
			uvMax = val6;
			for (long num22 = 0L; num22 < num5; num22++)
			{
				float num23 = x + (float)num22 * num;
				float num24 = x + (float)(num22 + 1) * num;
				if (num24 > num3)
				{
					uvMax.x = val5.x + (val6.x - val5.x) * (num3 - num23) / (num24 - num23);
					num24 = num3;
				}
				AddQuad(toFill, new Vector2(num23, 0f) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(num24, y) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val5.x, val.y), new Vector2(uvMax.x, val5.y));
				AddQuad(toFill, new Vector2(num23, num4) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(num24, ((Rect)(ref pixelAdjustedRect)).height) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val5.x, val6.y), new Vector2(uvMax.x, val.w));
			}
			AddQuad(toFill, new Vector2(0f, 0f) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(x, y) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val.x, val.y), new Vector2(val5.x, val5.y));
			AddQuad(toFill, new Vector2(num3, 0f) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(((Rect)(ref pixelAdjustedRect)).width, y) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val6.x, val.y), new Vector2(val.z, val5.y));
			AddQuad(toFill, new Vector2(0f, num4) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(x, ((Rect)(ref pixelAdjustedRect)).height) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val.x, val6.y), new Vector2(val5.x, val.w));
			AddQuad(toFill, new Vector2(num3, num4) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(((Rect)(ref pixelAdjustedRect)).width, ((Rect)(ref pixelAdjustedRect)).height) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), new Vector2(val6.x, val6.y), new Vector2(val.z, val.w));
		}
		else
		{
			Vector2 val7 = default(Vector2);
			((Vector2)(ref val7))._002Ector((num3 - x) / num, (num4 - y) / num2);
			if (m_FillCenter)
			{
				AddQuad(toFill, new Vector2(x, y) + ((Rect)(ref pixelAdjustedRect)).position, new Vector2(num3, num4) + ((Rect)(ref pixelAdjustedRect)).position, Color32.op_Implicit(((Graphic)this).color), Vector2.Scale(val5, val7), Vector2.Scale(val6, val7));
			}
		}
	}

	private static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		int currentVertCount = vertexHelper.currentVertCount;
		for (int i = 0; i < 4; i++)
		{
			vertexHelper.AddVert(quadPositions[i], color, Vector4.op_Implicit(quadUVs[i]));
		}
		vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}

	private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		int currentVertCount = vertexHelper.currentVertCount;
		vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, Vector4.op_Implicit(new Vector2(uvMin.x, uvMin.y)));
		vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, Vector4.op_Implicit(new Vector2(uvMin.x, uvMax.y)));
		vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, Vector4.op_Implicit(new Vector2(uvMax.x, uvMax.y)));
		vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, Vector4.op_Implicit(new Vector2(uvMax.x, uvMin.y)));
		vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
		vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
	}

	private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = ((Graphic)this).rectTransform.rect;
		for (int i = 0; i <= 1; i++)
		{
			Vector2 size = ((Rect)(ref rect)).size;
			if (((Vector2)(ref size))[i] != 0f)
			{
				size = ((Rect)(ref adjustedRect)).size;
				float num = ((Vector2)(ref size))[i];
				size = ((Rect)(ref rect)).size;
				float num2 = num / ((Vector2)(ref size))[i];
				ref Vector4 reference = ref border;
				int num3 = i;
				((Vector4)(ref reference))[num3] = ((Vector4)(ref reference))[num3] * num2;
				reference = ref border;
				num3 = i + 2;
				((Vector4)(ref reference))[num3] = ((Vector4)(ref reference))[num3] * num2;
			}
			float num4 = ((Vector4)(ref border))[i] + ((Vector4)(ref border))[i + 2];
			size = ((Rect)(ref adjustedRect)).size;
			if (((Vector2)(ref size))[i] < num4 && num4 != 0f)
			{
				size = ((Rect)(ref adjustedRect)).size;
				float num2 = ((Vector2)(ref size))[i] / num4;
				ref Vector4 reference = ref border;
				int num3 = i;
				((Vector4)(ref reference))[num3] = ((Vector4)(ref reference))[num3] * num2;
				reference = ref border;
				num3 = i + 2;
				((Vector4)(ref reference))[num3] = ((Vector4)(ref reference))[num3] * num2;
			}
		}
		return border;
	}

	private void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_064d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0653: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		toFill.Clear();
		float num = ((m_FillAmount > 0.999f) ? m_FillAmount : TPHelpers.Remap(m_FillAmount, 0f, 1f, m_FillAmountBoundaries.x, m_FillAmountBoundaries.y));
		if (num < 0.001f)
		{
			return;
		}
		Vector4 drawingDimensions = GetDrawingDimensions(preserveAspect);
		Vector4 val = (((Object)(object)activeSprite != (Object)null) ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero);
		UIVertex simpleVert = UIVertex.simpleVert;
		simpleVert.color = Color32.op_Implicit(((Graphic)this).color);
		float num2 = val.x;
		float num3 = val.y;
		float num4 = val.z;
		float num5 = val.w;
		if (m_FillMethod == FillMethod.Horizontal || m_FillMethod == FillMethod.Vertical)
		{
			if (fillMethod == FillMethod.Horizontal)
			{
				float num6 = (num4 - num2) * num;
				if (m_FillOrigin == 1)
				{
					drawingDimensions.x = drawingDimensions.z - (drawingDimensions.z - drawingDimensions.x) * num;
					num2 = num4 - num6;
				}
				else
				{
					drawingDimensions.z = drawingDimensions.x + (drawingDimensions.z - drawingDimensions.x) * num;
					num4 = num2 + num6;
				}
			}
			else if (fillMethod == FillMethod.Vertical)
			{
				float num7 = (num5 - num3) * num;
				if (m_FillOrigin == 1)
				{
					drawingDimensions.y = drawingDimensions.w - (drawingDimensions.w - drawingDimensions.y) * num;
					num3 = num5 - num7;
				}
				else
				{
					drawingDimensions.w = drawingDimensions.y + (drawingDimensions.w - drawingDimensions.y) * num;
					num5 = num3 + num7;
				}
			}
		}
		s_Xy[0] = Vector2.op_Implicit(new Vector2(drawingDimensions.x, drawingDimensions.y));
		s_Xy[1] = Vector2.op_Implicit(new Vector2(drawingDimensions.x, drawingDimensions.w));
		s_Xy[2] = Vector2.op_Implicit(new Vector2(drawingDimensions.z, drawingDimensions.w));
		s_Xy[3] = Vector2.op_Implicit(new Vector2(drawingDimensions.z, drawingDimensions.y));
		s_Uv[0] = Vector2.op_Implicit(new Vector2(num2, num3));
		s_Uv[1] = Vector2.op_Implicit(new Vector2(num2, num5));
		s_Uv[2] = Vector2.op_Implicit(new Vector2(num4, num5));
		s_Uv[3] = Vector2.op_Implicit(new Vector2(num4, num3));
		if (num < 1f && m_FillMethod != 0 && m_FillMethod != FillMethod.Vertical)
		{
			if (fillMethod == FillMethod.Radial90)
			{
				if (RadialCut(s_Xy, s_Uv, num, m_FillClockwise, m_FillOrigin))
				{
					AddQuad(toFill, s_Xy, Color32.op_Implicit(((Graphic)this).color), s_Uv);
				}
			}
			else if (fillMethod == FillMethod.Radial180)
			{
				for (int i = 0; i < 2; i++)
				{
					int num8 = ((m_FillOrigin > 1) ? 1 : 0);
					float num9;
					float num10;
					float num11;
					float num12;
					if (m_FillOrigin == 0 || m_FillOrigin == 2)
					{
						num9 = 0f;
						num10 = 1f;
						if (i == num8)
						{
							num11 = 0f;
							num12 = 0.5f;
						}
						else
						{
							num11 = 0.5f;
							num12 = 1f;
						}
					}
					else
					{
						num11 = 0f;
						num12 = 1f;
						if (i == num8)
						{
							num9 = 0.5f;
							num10 = 1f;
						}
						else
						{
							num9 = 0f;
							num10 = 0.5f;
						}
					}
					s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, num11);
					s_Xy[1].x = s_Xy[0].x;
					s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, num12);
					s_Xy[3].x = s_Xy[2].x;
					s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, num9);
					s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, num10);
					s_Xy[2].y = s_Xy[1].y;
					s_Xy[3].y = s_Xy[0].y;
					s_Uv[0].x = Mathf.Lerp(num2, num4, num11);
					s_Uv[1].x = s_Uv[0].x;
					s_Uv[2].x = Mathf.Lerp(num2, num4, num12);
					s_Uv[3].x = s_Uv[2].x;
					s_Uv[0].y = Mathf.Lerp(num3, num5, num9);
					s_Uv[1].y = Mathf.Lerp(num3, num5, num10);
					s_Uv[2].y = s_Uv[1].y;
					s_Uv[3].y = s_Uv[0].y;
					float num13 = (m_FillClockwise ? (num * 2f - (float)i) : (num * 2f - (float)(1 - i)));
					if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(num13), m_FillClockwise, (i + m_FillOrigin + 3) % 4))
					{
						AddQuad(toFill, s_Xy, Color32.op_Implicit(((Graphic)this).color), s_Uv);
					}
				}
			}
			else
			{
				if (fillMethod != FillMethod.Radial360)
				{
					return;
				}
				for (int j = 0; j < 4; j++)
				{
					float num14;
					float num15;
					if (j < 2)
					{
						num14 = 0f;
						num15 = 0.5f;
					}
					else
					{
						num14 = 0.5f;
						num15 = 1f;
					}
					float num16;
					float num17;
					if (j == 0 || j == 3)
					{
						num16 = 0f;
						num17 = 0.5f;
					}
					else
					{
						num16 = 0.5f;
						num17 = 1f;
					}
					s_Xy[0].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, num14);
					s_Xy[1].x = s_Xy[0].x;
					s_Xy[2].x = Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, num15);
					s_Xy[3].x = s_Xy[2].x;
					s_Xy[0].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, num16);
					s_Xy[1].y = Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, num17);
					s_Xy[2].y = s_Xy[1].y;
					s_Xy[3].y = s_Xy[0].y;
					s_Uv[0].x = Mathf.Lerp(num2, num4, num14);
					s_Uv[1].x = s_Uv[0].x;
					s_Uv[2].x = Mathf.Lerp(num2, num4, num15);
					s_Uv[3].x = s_Uv[2].x;
					s_Uv[0].y = Mathf.Lerp(num3, num5, num16);
					s_Uv[1].y = Mathf.Lerp(num3, num5, num17);
					s_Uv[2].y = s_Uv[1].y;
					s_Uv[3].y = s_Uv[0].y;
					float num18 = (m_FillClockwise ? (m_FillAmount * 4f - (float)((j + m_FillOrigin) % 4)) : (m_FillAmount * 4f - (float)(3 - (j + m_FillOrigin) % 4)));
					if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(num18), m_FillClockwise, (j + 2) % 4))
					{
						AddQuad(toFill, s_Xy, Color32.op_Implicit(((Graphic)this).color), s_Uv);
					}
				}
			}
		}
		else
		{
			AddQuad(toFill, s_Xy, Color32.op_Implicit(((Graphic)this).color), s_Uv);
		}
	}

	private static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
	{
		if (fill < 0.001f)
		{
			return false;
		}
		if ((corner & 1) == 1)
		{
			invert = !invert;
		}
		if (!invert && fill > 0.999f)
		{
			return true;
		}
		float num = Mathf.Clamp01(fill);
		if (invert)
		{
			num = 1f - num;
		}
		num *= (float)Math.PI / 2f;
		float cos = Mathf.Cos(num);
		float sin = Mathf.Sin(num);
		RadialCut(xy, cos, sin, invert, corner);
		RadialCut(uv, cos, sin, invert, corner);
		return true;
	}

	private static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
	{
		int num = (corner + 1) % 4;
		int num2 = (corner + 2) % 4;
		int num3 = (corner + 3) % 4;
		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
					xy[num2].x = xy[num].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num2].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
					xy[num3].y = xy[num2].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (!invert)
			{
				xy[num3].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
			}
			else
			{
				xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
			}
			return;
		}
		if (cos > sin)
		{
			sin /= cos;
			cos = 1f;
			if (!invert)
			{
				xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
				xy[num2].y = xy[num].y;
			}
		}
		else if (sin > cos)
		{
			cos /= sin;
			sin = 1f;
			if (invert)
			{
				xy[num2].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
				xy[num3].x = xy[num2].x;
			}
		}
		else
		{
			cos = 1f;
			sin = 1f;
		}
		if (invert)
		{
			xy[num3].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
		}
		else
		{
			xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
		}
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		//IL_014a: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		if (alphaHitTestMinimumThreshold <= 0f)
		{
			return true;
		}
		if (alphaHitTestMinimumThreshold > 1f)
		{
			return false;
		}
		if ((Object)(object)activeSprite == (Object)null)
		{
			return true;
		}
		Vector2 local = default(Vector2);
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(((Graphic)this).rectTransform, screenPoint, eventCamera, ref local))
		{
			return false;
		}
		Rect pixelAdjustedRect = ((Graphic)this).GetPixelAdjustedRect();
		local.x += ((Graphic)this).rectTransform.pivot.x * ((Rect)(ref pixelAdjustedRect)).width;
		local.y += ((Graphic)this).rectTransform.pivot.y * ((Rect)(ref pixelAdjustedRect)).height;
		local = MapCoordinate(local, pixelAdjustedRect);
		Rect textureRect = activeSprite.textureRect;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(local.x / ((Rect)(ref textureRect)).width, local.y / ((Rect)(ref textureRect)).height);
		float num = Mathf.Lerp(((Rect)(ref textureRect)).x, ((Rect)(ref textureRect)).xMax, val.x) / (float)((Texture)activeSprite.texture).width;
		float num2 = Mathf.Lerp(((Rect)(ref textureRect)).y, ((Rect)(ref textureRect)).yMax, val.y) / (float)((Texture)activeSprite.texture).height;
		try
		{
			return activeSprite.texture.GetPixelBilinear(num, num2).a >= alphaHitTestMinimumThreshold;
		}
		catch (UnityException val2)
		{
			UnityException val3 = val2;
			Debug.LogError((object)("Using alphaHitTestMinimumThreshold greater than 0 on BetterImage whose sprite texture cannot be read. " + ((Exception)(object)val3).Message + " Also make sure to disable sprite packing for this sprite."), (Object)(object)this);
			return true;
		}
	}

	private Vector2 MapCoordinate(Vector2 local, Rect rect)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		Rect rect2 = activeSprite.rect;
		if (type == Type.Simple || type == Type.Filled)
		{
			return new Vector2(local.x * ((Rect)(ref rect2)).width / ((Rect)(ref rect)).width, local.y * ((Rect)(ref rect2)).height / ((Rect)(ref rect)).height);
		}
		Vector4 border = activeSprite.border;
		Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
		for (int i = 0; i < 2; i++)
		{
			if (!(((Vector2)(ref local))[i] <= ((Vector4)(ref adjustedBorders))[i]))
			{
				Vector2 size = ((Rect)(ref rect)).size;
				if (((Vector2)(ref size))[i] - ((Vector2)(ref local))[i] <= ((Vector4)(ref adjustedBorders))[i + 2])
				{
					ref Vector2 reference = ref local;
					int num = i;
					ref Vector2 reference2 = ref reference;
					int num2 = num;
					float num3 = ((Vector2)(ref reference))[num];
					size = ((Rect)(ref rect)).size;
					float num4 = ((Vector2)(ref size))[i];
					size = ((Rect)(ref rect2)).size;
					((Vector2)(ref reference2))[num2] = num3 - (num4 - ((Vector2)(ref size))[i]);
				}
				else if (type == Type.Sliced)
				{
					float num5 = ((Vector4)(ref adjustedBorders))[i];
					size = ((Rect)(ref rect)).size;
					float num6 = Mathf.InverseLerp(num5, ((Vector2)(ref size))[i] - ((Vector4)(ref adjustedBorders))[i + 2], ((Vector2)(ref local))[i]);
					int num7 = i;
					float num8 = ((Vector4)(ref border))[i];
					size = ((Rect)(ref rect2)).size;
					((Vector2)(ref local))[num7] = Mathf.Lerp(num8, ((Vector2)(ref size))[i] - ((Vector4)(ref border))[i + 2], num6);
				}
				else
				{
					ref Vector2 reference = ref local;
					int num = i;
					((Vector2)(ref reference))[num] = ((Vector2)(ref reference))[num] - ((Vector4)(ref adjustedBorders))[i];
					int num9 = i;
					float num10 = ((Vector2)(ref local))[i];
					size = ((Rect)(ref rect2)).size;
					((Vector2)(ref local))[num9] = Mathf.Repeat(num10, ((Vector2)(ref size))[i] - ((Vector4)(ref border))[i] - ((Vector4)(ref border))[i + 2]);
					reference = ref local;
					num = i;
					((Vector2)(ref reference))[num] = ((Vector2)(ref reference))[num] + ((Vector4)(ref border))[i];
				}
			}
		}
		return local;
	}

	private static void RebuildImage(SpriteAtlas spriteAtlas)
	{
		for (int num = m_TrackedTexturelessImages.Count - 1; num >= 0; num--)
		{
			BetterImage betterImage = m_TrackedTexturelessImages[num];
			if (spriteAtlas.CanBindTo(betterImage.activeSprite))
			{
				((Graphic)betterImage).SetAllDirty();
				m_TrackedTexturelessImages.RemoveAt(num);
			}
		}
	}

	private static void TrackImage(BetterImage g)
	{
		if (!s_Initialized)
		{
			SpriteAtlasManager.atlasRegistered += RebuildImage;
			s_Initialized = true;
		}
		m_TrackedTexturelessImages.Add(g);
	}

	private static void UnTrackImage(BetterImage g)
	{
		m_TrackedTexturelessImages.Remove(g);
	}
}
