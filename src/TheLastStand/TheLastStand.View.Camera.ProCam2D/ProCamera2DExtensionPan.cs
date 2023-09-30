using System;
using Com.LuisPedroFonseca.ProCamera2D;
using Rewired;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Cursor;
using UnityEngine;

namespace TheLastStand.View.Camera.ProCam2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-pan-and-zoom/")]
public class ProCamera2DExtensionPan : BasePC2D, IPreMover
{
	public enum MouseButton
	{
		Left,
		Right,
		Middle
	}

	public Action OnPanStarted;

	public Action OnPanFinished;

	[Tooltip("If enabled, the user will be able to manually pan the camera")]
	public bool AllowPan = true;

	[Tooltip("Pan the camera by dragging the 'world'")]
	public bool UsePanByDrag = true;

	[Tooltip("Which mouse button do you want to use for panning? Only applicable if mouse is used")]
	public MouseButton PanMouseButton;

	[Tooltip("A normalized screen space area where the drag is active. Leave to default to use the whole screen")]
	public Rect DraggableAreaRect = new Rect(0f, 0f, 1f, 1f);

	[Tooltip("The speed at which to pan the camera")]
	public Vector2 DragPanSpeedMultiplier = new Vector2(1f, 1f);

	[Tooltip("How fast the camera inertia stops once the user starts dragging")]
	[Range(0f, 1f)]
	public float StopSpeedOnDragStart = 0.95f;

	[Tooltip("Pan the camera by moving the mouse to the edges of the screen")]
	public bool UsePanByMoveToEdges;

	[Tooltip("Pan the camera by moving the mouse to the edges of the screen")]
	public bool IgnoreUIOnEdges;

	[Tooltip("The speed at which the camera will move when the mouse reaches the edges of the screen")]
	public Vector2 EdgesPanSpeed = new Vector2(2f, 2f);

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving vertically")]
	[Range(0f, 0.99f)]
	public float TopPanEdge = 0.9f;

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving vertically")]
	[Range(0f, 0.99f)]
	public float BottomPanEdge = 0.9f;

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving horizontally")]
	[Range(0f, 0.99f)]
	public float LeftPanEdge = 0.9f;

	[Tooltip("If the mouse pointer goes beyond this edge the camera will start moving horizontally")]
	[Range(0f, 0.99f)]
	public float RightPanEdge = 0.9f;

	[Tooltip("Pan the camera by using keyboard (or controller, actually) inputs")]
	public bool UsePanByKeyboard = true;

	[Tooltip("The speed at which the camera will move when moved by the keyboard")]
	public Vector2 KeyboardPanSpeed = new Vector2(20f, 20f);

	[HideInInspector]
	public bool IsPanning;

	[HideInInspector]
	public bool ResetPrevPanPoint;

	private Vector2 panDelta;

	private Vector3 prevMousePosition;

	private bool cursorBordered;

	public Transform PanTarget { get; private set; }

	public int PrMOrder { get; set; }

