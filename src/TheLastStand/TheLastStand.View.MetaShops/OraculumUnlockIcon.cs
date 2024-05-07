using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class OraculumUnlockIcon : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private PointerEventsListener pointerEventsListener;

	private MetaUpgradeLineView metaUpgradeLineView;

	private Selectable selectable;

	public PointerEventsListener PointerEventsListener => pointerEventsListener;

	public Selectable Selectable
	{
		get
		{
			if ((Object)(object)selectable == (Object)null)
			{
				selectable = ((Component)this).GetComponent<Selectable>();
			}
			return selectable;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		RefreshTopAndBottomNavigation();
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		metaUpgradeLineView.OnIconDeselect();
	}

	protected void SetMetaUpgrade(MetaUpgradeLineView metaUpgradeLineView)
	{
		this.metaUpgradeLineView = metaUpgradeLineView;
		Selectable.SetMode((Mode)4);
		RefreshTopAndBottomNavigation();
	}

	private void RefreshTopAndBottomNavigation()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		int num = 5;
		int siblingIndex = transform.GetSiblingIndex();
		int childCount = ((Component)transform.parent).transform.childCount;
		int num2 = childCount % num;
		int num3 = ((num2 == 0) ? (childCount - num) : (childCount - num2));
		Navigation navigation;
		if (siblingIndex < num)
		{
			Selectable obj = Selectable;
			navigation = ((Selectable)metaUpgradeLineView.JoystickSelectable).navigation;
			obj.SetSelectOnUp(((Navigation)(ref navigation)).selectOnUp);
		}
		if (siblingIndex >= num3)
		{
			Selectable obj2 = Selectable;
			navigation = ((Selectable)metaUpgradeLineView.JoystickSelectable).navigation;
			obj2.SetSelectOnDown(((Navigation)(ref navigation)).selectOnDown);
		}
	}
}
