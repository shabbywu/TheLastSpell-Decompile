using TMPro;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.HUD.JoystickNavigation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class TutorialCategoryToggle : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public static class Constants
	{
		public const string TutorialCategoryLocalizationKeyPrefix = "TutorialCategory_";
	}

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private TextMeshProUGUI categoryText;

	[SerializeField]
	private LocalizedFont categoryTextLocalizedFont;

	[SerializeField]
	private DynamicNavigationMode dynamicNavigationMode;

	private TutorialsPanel tutorialsPanel;

	public DynamicNavigationMode DynamicNavigationMode => dynamicNavigationMode;

	public Toggle Toggle => toggle;

	public E_TutorialCategory Category { get; private set; }

	public void Init(E_TutorialCategory category, TutorialsPanel tutorialsPanel)
	{
		this.tutorialsPanel = tutorialsPanel;
		Category = category;
		((UnityEvent<bool>)(object)Toggle.onValueChanged).AddListener((UnityAction<bool>)delegate
		{
			this.tutorialsPanel.OnCategoryToggleValueChanged(this);
		});
		RefreshText();
	}

	private void OnEnable()
	{
		RefreshText();
	}

	private void RefreshText()
	{
		((TMP_Text)categoryText).text = Localizer.Get(string.Format("{0}{1}", "TutorialCategory_", Category));
		if ((Object)(object)categoryTextLocalizedFont != (Object)null)
		{
			categoryTextLocalizedFont.RefreshFont();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		tutorialsPanel.OnCategoryToggleJoystickSelected(this);
	}
}
