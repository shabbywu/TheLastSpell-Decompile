using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Skill;
using TheLastStand.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillAreaOfEffectGrid : MonoBehaviour
{
	public class Constants
	{
		public const string SkillAreaOfEffectCellPoolId = "SkillAoeCellPrefab";
	}

	[SerializeField]
	private Image cellPrefab;

	[SerializeField]
	private SkillAreaOfEffectResourcesByGridSize skillAreaOfEffectResourcesByGridSize;

	private Canvas canvas;

	private SkillAreaOfEffectResourcesByGridSize.GridSizes gridSize;

	private bool usePooler;

	public RectTransform RectTransform
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public bool Displayed { get; private set; }

	public Canvas Canvas
	{
		get
		{
			if ((Object)(object)canvas == (Object)null)
			{
				canvas = ((Component)this).GetComponent<Canvas>();
			}
			return canvas;
		}
	}

	public void Refresh(TheLastStand.Model.Skill.Skill skill)
	{
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		int count = skill.SkillDefinition.AreaOfEffectDefinition.Pattern[0].Count;
		int count2 = skill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count;
		Displayed = ShouldDisplayAreaOfEffectGrid(skill.SkillDefinition.AreaOfEffectDefinition.Pattern);
		((Component)this).gameObject.SetActive(Displayed);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Floor((float)count / 2f), Mathf.Floor((float)count2 / 2f));
		if (count % 2 != 0 && count2 % 2 == 0)
		{
			val.y -= 1f;
		}
		for (int num = ((Component)this).transform.childCount - 1; num >= 0; num--)
		{
			if (usePooler)
			{
				((Component)((Component)this).transform.GetChild(num)).gameObject.SetActive(false);
			}
			else
			{
				Object.Destroy((Object)(object)((Component)((Component)this).transform.GetChild(num)).gameObject);
			}
		}
		if (!skillAreaOfEffectResourcesByGridSize.ResourcesByGridSize.TryGetValue(gridSize, out var value))
		{
			((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)$"Can't find Resources for this grid size : {gridSize} !", (CLogLevel)1, true, true);
			return;
		}
		Vector2 val3 = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < count2; j++)
			{
				Sprite val2 = (Sprite)(skill.SkillDefinition.AreaOfEffectDefinition.Pattern[count2 - 1 - j][i] switch
				{
					'X' => value.AreaOfEffectSprite, 
					'M' => value.ManeuverSprite, 
					'e' => value.SurroundingSprite, 
					_ => null, 
				});
				if ((Object)(object)val2 != (Object)null)
				{
					((Vector2)(ref val3))._002Ector(value.GridCenter.x + ((float)i - val.x) * (float)value.CellSize, value.GridCenter.y - ((float)j - val.y) * (float)value.CellSize);
					Image obj = (usePooler ? ObjectPooler.GetPooledComponent<Image>("SkillAoeCellPrefab", cellPrefab, ((Component)this).transform, dontSetParent: false) : Object.Instantiate<Image>(cellPrefab, ((Component)this).transform));
					((Component)obj).transform.localPosition = Vector2.op_Implicit(val3);
					obj.sprite = val2;
				}
			}
		}
	}

	private void Awake()
	{
		usePooler = (Object)(object)SingletonBehaviour<ObjectPooler>.Instance != (Object)null;
	}

	private bool ShouldDisplayAreaOfEffectGrid(List<List<char>> pattern)
	{
		int num = 0;
		for (int i = 0; i < pattern.Count; i++)
		{
			for (int j = 0; j < pattern[i].Count; j++)
			{
				if (pattern[i][j] != '_')
				{
					num++;
				}
			}
		}
		return num > 1;
	}
}
