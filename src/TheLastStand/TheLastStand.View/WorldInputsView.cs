using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View;

public class WorldInputsView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public delegate void OnPointerOverWorldChanged(bool isPointerOverWorld);

	private static List<RaycastResult> raycastResults = new List<RaycastResult>();

	[SerializeField]
	private bool verbose;

	[SerializeField]
	protected BoxCollider2D worldCollider;

	[SerializeField]
	private EventSystem eventSystem;

	private bool isPointerOverWorld;

	protected float camRefAspect = -1f;

	protected float camRefOrthoSize = -1f;

	private GraphicRaycaster[] graphicsRaycasters;

	public bool IsPointerOverWorld
	{
		get
		{
			return isPointerOverWorld;
		}
		set
		{
			if (isPointerOverWorld != value)
			{
				isPointerOverWorld = value;
				this.PointerOverWorldChangeEvent?.Invoke(isPointerOverWorld);
			}
		}
	}

	public event OnPointerOverWorldChanged PointerOverWorldChangeEvent;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (verbose)
		{
			TPDebug.Log((object)"Pointer Enter", (Object)(object)this);
		}
		IsPointerOverWorld = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (verbose)
		{
			TPDebug.Log((object)"Pointer Exit", (Object)(object)this);
		}
		IsPointerOverWorld = false;
	}

	private void Awake()
	{
		if ((Object)(object)worldCollider == (Object)null)
		{
			worldCollider = ((Component)this).GetComponent<BoxCollider2D>();
		}
		graphicsRaycasters = TPSingleton<GameView>.Instance.MainCanvasGameObject.GetComponentsInChildren<GraphicRaycaster>(true);
		IsPointerOverWorld = !IsPointerOverUI();
	}

	private bool IsPointerOverUI(bool forceNoLog = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		PointerEventData val = new PointerEventData(eventSystem)
		{
			position = Vector2.op_Implicit(InputManager.MousePosition)
		};
		raycastResults.Clear();
		if (!verbose || forceNoLog)
		{
			int i = 0;
			for (int num = graphicsRaycasters.Length; i < num; i++)
			{
				((BaseRaycaster)graphicsRaycasters[i]).Raycast(val, raycastResults);
				if (raycastResults.Count > 0)
				{
					return true;
				}
			}
			return false;
		}
		bool flag = false;
		int j = 0;
		for (int num2 = graphicsRaycasters.Length; j < num2; j++)
		{
			((BaseRaycaster)graphicsRaycasters[j]).Raycast(val, raycastResults);
			if (raycastResults.Count > 0)
			{
				flag = true;
				for (int k = 0; k < raycastResults.Count; k++)
				{
					RaycastResult val2 = raycastResults[k];
					TPDebug.Log((object)("The cursor is over '" + ((Object)((RaycastResult)(ref val2)).gameObject).name + "'"), (Object)(object)graphicsRaycasters[j]);
				}
				raycastResults.Clear();
			}
		}
		if (flag)
		{
			Debug.Log((object)"---------- END OF RAYCAST --------------");
		}
		return flag;
	}

	public virtual void LateUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (camRefOrthoSize != ACameraView.MainCam.orthographicSize)
		{
			camRefOrthoSize = ACameraView.MainCam.orthographicSize;
			camRefAspect = ACameraView.MainCam.aspect;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(camRefOrthoSize * 2f * camRefAspect, camRefOrthoSize * 2f);
			val *= 1.1f;
			worldCollider.size = val;
		}
	}
}
