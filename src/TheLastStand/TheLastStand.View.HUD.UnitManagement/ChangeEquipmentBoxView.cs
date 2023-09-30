using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD.UnitManagement;

public class ChangeEquipmentBoxView : MonoBehaviour, IJoystickSelect
{
	[SerializeField]
	private Transform changeButton;

	[SerializeField]
	private Transform firstWeaponSetPosition;

	[SerializeField]
	private Transform secondWeaponSetPosition;

	[SerializeField]
	private EquipmentBoxSlotView firstSetLeftSlot;

	[SerializeField]
	private EquipmentBoxSlotView firstSetRightSlot;

	[SerializeField]
	private EquipmentBoxSlotView secondSetLeftSlot;

	[SerializeField]
	private EquipmentBoxSlotView secondSetRightSlot;

	[SerializeField]
	private DataColor equipmentDisableColor;

	[SerializeField]
	[Range(0f, 3f)]
	private float changeEquipmentTime = 0.3f;

	private Tween changeButtonTween;

	public void OnSkillHover(bool select)
	{
		EventSystem.current.SetSelectedGameObject(select ? ((Component)this).gameObject : null);
	}

	public void OnDisplayTooltip(bool display)
	{
	}

	public void RefreshChangeButton(bool isFirstSet, bool changeInstantly = false)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = changeButtonTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		changeButtonTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(changeButton, isFirstSet ? firstWeaponSetPosition.position : secondWeaponSetPosition.position, changeInstantly ? 0f : changeEquipmentTime, false), (Ease)9);
		RefreshEquipmentColors(isFirstSet);
	}

	public void RefreshEquipmentSlots()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			firstSetLeftSlot.Refresh(null);
			firstSetRightSlot.Refresh(null);
			secondSetLeftSlot.Refresh(null);
			secondSetRightSlot.Refresh(null);
			secondSetLeftSlot.DisplayLockedImage(display: false);
			secondSetRightSlot.DisplayLockedImage(display: false);
			return;
		}
		if (TileObjectSelectionManager.SelectedPlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value))
		{
			firstSetRightSlot.Refresh(value[0].Item);
			if (value.Count > 1)
			{
				secondSetRightSlot.DisplayLockedImage(display: false);
				secondSetRightSlot.Refresh(value[1].Item);
			}
			else
			{
				secondSetRightSlot.DisplayLockedImage(display: true);
				secondSetRightSlot.Refresh(null);
			}
		}
		else
		{
			firstSetRightSlot.Refresh(null);
			secondSetRightSlot.Refresh(null);
		}
		if (TileObjectSelectionManager.SelectedPlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value2) && value2.Count > 0)
		{
			bool flag = value?[0].Item != null && value[0].Item.IsTwoHandedWeapon;
			firstSetLeftSlot.Refresh(flag ? value[0].Item : value2[0].Item, flag);
			if (value2.Count > 1)
			{
				secondSetLeftSlot.DisplayLockedImage(display: false);
				flag = value?[1].Item != null && value[1].Item.IsTwoHandedWeapon;
				secondSetLeftSlot.Refresh(flag ? value[1].Item : value2[1].Item, flag);
			}
			else
			{
				secondSetLeftSlot.DisplayLockedImage(display: true);
				secondSetLeftSlot.Refresh(null);
			}
		}
		else
		{
			firstSetLeftSlot.Refresh(null);
			secondSetLeftSlot.Refresh(null);
		}
	}

	private void RefreshEquipmentColors(bool isFirstSet)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		firstSetLeftSlot.RefreshColor(isFirstSet ? Color.white : equipmentDisableColor._Color, firstSetRightSlot.item?.IsTwoHandedWeapon ?? false);
		firstSetRightSlot.RefreshColor(isFirstSet ? Color.white : equipmentDisableColor._Color);
		secondSetLeftSlot.RefreshColor(isFirstSet ? equipmentDisableColor._Color : Color.white, secondSetRightSlot.item?.IsTwoHandedWeapon ?? false);
		secondSetRightSlot.RefreshColor(isFirstSet ? equipmentDisableColor._Color : Color.white);
	}
}
