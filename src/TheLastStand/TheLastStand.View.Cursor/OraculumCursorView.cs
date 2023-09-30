using DG.Tweening;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Cursor;

public class OraculumCursorView : MonoBehaviour
{
	private Tween hideTween;

	private float joystickFastSpeedTimer;

	public bool Enabled => ((Component)this).gameObject.activeSelf;

	public void Enable(bool isOn)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (isOn != Enabled)
		{
			Vector3 position = ACameraView.MainCam.ViewportToScreenPoint(Vector2.op_Implicit(new Vector2(0.5f, 0.5f)));
			SetPosition(position);
			((Component)this).gameObject.SetActive(InputManager.IsLastControllerJoystick && isOn);
		}
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
	}

	private void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		float axis = InputManager.GetAxis(77);
		float axis2 = InputManager.GetAxis(78);
		Vector3 zero = default(Vector3);
		((Vector3)(ref zero))._002Ector(axis, axis2);
		float magnitude = ((Vector3)(ref zero)).magnitude;
		if (magnitude < InputManager.JoystickConfig.DefaultDeadZone)
		{
			zero = Vector3.zero;
		}
		float num;
		if (magnitude < InputManager.JoystickConfig.Cursor.FastSpeedStartInclination)
		{
			num = InputManager.JoystickConfig.Cursor.OraculumSlowSpeed;
			joystickFastSpeedTimer = 0f;
		}
		else
		{
			num = Mathf.Lerp(InputManager.JoystickConfig.Cursor.OraculumFastSpeedMinMax.x, InputManager.JoystickConfig.Cursor.OraculumFastSpeedMinMax.y, joystickFastSpeedTimer);
			if (InputManager.JoystickConfig.Cursor.FastSpeedTransitionDuration == 0f)
			{
				joystickFastSpeedTimer = 1f;
			}
			else
			{
				joystickFastSpeedTimer += Time.deltaTime / InputManager.JoystickConfig.Cursor.OraculumFastSpeedTransitionDuration;
			}
		}
		Vector3 position = ((Component)this).transform.position + (InputManager.JoystickConfig.Cursor.NormalizeInput ? ((Vector3)(ref zero)).normalized : zero) * num * Time.deltaTime;
		SetPosition(ClampToScreen(position));
	}

	private static Vector3 ClampToScreen(Vector3 position)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!InputManager.JoystickConfig.Cursor.ClampToScreen)
		{
			return position;
		}
		Vector3 val = ACameraView.MainCam.ViewportToScreenPoint(Vector2.op_Implicit(Vector2.zero));
		Vector3 val2 = ACameraView.MainCam.ViewportToScreenPoint(Vector2.op_Implicit(Vector2.one));
		position.x = Mathf.Clamp(position.x, val.x, val2.x);
		position.y = Mathf.Clamp(position.y, val.y, val2.y);
		return position;
	}
}
