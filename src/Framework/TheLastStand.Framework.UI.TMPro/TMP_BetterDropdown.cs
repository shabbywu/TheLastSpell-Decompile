using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TheLastStand.Framework.UI.CoroutineTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI.TMPro;

[RequireComponent(typeof(RectTransform))]
public class TMP_BetterDropdown : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler, ICancelHandler
{
	protected class DropdownItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ICancelHandler
	{
		[SerializeField]
		private TMP_Text m_Text;

		[SerializeField]
		private Image m_Image;

		[SerializeField]
		private RectTransform m_RectTransform;

		[SerializeField]
		private Toggle m_Toggle;

		[SerializeField]
		private bool m_isHovered;

		public TMP_Text text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
			}
		}

		public Image image
		{
			get
			{
				return m_Image;
			}
			set
			{
				m_Image = value;
			}
		}

		public RectTransform rectTransform
		{
			get
			{
				return m_RectTransform;
			}
			set
			{
				m_RectTransform = value;
			}
		}

		public Toggle toggle
		{
			get
			{
				return m_Toggle;
			}
			set
			{
				m_Toggle = value;
			}
		}

		public bool IsHovered
		{
			get
			{
				return m_isHovered;
			}
			set
			{
				m_isHovered = value;
			}
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			IsHovered = true;
			EventSystem.current.SetSelectedGameObject(((Component)this).gameObject);
		}

		public virtual void OnCancel(BaseEventData eventData)
		{
			TMP_BetterDropdown componentInParent = ((Component)this).GetComponentInParent<TMP_BetterDropdown>();
			if (Object.op_Implicit((Object)(object)componentInParent))
			{
				componentInParent.Hide();
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			IsHovered = false;
		}
	}

	[Serializable]
	public class OptionData
	{
		[SerializeField]
		private string m_Text;

		[SerializeField]
		private Sprite m_Image;

		public string text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
			}
		}

		public Sprite image
		{
			get
			{
				return m_Image;
			}
			set
			{
				m_Image = value;
			}
		}

		public OptionData()
		{
		}

		public OptionData(string text)
		{
			this.text = text;
		}

		public OptionData(Sprite image)
		{
			this.image = image;
		}

		public OptionData(string text, Sprite image)
		{
			this.text = text;
			this.image = image;
		}
	}

	[Serializable]
	public class OptionDataList
	{
		[SerializeField]
		private List<OptionData> m_Options;

		public List<OptionData> options
		{
			get
			{
				return m_Options;
			}
			set
			{
				m_Options = value;
			}
		}

		public OptionDataList()
		{
			options = new List<OptionData>();
		}
	}

	[Serializable]
	public class DropdownEvent : UnityEvent<int>
	{
	}

	[SerializeField]
	private RectTransform m_Template;

	[SerializeField]
	private TMP_Text m_CaptionText;

	[SerializeField]
	private Image m_CaptionImage;

	[Space]
	[SerializeField]
	protected TMP_Text m_ItemText;

	[SerializeField]
	protected Image m_ItemImage;

	[Space]
	[SerializeField]
	private int m_Value;

	[Space]
	[SerializeField]
	private OptionDataList m_Options = new OptionDataList();

	[SerializeField]
	private bool m_AutoDeselection;

	[Space]
	[SerializeField]
	private DropdownEvent m_OnValueChanged = new DropdownEvent();

	[SerializeField]
	private UnityEvent m_OnDropDownOpened = new UnityEvent();

	[SerializeField]
	private UnityEvent m_OnDropDownClosed = new UnityEvent();

	[SerializeField]
	private UnityEvent m_onPointerEnter = new UnityEvent();

	private GameObject m_Dropdown;

	private GameObject m_Blocker;

	protected List<DropdownItem> m_Items = new List<DropdownItem>();

	private TweenRunner<FloatTween> m_AlphaTweenRunner;

	private bool validTemplate;

	private bool m_displayed;

	private static OptionData s_NoOptionData = new OptionData();

	public RectTransform template
	{
		get
		{
			return m_Template;
		}
		set
		{
			m_Template = value;
			RefreshShownValue();
		}
	}

	public TMP_Text captionText
	{
		get
		{
			return m_CaptionText;
		}
		set
		{
			m_CaptionText = value;
			RefreshShownValue();
		}
	}

	public Image captionImage
	{
		get
		{
			return m_CaptionImage;
		}
		set
		{
			m_CaptionImage = value;
			RefreshShownValue();
		}
	}

	public TMP_Text itemText
	{
		get
		{
			return m_ItemText;
		}
		set
		{
			m_ItemText = value;
			RefreshShownValue();
		}
	}

	public Image itemImage
	{
		get
		{
			return m_ItemImage;
		}
		set
		{
			m_ItemImage = value;
			RefreshShownValue();
		}
	}

	public List<OptionData> options
	{
		get
		{
			return m_Options.options;
		}
		set
		{
			m_Options.options = value;
			RefreshShownValue();
		}
	}

	public bool autoDeselection
	{
		get
		{
			return m_AutoDeselection;
		}
		set
		{
			m_AutoDeselection = value;
			RefreshShownValue();
		}
	}

	public DropdownEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			m_OnValueChanged = value;
		}
	}

	public UnityEvent OnDropDownOpened => m_OnDropDownOpened;

	public UnityEvent OnDropDownClosed => m_OnDropDownClosed;

	public UnityEventBase OnPointerEnterEvent => (UnityEventBase)(object)m_onPointerEnter;

	public bool Displayed
	{
		get
		{
			return m_displayed;
		}
		set
		{
			m_displayed = value;
		}
	}

	public int value
	{
		get
		{
			return m_Value;
		}
		set
		{
			SetValue(value);
		}
	}

	public bool IsExpanded => (Object)(object)m_Dropdown != (Object)null;

	public void SetValueWithoutNotify(int input)
	{
		SetValue(input, sendCallback: false);
	}

	private void SetValue(int value, bool sendCallback = true)
	{
		if (!Application.isPlaying || (value != m_Value && options.Count != 0))
		{
			m_Value = Mathf.Clamp(value, 0, options.Count - 1);
			RefreshShownValue();
			if (sendCallback)
			{
				UISystemProfilerApi.AddMarker("Dropdown.value", (Object)(object)this);
				((UnityEvent<int>)m_OnValueChanged).Invoke(m_Value);
			}
		}
	}

	protected TMP_BetterDropdown()
	{
	}//IL_0017: Unknown result type (might be due to invalid IL or missing references)
	//IL_0021: Expected O, but got Unknown
	//IL_0022: Unknown result type (might be due to invalid IL or missing references)
	//IL_002c: Expected O, but got Unknown
	//IL_002d: Unknown result type (might be due to invalid IL or missing references)
	//IL_0037: Expected O, but got Unknown


	protected override void Awake()
	{
		m_AlphaTweenRunner = new TweenRunner<FloatTween>();
		m_AlphaTweenRunner.Init((MonoBehaviour)(object)this);
		if (Object.op_Implicit((Object)(object)m_CaptionImage))
		{
			((Behaviour)m_CaptionImage).enabled = (Object)(object)m_CaptionImage.sprite != (Object)null;
		}
		if (Object.op_Implicit((Object)(object)m_Template))
		{
			((Component)m_Template).gameObject.SetActive(false);
		}
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
		RefreshShownValue();
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		((Selectable)this).OnPointerEnter(eventData);
		if (((Selectable)this).interactable)
		{
			m_onPointerEnter.Invoke();
		}
	}

	protected override void OnDisable()
	{
		ImmediateDestroyDropdownList();
		if ((Object)(object)m_Blocker != (Object)null)
		{
			DestroyBlocker(m_Blocker);
		}
		m_Blocker = null;
	}

	public virtual void RefreshShownValue()
	{
		OptionData optionData = s_NoOptionData;
		if (options.Count > 0)
		{
			optionData = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];
		}
		if (Object.op_Implicit((Object)(object)m_CaptionText))
		{
			if (optionData != null && optionData.text != null)
			{
				m_CaptionText.text = optionData.text;
			}
			else
			{
				m_CaptionText.text = "";
			}
		}
		if (Object.op_Implicit((Object)(object)m_CaptionImage))
		{
			if (optionData != null)
			{
				m_CaptionImage.sprite = optionData.image;
			}
			else
			{
				m_CaptionImage.sprite = null;
			}
			((Behaviour)m_CaptionImage).enabled = (Object)(object)m_CaptionImage.sprite != (Object)null;
		}
	}

	public void AddOptions(List<OptionData> options)
	{
		this.options.AddRange(options);
		RefreshShownValue();
	}

	public void AddOptions(List<string> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			this.options.Add(new OptionData(options[i]));
		}
		RefreshShownValue();
	}

	public void AddOptions(List<Sprite> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			this.options.Add(new OptionData(options[i]));
		}
		RefreshShownValue();
	}

	public void ClearOptions()
	{
		options.Clear();
		m_Value = 0;
		RefreshShownValue();
	}

	public void SelectHoveredItem()
	{
		GameObject go = EventSystem.current.currentSelectedGameObject;
		if ((Object)(object)go != (Object)null)
		{
			SetValue(m_Items.FindIndex((DropdownItem x) => (Object)(object)((Component)x).gameObject == (Object)(object)go));
		}
	}

	private void SetupTemplate()
	{
		validTemplate = false;
		if (!Object.op_Implicit((Object)(object)m_Template))
		{
			Debug.LogError((object)"The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", (Object)(object)this);
			return;
		}
		GameObject gameObject = ((Component)m_Template).gameObject;
		gameObject.SetActive(true);
		Toggle componentInChildren = ((Component)m_Template).GetComponentInChildren<Toggle>();
		validTemplate = true;
		if (!Object.op_Implicit((Object)(object)componentInChildren) || (Object)(object)((Component)componentInChildren).transform == (Object)(object)template)
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The template must have a child GameObject with a Toggle component serving as the item.", (Object)(object)template);
		}
		else if (!(((Component)componentInChildren).transform.parent is RectTransform))
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", (Object)(object)template);
		}
		else if ((Object)(object)itemText != (Object)null && !itemText.transform.IsChildOf(((Component)componentInChildren).transform))
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", (Object)(object)template);
		}
		else if ((Object)(object)itemImage != (Object)null && !((Component)itemImage).transform.IsChildOf(((Component)componentInChildren).transform))
		{
			validTemplate = false;
			Debug.LogError((object)"The dropdown template is not valid. The Item Image must be on the item GameObject or children of it.", (Object)(object)template);
		}
		if (!validTemplate)
		{
			gameObject.SetActive(false);
			return;
		}
		AddDropdownItemComponent(componentInChildren);
		Canvas orAddComponent = GetOrAddComponent<Canvas>(gameObject);
		orAddComponent.overrideSorting = true;
		orAddComponent.sortingOrder = 30000;
		GetOrAddComponent<GraphicRaycaster>(gameObject);
		GetOrAddComponent<CanvasGroup>(gameObject);
		gameObject.SetActive(false);
		validTemplate = true;
	}

	protected virtual void AddDropdownItemComponent(Toggle itemToggle)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		DropdownItem dropdownItem = ((Component)itemToggle).gameObject.AddComponent<DropdownItem>();
		dropdownItem.text = m_ItemText;
		dropdownItem.image = m_ItemImage;
		dropdownItem.toggle = itemToggle;
		dropdownItem.rectTransform = (RectTransform)((Component)itemToggle).transform;
	}

	private static T GetOrAddComponent<T>(GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		Show();
	}

	public virtual void OnSubmit(BaseEventData eventData)
	{
		Show();
	}

	public virtual void OnCancel(BaseEventData eventData)
	{
		Hide();
	}

	public void Show()
	{
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		if (!((UIBehaviour)this).IsActive() || !((Selectable)this).IsInteractable() || (Object)(object)m_Dropdown != (Object)null)
		{
			return;
		}
		List<Canvas> list = TMP_ListPool<Canvas>.Get();
		((Component)this).gameObject.GetComponentsInParent<Canvas>(false, list);
		if (list.Count == 0)
		{
			return;
		}
		Canvas val = list[list.Count - 1];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].isRootCanvas)
			{
				val = list[i];
				break;
			}
		}
		TMP_ListPool<Canvas>.Release(list);
		if (!validTemplate)
		{
			SetupTemplate();
			if (!validTemplate)
			{
				return;
			}
		}
		((Component)m_Template).gameObject.SetActive(true);
		((Component)m_Template).GetComponent<Canvas>().sortingLayerID = val.sortingLayerID;
		m_Dropdown = CreateDropdownList(((Component)m_Template).gameObject);
		((Object)m_Dropdown).name = "Dropdown List";
		m_Dropdown.SetActive(true);
		Transform transform = m_Dropdown.transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		((Transform)val2).SetParent(((Component)m_Template).transform.parent, false);
		DropdownItem componentInChildren = m_Dropdown.GetComponentInChildren<DropdownItem>();
		Transform transform2 = ((Component)((Transform)componentInChildren.rectTransform).parent).gameObject.transform;
		RectTransform val3 = (RectTransform)(object)((transform2 is RectTransform) ? transform2 : null);
		((Component)componentInChildren.rectTransform).gameObject.SetActive(true);
		Rect rect = val3.rect;
		Rect rect2 = componentInChildren.rectTransform.rect;
		Vector2 val4 = ((Rect)(ref rect2)).min - ((Rect)(ref rect)).min + Vector2.op_Implicit(((Transform)componentInChildren.rectTransform).localPosition);
		Vector2 val5 = ((Rect)(ref rect2)).max - ((Rect)(ref rect)).max + Vector2.op_Implicit(((Transform)componentInChildren.rectTransform).localPosition);
		Vector2 size = ((Rect)(ref rect2)).size;
		m_Items.Clear();
		Toggle val6 = null;
		for (int j = 0; j < options.Count; j++)
		{
			OptionData data = options[j];
			DropdownItem item = AddItem(data, value == j, componentInChildren, m_Items);
			if (!((Object)(object)item == (Object)null))
			{
				item.toggle.isOn = value == j;
				((UnityEvent<bool>)(object)item.toggle.onValueChanged).AddListener((UnityAction<bool>)delegate
				{
					OnSelectItem(item.toggle);
				});
				if (item.toggle.isOn)
				{
					((Selectable)item.toggle).Select();
				}
				if ((Object)(object)val6 != (Object)null)
				{
					Navigation navigation = ((Selectable)val6).navigation;
					Navigation navigation2 = ((Selectable)item.toggle).navigation;
					((Navigation)(ref navigation)).mode = (Mode)4;
					((Navigation)(ref navigation2)).mode = (Mode)4;
					((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)item.toggle;
					((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)item.toggle;
					((Navigation)(ref navigation2)).selectOnLeft = (Selectable)(object)val6;
					((Navigation)(ref navigation2)).selectOnUp = (Selectable)(object)val6;
					((Selectable)val6).navigation = navigation;
					((Selectable)item.toggle).navigation = navigation2;
				}
				val6 = item.toggle;
			}
		}
		Vector2 sizeDelta = val3.sizeDelta;
		sizeDelta.y = size.y * (float)m_Items.Count + val4.y - val5.y;
		val3.sizeDelta = sizeDelta;
		Rect rect3 = val2.rect;
		float height = ((Rect)(ref rect3)).height;
		rect3 = val3.rect;
		float num = height - ((Rect)(ref rect3)).height;
		if (num > 0f)
		{
			val2.sizeDelta = new Vector2(val2.sizeDelta.x, val2.sizeDelta.y - num);
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		val2.GetWorldCorners(array);
		Transform transform3 = ((Component)val).transform;
		RectTransform val7 = (RectTransform)(object)((transform3 is RectTransform) ? transform3 : null);
		Rect rect4 = val7.rect;
		for (int k = 0; k < 2; k++)
		{
			bool flag = false;
			for (int l = 0; l < 4; l++)
			{
				Vector3 val8 = ((Transform)val7).InverseTransformPoint(array[l]);
				float num2 = ((Vector3)(ref val8))[k];
				Vector2 val9 = ((Rect)(ref rect4)).min;
				if (num2 < ((Vector2)(ref val9))[k])
				{
					float num3 = ((Vector3)(ref val8))[k];
					val9 = ((Rect)(ref rect4)).min;
					if (!Mathf.Approximately(num3, ((Vector2)(ref val9))[k]))
					{
						goto IL_0494;
					}
				}
				float num4 = ((Vector3)(ref val8))[k];
				val9 = ((Rect)(ref rect4)).max;
				if (!(num4 > ((Vector2)(ref val9))[k]))
				{
					continue;
				}
				float num5 = ((Vector3)(ref val8))[k];
				val9 = ((Rect)(ref rect4)).max;
				if (Mathf.Approximately(num5, ((Vector2)(ref val9))[k]))
				{
					continue;
				}
				goto IL_0494;
				IL_0494:
				flag = true;
				break;
			}
			if (flag)
			{
				RectTransformUtility.FlipLayoutOnAxis(val2, k, false, false);
			}
		}
		for (int m = 0; m < m_Items.Count; m++)
		{
			RectTransform rectTransform = m_Items[m].rectTransform;
			rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0f);
			rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0f);
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, val4.y + size.y * (float)(m_Items.Count - 1 - m) + size.y * rectTransform.pivot.y);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, size.y);
		}
		AlphaFadeList(0.15f, 0f, 1f);
		((Component)m_Template).gameObject.SetActive(false);
		((Component)componentInChildren).gameObject.SetActive(false);
		m_Blocker = CreateBlocker(val);
		UnityEvent onDropDownOpened = m_OnDropDownOpened;
		if (onDropDownOpened != null)
		{
			onDropDownOpened.Invoke();
		}
		Displayed = true;
	}

	protected virtual GameObject CreateBlocker(Canvas rootCanvas)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00ad: Expected O, but got Unknown
		GameObject val = new GameObject("Blocker");
		RectTransform obj = val.AddComponent<RectTransform>();
		((Transform)obj).SetParent(((Component)rootCanvas).transform, false);
		obj.anchorMin = Vector2.op_Implicit(Vector3.zero);
		obj.anchorMax = Vector2.op_Implicit(Vector3.one);
		obj.sizeDelta = Vector2.zero;
		Canvas obj2 = val.AddComponent<Canvas>();
		obj2.overrideSorting = true;
		Canvas component = m_Dropdown.GetComponent<Canvas>();
		obj2.sortingLayerID = component.sortingLayerID;
		obj2.sortingOrder = component.sortingOrder - 1;
		val.AddComponent<GraphicRaycaster>();
		((Graphic)val.AddComponent<Image>()).color = Color.clear;
		((UnityEvent)val.AddComponent<Button>().onClick).AddListener(new UnityAction(Hide));
		return val;
	}

	protected virtual void DestroyBlocker(GameObject blocker)
	{
		Object.Destroy((Object)(object)blocker);
	}

	protected virtual GameObject CreateDropdownList(GameObject template)
	{
		return Object.Instantiate<GameObject>(template);
	}

	protected virtual void DestroyDropdownList(GameObject dropdownList)
	{
		Object.Destroy((Object)(object)dropdownList);
	}

	protected virtual DropdownItem CreateItem(DropdownItem itemTemplate)
	{
		return Object.Instantiate<DropdownItem>(itemTemplate);
	}

	protected virtual void DestroyItem(DropdownItem item)
	{
	}

	protected virtual DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
	{
		DropdownItem dropdownItem = CreateItem(itemTemplate);
		((Transform)dropdownItem.rectTransform).SetParent(((Transform)itemTemplate.rectTransform).parent, false);
		((Component)dropdownItem).gameObject.SetActive(true);
		((Object)((Component)dropdownItem).gameObject).name = "Item " + items.Count + ((data.text != null) ? (": " + data.text) : "");
		if ((Object)(object)dropdownItem.toggle != (Object)null)
		{
			dropdownItem.toggle.isOn = false;
		}
		if (Object.op_Implicit((Object)(object)dropdownItem.text))
		{
			dropdownItem.text.text = data.text;
		}
		if (Object.op_Implicit((Object)(object)dropdownItem.image))
		{
			dropdownItem.image.sprite = data.image;
			((Behaviour)dropdownItem.image).enabled = (Object)(object)dropdownItem.image.sprite != (Object)null;
		}
		items.Add(dropdownItem);
		return dropdownItem;
	}

	private void AlphaFadeList(float duration, float alpha)
	{
		CanvasGroup component = m_Dropdown.GetComponent<CanvasGroup>();
		AlphaFadeList(duration, component.alpha, alpha);
	}

	private void AlphaFadeList(float duration, float start, float end)
	{
		if (!end.Equals(start))
		{
			FloatTween floatTween = default(FloatTween);
			floatTween.Duration = duration;
			floatTween.startValue = start;
			floatTween.targetValue = end;
			FloatTween info = floatTween;
			info.AddOnChangedCallback(SetAlpha);
			info.IgnoreTimeScale = true;
			m_AlphaTweenRunner.StartTween(info);
		}
	}

	private void SetAlpha(float alpha)
	{
		if (Object.op_Implicit((Object)(object)m_Dropdown))
		{
			m_Dropdown.GetComponent<CanvasGroup>().alpha = alpha;
		}
	}

	public void Hide()
	{
		if ((Object)(object)m_Dropdown != (Object)null)
		{
			AlphaFadeList(0.15f, 0f);
			if (((UIBehaviour)this).IsActive())
			{
				((MonoBehaviour)this).StartCoroutine(DelayedDestroyDropdownList(0.15f));
			}
		}
		if ((Object)(object)m_Blocker != (Object)null)
		{
			DestroyBlocker(m_Blocker);
		}
		m_Blocker = null;
		((Selectable)this).Select();
		UnityEvent onDropDownClosed = OnDropDownClosed;
		if (onDropDownClosed != null)
		{
			onDropDownClosed.Invoke();
		}
		Displayed = false;
	}

	private IEnumerator DelayedDestroyDropdownList(float delay)
	{
		yield return (object)new WaitForSecondsRealtime(delay);
		ImmediateDestroyDropdownList();
	}

	private void ImmediateDestroyDropdownList()
	{
		for (int i = 0; i < m_Items.Count; i++)
		{
			if ((Object)(object)m_Items[i] != (Object)null)
			{
				DestroyItem(m_Items[i]);
			}
		}
		m_Items.Clear();
		if ((Object)(object)m_Dropdown != (Object)null)
		{
			DestroyDropdownList(m_Dropdown);
		}
		m_Dropdown = null;
	}

	private void OnSelectItem(Toggle toggle)
	{
		if (!toggle.isOn)
		{
			toggle.isOn = true;
		}
		int num = -1;
		Transform transform = ((Component)toggle).transform;
		Transform parent = transform.parent;
		for (int i = 0; i < parent.childCount; i++)
		{
			if ((Object)(object)parent.GetChild(i) == (Object)(object)transform)
			{
				num = i - 1;
				break;
			}
		}
		if (num < 0)
		{
			return;
		}
		value = num;
		Hide();
		if (autoDeselection)
		{
			EventSystem current = EventSystem.current;
			if (current != null)
			{
				current.SetSelectedGameObject((GameObject)null);
			}
		}
	}
}
