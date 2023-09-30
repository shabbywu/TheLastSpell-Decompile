using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class EnemyUnitManagementView : UnitManagementView<EnemyUnitManagementView>
{
	[SerializeField]
	protected UnitStatDisplay healthDisplay;

	[SerializeField]
	protected UnitStatDisplay stunResistDisplay;

	[SerializeField]
	private EnemyPortraitView unitPortraitView;

	[SerializeField]
	protected Image healthGaugeImage;

	[SerializeField]
	protected Sprite healthGaugeBaseSprite;

	[SerializeField]
	protected Sprite healthGaugeInvincibleSprite;

	[SerializeField]
	private TextMeshProUGUI enemyUnitDescriptionText;

	[SerializeField]
	private Canvas enemyUnitDescriptionCanvas;

	[SerializeField]
	private Image unitInfoBackgroundImageTop;

	[SerializeField]
	private Image unitInfoBackgroundImageArmor;

	[SerializeField]
	private Image unitInfoBackgroundImageBottom;

	[SerializeField]
	private Sprite unitInfoBackgroundImageBaseTop;

	[SerializeField]
	private Sprite unitInfoBackgroundImageBaseArmor;

	[SerializeField]
	private Sprite unitInfoBackgroundImageBaseBottom;

	[SerializeField]
	private Sprite unitInfoBackgroundImageEliteTop;

	[SerializeField]
	private Sprite unitInfoBackgroundImageEliteArmor;

	[SerializeField]
	private Sprite unitInfoBackgroundImageEliteBottom;

	public override void Init()
	{
		base.Init();
		healthDisplay.Init();
		stunResistDisplay.Init();
		healthDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		healthDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		stunResistDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.StunResistance];
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected override void RefreshView()
	{
		if (TileObjectSelectionManager.SelectedUnit == null || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.NightReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			UnitManagementView<EnemyUnitManagementView>.DisplayCanvas(display: false);
			if ((Object)(object)simpleFontLocalizedParent != (Object)null)
			{
				((FontLocalizedParent)simpleFontLocalizedParent).UnregisterChilds();
			}
			return;
		}
		base.RefreshView();
		EnemyUnit selectedEnemyUnit = TileObjectSelectionManager.SelectedEnemyUnit;
		healthDisplay.Refresh();
		healthGaugeImage.sprite = (selectedEnemyUnit.EnemyUnitTemplateDefinition.IsInvulnerable ? healthGaugeInvincibleSprite : healthGaugeBaseSprite);
		stunResistDisplay.Refresh();
		((EnemySkillBar)TPSingleton<EnemyUnitManagementView>.Instance.skillBar).Refresh();
		unitPortraitView.EnemyUnit = selectedEnemyUnit;
		unitPortraitView.RefreshPortrait();
		((TMP_Text)unitName).text = selectedEnemyUnit.Name;
		injuriesDisplay.Refresh(selectedEnemyUnit);
		bool flag = selectedEnemyUnit is EliteEnemyUnit;
		unitInfoBackgroundImageTop.sprite = (flag ? unitInfoBackgroundImageEliteTop : unitInfoBackgroundImageBaseTop);
		unitInfoBackgroundImageArmor.sprite = (flag ? unitInfoBackgroundImageEliteArmor : unitInfoBackgroundImageBaseArmor);
		unitInfoBackgroundImageBottom.sprite = (flag ? unitInfoBackgroundImageEliteBottom : unitInfoBackgroundImageBaseBottom);
		((Graphic)unitInfoBackgroundImageTop).SetNativeSize();
		((Graphic)unitInfoBackgroundImageArmor).SetNativeSize();
		((Graphic)unitInfoBackgroundImageBottom).SetNativeSize();
		RefreshLocalizedTexts();
		((MonoBehaviour)this).StartCoroutine(ForceUpdateDescriptionCanvas());
	}

	private IEnumerator ForceUpdateDescriptionCanvas()
	{
		Canvas obj = enemyUnitDescriptionCanvas;
		int sortingOrder = obj.sortingOrder;
		obj.sortingOrder = sortingOrder + 1;
		yield return null;
		Canvas obj2 = enemyUnitDescriptionCanvas;
		sortingOrder = obj2.sortingOrder;
		obj2.sortingOrder = sortingOrder - 1;
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Behaviour)canvas).enabled)
		{
			RefreshLocalizedTexts();
		}
	}

	private void RefreshLocalizedTexts()
	{
		EnemyUnit selectedEnemyUnit = TileObjectSelectionManager.SelectedEnemyUnit;
		((TMP_Text)unitName).text = selectedEnemyUnit.Name;
		((TMP_Text)enemyUnitDescriptionText).text = selectedEnemyUnit.Description;
	}
}
