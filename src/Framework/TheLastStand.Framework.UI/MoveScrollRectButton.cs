using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

[RequireComponent(typeof(Button))]
public class MoveScrollRectButton : MonoBehaviour
{
	public enum E_moveMethod
	{
		Relative,
		Pixels
	}

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private E_moveMethod moveMethod;

	[SerializeField]
	private RectTransform scrollRectContentRectTransform;

	[SerializeField]
	private float moveValue = 0.1f;

	public void MoveScrollRect()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		switch (moveMethod)
		{
		case E_moveMethod.Relative:
			if (scrollRect.horizontal)
			{
				ScrollRect obj = scrollRect;
				obj.horizontalNormalizedPosition += moveValue;
			}
			if (scrollRect.vertical)
			{
				ScrollRect obj2 = scrollRect;
				obj2.verticalNormalizedPosition += moveValue;
			}
			break;
		case E_moveMethod.Pixels:
		{
			Vector3 zero = Vector3.zero;
			if (scrollRect.horizontal)
			{
				zero.x = 0f - moveValue;
			}
			if (scrollRect.vertical)
			{
				zero.y = 0f - moveValue;
			}
			((Transform)scrollRectContentRectTransform).Translate(zero);
			break;
		}
		}
	}
}
