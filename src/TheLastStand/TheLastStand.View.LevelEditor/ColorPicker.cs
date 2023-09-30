using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager.LevelEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class ColorPicker : MonoBehaviour
{
	public enum PickType
	{
		Hover,
		Click,
		Drag
	}

	[SerializeField]
	private RectTransform colorPickerRect;

	[SerializeField]
	private Image colorPickerImage;

	[SerializeField]
	private Image previewImage;

	[SerializeField]
	private TextMeshProUGUI colorHexText;

	[SerializeField]
	private Button hexToClipboardButton;

	[Tooltip("Defines how the color should be updated by user input.")]
	[SerializeField]
	private PickType pickType = PickType.Drag;

	[Tooltip("Does not take fully transparent pixels into account.")]
	[SerializeField]
	private bool ignoreTransparent = true;

	[Tooltip("Only pick the tint and always set the color as opaque.")]
	[SerializeField]
	private bool ignoreAlpha = true;

	[SerializeField]
	private ColorEvent onColorHovered = new ColorEvent();

	[SerializeField]
	private ColorEvent onColorClicked = new ColorEvent();

	[SerializeField]
	private Color hoveredColorPreview = Color.white;

	[SerializeField]
	private Color clickedColorPreview = Color.white;

	private bool disablePreview = true;

	private Texture2D colorPickerTexture;

	private Color lastPickedColor;

	private bool previousMousePressed;

	private bool mousePressed;

	public ColorEvent OnColorHovered => onColorHovered;

	public ColorEvent OnColorClicked => onColorClicked;

	public string LastPickedColorToHtmlString
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			if (!ignoreAlpha)
			{
				return ColorUtility.ToHtmlStringRGBA(lastPickedColor);
			}
			return ColorUtility.ToHtmlStringRGB(lastPickedColor);
		}
	}

	private void PickColor(Color color)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		((UnityEvent<Color>)onColorClicked)?.Invoke(color);
		clickedColorPreview = color;
		lastPickedColor = color;
		if ((Object)(object)previewImage != (Object)null)
		{
			((Graphic)previewImage).color = color;
		}
		if ((Object)(object)colorHexText != (Object)null)
		{
			((TMP_Text)colorHexText).text = LastPickedColorToHtmlString;
		}
	}

	private void UpdateColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		if (!RectTransformUtility.RectangleContainsScreenPoint(colorPickerRect, Vector2.op_Implicit(Input.mousePosition)))
		{
			return;
		}
		Vector2 val = default(Vector2);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(colorPickerRect, Vector2.op_Implicit(Input.mousePosition), (Camera)null, ref val);
		Rect rect = colorPickerRect.rect;
		float width = ((Rect)(ref rect)).width;
		rect = colorPickerRect.rect;
		float height = ((Rect)(ref rect)).height;
		Vector2 val2 = val;
		rect = colorPickerRect.rect;
		val = val2 + ((Rect)(ref rect)).size * 0.5f;
		float num = val.x / width;
		float num2 = val.y / height;
		Color pixel = colorPickerTexture.GetPixel(Mathf.RoundToInt(num * (float)((Texture)colorPickerTexture).width), Mathf.RoundToInt(num2 * (float)((Texture)colorPickerTexture).height));
		if (ignoreTransparent && pixel.a == 0f)
		{
			return;
		}
		if (ignoreAlpha && pixel.a < 1f)
		{
			((Color)(ref pixel))._002Ector(pixel.r, pixel.g, pixel.b, 1f);
		}
		((UnityEvent<Color>)onColorHovered)?.Invoke(pixel);
		hoveredColorPreview = pixel;
		bool flag = lastPickedColor != pixel;
		if (!flag)
		{
			return;
		}
		switch (pickType)
		{
		case PickType.Click:
			if (mousePressed && !previousMousePressed)
			{
				PickColor(pixel);
			}
			break;
		case PickType.Hover:
			if (flag)
			{
				PickColor(pixel);
			}
			break;
		case PickType.Drag:
			if (flag && mousePressed)
			{
				PickColor(pixel);
			}
			break;
		default:
			((CLogger<LevelEditorManager>)TPSingleton<LevelEditorManager>.Instance).LogError((object)$"Unhandled color pick type {pickType}.", (CLogLevel)1, true, true);
			break;
		}
	}

	private void CopyPickedColorHexToClipboard()
	{
		GUIUtility.systemCopyBuffer = LastPickedColorToHtmlString;
	}

	private void Start()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		colorPickerTexture = colorPickerImage.sprite.texture;
		if ((Object)(object)hexToClipboardButton != (Object)null)
		{
			((UnityEvent)hexToClipboardButton.onClick).AddListener(new UnityAction(CopyPickedColorHexToClipboard));
		}
	}

	private void Update()
	{
		previousMousePressed = mousePressed;
		mousePressed = Input.GetMouseButton(0);
		UpdateColor();
	}

	private void OnDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)hexToClipboardButton != (Object)null)
		{
			((UnityEvent)hexToClipboardButton.onClick).RemoveListener(new UnityAction(CopyPickedColorHexToClipboard));
		}
	}
}
