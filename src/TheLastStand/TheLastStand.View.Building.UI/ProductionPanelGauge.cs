using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class ProductionPanelGauge : GraduatedGauge
{
	[SerializeField]
	private Transform unitsParent;

	[SerializeField]
	private RectTransform unitsBox;

	[SerializeField]
	private HorizontalLayoutGroup unitsLayoutGroup;

	[SerializeField]
	private List<Transform> unitsTransforms;

	private Dictionary<Transform, Image> unitsImages = new Dictionary<Transform, Image>();

	public override int MaxUnits => unitsTransforms.Count;

	public override void AddUnits(int amount, bool tween = true)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (amount <= 0)
		{
			return;
		}
		int num = base.Units;
		for (int i = 0; i < amount; i++)
		{
			if (num > MaxUnits - 1)
			{
				if (!clearOnCapacityExceeded)
				{
					break;
				}
				Clear();
				num = 0;
			}
			Transform val = unitsTransforms[num];
			((Component)val).gameObject.SetActive(true);
			if (tween)
			{
				((Graphic)unitsImages[val]).color = new Color(1f, 1f, 1f, 0f);
				TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade(unitsImages[val], 1f, 0.15f), (Ease)14);
				val.localScale = Vector3.one * (float)((num == MaxUnits - 1) ? 7 : 3);
				TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(val, 1f, 0.2f), (Ease)14);
			}
			else
			{
				((Graphic)unitsImages[val]).color = new Color(1f, 1f, 1f, 1f);
				val.localScale = Vector3.one;
			}
			num++;
		}
		((Behaviour)unitsLayoutGroup).enabled = true;
		Transform obj = unitsParent;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((obj is RectTransform) ? obj : null));
		((Behaviour)unitsLayoutGroup).enabled = false;
		base.Units = num;
	}

	public override void Clear()
	{
		for (int num = MaxUnits - 1; num >= 0; num--)
		{
			((Component)unitsTransforms[num]).gameObject.SetActive(false);
		}
		base.Units = 0;
	}

	public void SetUnitsCount(int count)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (MaxUnits == count)
		{
			return;
		}
		Transform obj = unitsTransforms[0];
		Rect rect = ((RectTransform)((obj is RectTransform) ? obj : null)).rect;
		float num = (((Rect)(ref rect)).width + ((HorizontalOrVerticalLayoutGroup)unitsLayoutGroup).spacing) * (float)(count - MaxUnits);
		RectTransform obj2 = unitsBox;
		obj2.sizeDelta += new Vector2(num, 0f);
		if (MaxUnits < count)
		{
			while (MaxUnits < count)
			{
				Transform item = Object.Instantiate<Transform>(unitsTransforms[0], unitsParent);
				unitsTransforms.Add(item);
			}
		}
		else
		{
			while (MaxUnits > count)
			{
				Transform val = unitsTransforms[unitsTransforms.Count - 1];
				unitsTransforms.Remove(val);
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
		}
		RefreshUnitsImagesDictionary();
	}

	protected override void Awake()
	{
		base.Awake();
		RefreshUnitsImagesDictionary();
	}

	private void RefreshUnitsImagesDictionary()
	{
		unitsImages.Clear();
		for (int num = unitsTransforms.Count - 1; num >= 0; num--)
		{
			unitsImages.Add(unitsTransforms[num], ((Component)unitsTransforms[num]).GetComponent<Image>());
			if ((Object)(object)unitsImages[unitsTransforms[num]] == (Object)null)
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogError((object)("ProductionUnitsGauge ERROR: There is no UnityEngine.UI.Image attached to " + ((Object)unitsTransforms[num]).name + "!"), (Object)(object)((Component)this).gameObject, (CLogLevel)1, true, true);
			}
		}
	}
}
