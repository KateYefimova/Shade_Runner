using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    private GameObject mainColumn;
    private GameObject settingsColumn;
    private Button settingsButton;
    private Button backButton;

    public event Action PlayClicked;
    public event Action SettingsClicked;
    public event Action BackClicked;
    public event Action QuitClicked;

    private void Awake()
    {
        mainColumn = playButton.transform.parent.gameObject;
        settingsButton = CloneButton(quitButton, mainColumn.transform, "SettingsButton", "Налаштування",
            playButton.transform.GetSiblingIndex() + 1);
        BuildSettings();

        playButton.onClick.AddListener(() => PlayClicked?.Invoke());
        settingsButton.onClick.AddListener(() => SettingsClicked?.Invoke());
        backButton.onClick.AddListener(() => BackClicked?.Invoke());
        quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
    }

    private void Start()
    {
        ShowSettings(false);
    }

    private void Update()
    {
        UiSelection.KeepFocus(settingsColumn.activeSelf ? backButton : playButton);
    }

    public void ShowSettings(bool show)
    {
        mainColumn.SetActive(!show);
        settingsColumn.SetActive(show);
        UiSelection.Focus(show ? backButton : playButton);
    }

    private void BuildSettings()
    {
        var source = (RectTransform)mainColumn.transform;
        RectTransform column = UiFactory.CreateRect("SettingsColumn", source.parent);
        column.anchorMin = source.anchorMin;
        column.anchorMax = source.anchorMax;
        column.pivot = source.pivot;
        column.anchoredPosition = source.anchoredPosition;
        column.sizeDelta = source.sizeDelta;

        VerticalLayoutGroup sourceLayout = mainColumn.GetComponent<VerticalLayoutGroup>();
        VerticalLayoutGroup layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = sourceLayout.padding;
        layout.spacing = sourceLayout.spacing;
        layout.childAlignment = sourceLayout.childAlignment;
        layout.childControlWidth = sourceLayout.childControlWidth;
        layout.childControlHeight = sourceLayout.childControlHeight;
        layout.childForceExpandWidth = sourceLayout.childForceExpandWidth;
        layout.childForceExpandHeight = sourceLayout.childForceExpandHeight;

        Text title = UiFactory.CreateText(column, "Title", 64, new Color(0.10f, 0.95f, 1f), TextAnchor.MiddleCenter);
        title.text = "НАЛАШТУВАННЯ";
        title.fontStyle = FontStyle.Bold;
        UiFactory.SetHeight(title.gameObject, 110f);

        VolumeSettingsView volume = GetComponentInChildren<VolumeSettingsView>(true);
        if (volume == null)
        {
            volume = column.gameObject.AddComponent<VolumeSettingsView>();
            column.gameObject.AddComponent<VolumeSettingsPresenter>();
        }
        volume.SetContainer(column, 1);

        backButton = CloneButton(quitButton, column, "BackButton", "Назад", -1);

        settingsColumn = column.gameObject;
    }

    private static Button CloneButton(Button source, Transform parent, string objectName, string label, int siblingIndex)
    {
        GameObject copy = Instantiate(source.gameObject, parent);
        copy.name = objectName;
        if (siblingIndex >= 0) copy.transform.SetSiblingIndex(siblingIndex);

        Text text = copy.GetComponentInChildren<Text>();
        if (text != null) text.text = label;

        Button button = copy.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        return button;
    }
}
