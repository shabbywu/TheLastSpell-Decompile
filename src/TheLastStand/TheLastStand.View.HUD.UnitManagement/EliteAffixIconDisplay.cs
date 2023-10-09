using TPLib;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit.Enemy.Affix;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class EliteAffixIconDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public class Constants
	{
		public const string IconPathFormat = "View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{0}";

		public const string BackgroundPathFormat = "View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{0}";

		public const string HoverPathFormat = "View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{0}_Hovered";

		private const string GlobalPath = "View/Sprites/UI/Units/EnemiesAffixes/";
	}

	[SerializeField]
	private Image enemyAffixBackground;

	[SerializeField]
	private Image enemyAffixIcon;

	[SerializeField]
	private Image hover;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	private EnemyAffix affix;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Display(EnemyAffix newAffix, EnemyAffixEffectDefinition.E_EnemyAffixBoxType boxType)
	{
		affix = newAffix;
		enemyAffixIcon.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect.ToString()}", failSilently: false);
		enemyAffixBackground.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{boxType.ToString()}", failSilently: false);
		hover.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Backgrounds/EnemyAffix_Box_{boxType.ToString()}_Hovered", failSilently: false);
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
		((Behaviour)hover).enabled = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		((Behaviour)hover).enabled = true;
		UIManager.EliteAffixTooltip.Affix = affix;
		UIManager.EliteAffixTooltip.FollowElement.ChangeTarget(((Component)this).transform);
		UIManager.EliteAffixTooltip.Display();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		((Behaviour)hover).enabled = false;
		UIManager.EliteAffixTooltip.Hide();
	}

	public void OnJoystickSelect()
	{
		((Behaviour)hover).enabled = true;
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			UIManager.EliteAffixTooltip.Affix = affix;
			UIManager.EliteAffixTooltip.FollowElement.ChangeTarget(((Component)this).transform);
			UIManager.EliteAffixTooltip.Display();
		}
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips)
		{
			UIManager.EliteAffixTooltip.Affix = affix;
			UIManager.EliteAffixTooltip.FollowElement.ChangeTarget(((Component)this).transform);
			UIManager.EliteAffixTooltip.Display();
		}
		else
		{
			UIManager.GenericTooltip.Hide();
		}
	}
}