	protected override void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((BasePC2D)this).Awake();
		PanTarget = new GameObject("PC2DPanTarget").transform;
		((BasePC2D)this).ProCamera2D.AddPreMover((IPreMover)(object)this);
	}

	protected override void OnDestroy()
	{
		((BasePC2D)this).OnDestroy();
		if (Object.op_Implicit((Object)(object)((BasePC2D)this).ProCamera2D))
		{
			((BasePC2D)this).ProCamera2D.RemovePreMover((IPreMover)(object)this);
		}
	}

	protected override void OnEnable()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((BasePC2D)this).OnEnable();
		((BasePC2D)this).ProCamera2D.AddCameraTarget(PanTarget, 1f, 1f, 0f, default(Vector2));
		CenterPanTargetOnCamera();
	}

	protected override void OnDisable()
	{
		((BasePC2D)this).OnDisable();
		ResetPrevPanPoint = true;
		((BasePC2D)this).ProCamera2D.RemoveCameraTarget(PanTarget, 0f);
	}

	private bool CanEdgePan()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!UsePanByMoveToEdges || !Application.isFocused)
		{
			return false;
		}
		ControllerType lastControllerType = InputManager.GetLastControllerType();
		if ((int)lastControllerType > 1)
		{
			if ((int)lastControllerType == 2)
			{
				bool flag = InputManager.IsLastControllerJoystick && TPSingleton<CursorView>.Instance.JoystickCursorMoving;
				return panDelta == Vector2.zero && flag;
			}
			if ((int)lastControllerType == 20)
			{
			}
		}
		if (panDelta == Vector2.zero)
		{
			if (!IgnoreUIOnEdges && !InputManager.IsPointerOverWorld && !InputManager.IsPointerOverAllowingCursorUI)
			{
				return cursorBordered;
			}
			return true;
		}
		return false;
	}

	private void Pan(float deltaTime)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		panDelta = Vector2.zero;
		Vector2 val = DragPanSpeedMultiplier;
		if (UsePanByDrag && InputManager.GetButtonDown(27))
		{
			CenterPanTargetOnCamera(StopSpeedOnDragStart);
		}
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(InputManager.MousePosition.x, InputManager.MousePosition.y, Mathf.Abs(base.Vector3D(((BasePC2D)this).ProCamera2D.LocalPosition)));
		bool button = InputManager.GetButton(27);
		if (UsePanByDrag && button)
		{
			Vector2 normalizedInput = default(Vector2);
			((Vector2)(ref normalizedInput))._002Ector(InputManager.MousePosition.x / (float)((BasePC2D)this).ProCamera2D.GameCamera.pixelWidth, InputManager.MousePosition.y / (float)((BasePC2D)this).ProCamera2D.GameCamera.pixelHeight);
			Rect pixelRect = ((BasePC2D)this).ProCamera2D.GameCamera.pixelRect;
			if (((Rect)(ref pixelRect)).Contains(val2) && InsideDraggableArea(normalizedInput))
			{
				Vector3 val3 = ((BasePC2D)this).ProCamera2D.GameCamera.ScreenToWorldPoint(prevMousePosition);
				if (ResetPrevPanPoint)
				{
					val3 = ((BasePC2D)this).ProCamera2D.GameCamera.ScreenToWorldPoint(val2);
					ResetPrevPanPoint = false;
				}
				Vector3 val4 = ((BasePC2D)this).ProCamera2D.GameCamera.ScreenToWorldPoint(val2);
				Vector3 arg = val3 - val4;
				panDelta = new Vector2(base.Vector3H(arg), base.Vector3V(arg));
			}
		}
		else if (!button)
		{
			bool flag = TPSingleton<HUDJoystickNavigationManager>.Exist() && TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn;
			if (UsePanByKeyboard && !flag)
			{
				Vector2 val5 = default(Vector2);
				((Vector2)(ref val5))._002Ector(InputManager.GetAxis(17), InputManager.GetAxis(18));
				panDelta = val5 * deltaTime;
				if (panDelta != Vector2.zero)
				{
					val = KeyboardPanSpeed;
				}
			}
			Vector3 val6 = (InputManager.IsLastControllerJoystick ? ACameraView.MainCam.WorldToScreenPoint(InputManager.JoystickCursorPosition) : InputManager.MousePosition);
			cursorBordered = false;
			if (TPSingleton<SettingsManager>.Instance.Settings.IsCursorRestricted && TPSingleton<SettingsManager>.Instance.Settings.WindowMode == SettingsManager.E_WindowMode.Windowed && Application.isFocused && (val6.x < 0f || val6.x >= (float)Screen.width || val6.y < 0f || val6.y > (float)Screen.height))
			{
				cursorBordered = true;
			}
			if (CanEdgePan())
			{
				float num = ((float)(-Screen.width) * 0.5f + val6.x) / (float)Screen.width;
				float num2 = ((float)(-Screen.height) * 0.5f + val6.y) / (float)Screen.height;
				if (num < 0f)
				{
					num = Utils.Remap(num, -0.5f, (0f - LeftPanEdge) * 0.5f, -0.5f, 0f);
				}
				else if (num > 0f)
				{
					num = Utils.Remap(num, RightPanEdge * 0.5f, 0.5f, 0f, 0.5f);
				}
				if (num2 < 0f)
				{
					num2 = Utils.Remap(num2, -0.5f, (0f - BottomPanEdge) * 0.5f, -0.5f, 0f);
				}
				else if (num2 > 0f)
				{
					num2 = Utils.Remap(num2, TopPanEdge * 0.5f, 0.5f, 0f, 0.5f);
				}
				panDelta = new Vector2(num, num2) * deltaTime;
				if (panDelta != Vector2.zero)
				{
					val = EdgesPanSpeed;
				}
			}
		}
		prevMousePosition = val2;
		if (panDelta != Vector2.zero)
		{
			Vector3 val7 = base.VectorHV(panDelta.x * val.x, panDelta.y * val.y);
			PanTarget.Translate(val7);
			if (!IsPanning && OnPanStarted != null)
			{
				OnPanStarted();
			}
			IsPanning = true;
		}
		if ((((BasePC2D)this).ProCamera2D.IsCameraPositionLeftBounded && base.Vector3H(PanTarget.position) < base.Vector3H(((BasePC2D)this).ProCamera2D.LocalPosition)) || (((BasePC2D)this).ProCamera2D.IsCameraPositionRightBounded && base.Vector3H(PanTarget.position) > base.Vector3H(((BasePC2D)this).ProCamera2D.LocalPosition)))
		{
			PanTarget.position = base.VectorHVD(base.Vector3H(((BasePC2D)this).ProCamera2D.LocalPosition) - ((BasePC2D)this).ProCamera2D.GetOffsetX() * 0.9999f, base.Vector3V(PanTarget.position), base.Vector3D(PanTarget.position));
		}
		if ((((BasePC2D)this).ProCamera2D.IsCameraPositionBottomBounded && base.Vector3V(PanTarget.position) < base.Vector3V(((BasePC2D)this).ProCamera2D.LocalPosition)) || (((BasePC2D)this).ProCamera2D.IsCameraPositionTopBounded && base.Vector3V(PanTarget.position) > base.Vector3V(((BasePC2D)this).ProCamera2D.LocalPosition)))
		{
			PanTarget.position = base.VectorHVD(base.Vector3H(PanTarget.position), base.Vector3V(((BasePC2D)this).ProCamera2D.LocalPosition) - ((BasePC2D)this).ProCamera2D.GetOffsetY() * 0.9999f, base.Vector3D(PanTarget.position));
		}
	}

	public void CenterPanTargetOnCamera(float interpolant = 1f)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PanTarget != (Object)null)
		{
			PanTarget.position = Vector3.Lerp(PanTarget.position, base.VectorHV(base.Vector3H(((BasePC2D)this).ProCamera2D.LocalPosition) - ((BasePC2D)this).ProCamera2D.GetOffsetX(), base.Vector3V(((BasePC2D)this).ProCamera2D.LocalPosition) - ((BasePC2D)this).ProCamera2D.GetOffsetY()), interpolant);
		}
	}

	private bool InsideDraggableArea(Vector2 normalizedInput)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (((Rect)(ref DraggableAreaRect)).x == 0f && ((Rect)(ref DraggableAreaRect)).y == 0f && ((Rect)(ref DraggableAreaRect)).width == 1f && ((Rect)(ref DraggableAreaRect)).height == 1f)
		{
			return true;
		}
		if (normalizedInput.x > ((Rect)(ref DraggableAreaRect)).x + (1f - ((Rect)(ref DraggableAreaRect)).width) / 2f && normalizedInput.x < ((Rect)(ref DraggableAreaRect)).x + ((Rect)(ref DraggableAreaRect)).width + (1f - ((Rect)(ref DraggableAreaRect)).width) / 2f && normalizedInput.y > ((Rect)(ref DraggableAreaRect)).y + (1f - ((Rect)(ref DraggableAreaRect)).height) / 2f && normalizedInput.y < ((Rect)(ref DraggableAreaRect)).y + ((Rect)(ref DraggableAreaRect)).height + (1f - ((Rect)(ref DraggableAreaRect)).height) / 2f)
		{
			return true;
		}
		return false;
	}

	public void PreMove(float deltaTime)
	{
		if (((Behaviour)this).enabled && AllowPan)
		{
			if (IsPanning && OnPanFinished != null)
			{
				OnPanFinished();
			}
			IsPanning = false;
			Pan(deltaTime);
		}
	}
}
