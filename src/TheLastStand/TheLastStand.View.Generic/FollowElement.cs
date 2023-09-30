using System;
using System.Collections.Generic;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Generic;

[RequireComponent(typeof(RectTransform))]
public class FollowElement : MonoBehaviour
{
	[Serializable]
	public class FollowDatas
	{
		[Tooltip("Local offset to add to the final position.")]
		[SerializeField]
		private Vector3 offset = Vector3.zero;

		[Tooltip("If true, the local offset will be multiplied by the scale of this object./n/!\\ The scale used here is the lossy scale, not the local!")]
		[SerializeField]
		private bool offsetAffectedByScale;

		[Tooltip("Leave empty to follow the mouse/joystick cursor (when Always Follow Target is true), or define a target to follow.")]
		[SerializeField]
		private Transform followTarget;

		[SerializeField]
		[FormerlySerializedAs("autoMove")]
		private bool alwaysFollow = true;

		[SerializeField]
		private bool clampToParent;

		public bool AlwaysFollow
		{
			get
			{
				return alwaysFollow;
			}
			set
			{
				alwaysFollow = value;
			}
		}

		public bool ClampToParent
		{
			get
			{
				return clampToParent;
			}
			set
			{
				clampToParent = value;
			}
		}

		public Transform FollowTarget
		{
			get
			{
				return followTarget;
			}
			set
			{
				followTarget = value;
			}
		}

		public Vector3 Offset
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return offset;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				offset = value;
			}
		}

		public bool OffsetAffectedByScale
		{
			get
			{
				return offsetAffectedByScale;
			}
			set
			{
				offsetAffectedByScale = value;
			}
		}

		public FollowDatas()
		{
		}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)


		public FollowDatas(FollowDatas copy)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			alwaysFollow = copy.alwaysFollow;
			offset = new Vector3(copy.offset.x, copy.offset.y, copy.offset.z);
			followTarget = copy.followTarget;
			offsetAffectedByScale = copy.offsetAffectedByScale;
		}
	}

	[SerializeField]
	private FollowDatas followElementDatas = new FollowDatas();

	[SerializeField]
	private Camera targetCamera;

	private HashSet<MonoBehaviour> scriptsUsingMe;

	private Vector3 initFollowOffset;

	public bool ConvertToScreenSpace;

	public FollowDatas FollowElementDatas => followElementDatas;

	public RectTransform RectTransform { get; private set; }

	private Vector3 ScaledOffset => followElementDatas.Offset * ((Component)this).transform.lossyScale.x;

	public void AutoMove()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)FollowElementDatas.FollowTarget != (Object)null)
		{
			SetPosition((ConvertToScreenSpace && (Object)(object)targetCamera != (Object)null) ? targetCamera.WorldToScreenPoint(FollowElementDatas.FollowTarget.position) : FollowElementDatas.FollowTarget.position);
		}
		else
		{
			SetPosition(InputManager.IsLastControllerJoystick ? ACameraView.MainCam.WorldToScreenPoint(InputManager.JoystickCursorPosition) : InputManager.MousePosition);
		}
	}

	public void ChangeFollowDatas(FollowDatas followDatas)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		FollowElementDatas.AlwaysFollow = followDatas.AlwaysFollow;
		FollowElementDatas.ClampToParent = followDatas.ClampToParent;
		FollowElementDatas.FollowTarget = followDatas.FollowTarget;
		FollowElementDatas.Offset = followDatas.Offset;
		FollowElementDatas.OffsetAffectedByScale = followDatas.OffsetAffectedByScale;
		AutoMove();
	}

	public void RestoreFollowDatasOffset()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		FollowElementDatas.Offset = initFollowOffset;
	}

	public void ChangeOffset(Vector3 offset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		FollowElementDatas.Offset = offset;
		if (!FollowElementDatas.AlwaysFollow)
		{
			AutoMove();
		}
	}

	public void ChangeTarget(Transform newTarget)
	{
		if ((Object)(object)FollowElementDatas.FollowTarget != (Object)(object)newTarget)
		{
			FollowElementDatas.FollowTarget = newTarget;
			AutoMove();
		}
	}

	public void SetPosition(Vector3 targetPos)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)RectTransform == (Object)null))
		{
			if ((targetPos += (FollowElementDatas.OffsetAffectedByScale ? ScaledOffset : FollowElementDatas.Offset)) != ((Transform)RectTransform).position)
			{
				((Transform)RectTransform).position = targetPos;
			}
			if (FollowElementDatas.ClampToParent)
			{
				RectTransformExtensions.ClampToParent(RectTransform);
			}
		}
	}

	public void StartUseBy(MonoBehaviour script)
	{
		if (scriptsUsingMe == null)
		{
			scriptsUsingMe = new HashSet<MonoBehaviour>();
		}
		if (scriptsUsingMe.Add(script))
		{
			((Behaviour)this).enabled = true;
		}
	}

	public void StopUseBy(MonoBehaviour script)
	{
		if (scriptsUsingMe != null && scriptsUsingMe.Remove(script) && scriptsUsingMe.Count == 0)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		RectTransform = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		initFollowOffset = FollowElementDatas.Offset;
	}

	private void LateUpdate()
	{
		if (FollowElementDatas.AlwaysFollow)
		{
			AutoMove();
		}
	}
}
