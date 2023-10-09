using TMPro;
using TPLib;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class EnemyAffixDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI[] parametersTexts;

	[SerializeField]
	private GameObject[] parametersContainers;

	[SerializeField]
	private Image eliteAffixIcon;

	[SerializeField]
	private Image affixBoxBackground;

	[SerializeField]
	private Image affixBoxImage;

	[SerializeField]
	private Image eliteAffixBoxImage;

	[SerializeField]
	private DataSpriteTable affixBoxes;

	[SerializeField]
	private DataSpriteTable eliteAffixBoxes;

	public void Display(bool state)
	{
		((Component)this).gameObject.SetActive(state);
	}

	public void Refresh(EnemyAffixEffectDefinition.E_EnemyAffixEffect enemyAffixEffect, EnemyAffixEffectDefinition.E_EnemyAffixBoxType boxType, params string[] localizedParameters)
	{
		eliteAffixIcon.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{enemyAffixEffect}", failSilently: false);
		if (boxType != 0 && boxType == EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Elite)
		{
			eliteAffixBoxImage.sprite = eliteAffixBoxes._Sprites[localizedParameters.Length - 1];
		}
		else
		{
			affixBoxImage.sprite = affixBoxes._Sprites[localizedParameters.Length - 1];
		}
		((Behaviour)affixBoxBackground).enabled = boxType != EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Elite;
		((Behaviour)eliteAffixBoxImage).enabled = boxType == EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Elite;
		for (int i = 0; i < parametersContainers.Length; i++)
		{
			bool flag = i < localizedParameters.Length;
			parametersContainers[i].SetActive(flag);
			if (flag)
			{
				((TMP_Text)parametersTexts[i]).text = localizedParameters[i];
			}
		}
	}
}
