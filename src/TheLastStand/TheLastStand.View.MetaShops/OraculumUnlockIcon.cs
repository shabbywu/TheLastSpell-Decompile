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
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Selectable obj = Selectable;
		Navigation navigation = ((Selectable)metaUpgradeLineView.JoystickSelectable).navigation;
		SelectableExtensions.SetSelectOnUp(obj, ((Navigation)(ref navigation)).selectOnUp);
		Selectable obj2 = Selectable;
		navigation = ((Selectable)metaUpgradeLineView.JoystickSelectable).navigation;
		SelectableExtensions.SetSelectOnDown(obj2, ((Navigation)(ref navigation)).selectOnDown);
	}

	public virtual void OnDeselect(BaseEventData eventData)
	{
		metaUpgradeLineView.OnIconDeselect();
	}

	protected void SetMetaUpgrade(MetaUpgradeLineView metaUpgradeLineView)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		this.metaUpgradeLineView = metaUpgradeLineView;
		SelectableExtensions.SetMode(Selectable, (Mode)4);
		Selectable obj = Selectable;
		Navigation navigation = ((Selectable)this.metaUpgradeLineView.JoystickSelectable).navigation;
		SelectableExtensions.SetSelectOnUp(obj, ((Navigation)(ref navigation)).selectOnUp);
		Selectable obj2 = Selectable;
		navigation = ((Selectable)this.metaUpgradeLineView.JoystickSelectable).navigation;
		SelectableExtensions.SetSelectOnDown(obj2, ((Navigation)(ref navigation)).selectOnDown);
	}
}
