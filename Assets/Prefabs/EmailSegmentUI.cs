using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmailSegmentUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image background;

    private bool isPhishing;
    private bool isSelected;

    public bool IsPhishing => isPhishing;
    public bool IsSelected => isSelected;

    private Color normalColor = new Color(0, 0, 0, 0);      // transparent
    private Color selectedColor = new Color(1f, 0.9f, 0.5f); // light yellow

    public void Init(string text, bool phishing)
    {
        label.text = text;
        isPhishing = phishing;
        SetSelected(false);

        // Ensure the button click calls ToggleSelect
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(ToggleSelect);
    }

    private void ToggleSelect()
    {
        SetSelected(!isSelected);
    }

    private void SetSelected(bool value)
    {
        isSelected = value;
        if (background != null)
            background.color = isSelected ? selectedColor : normalColor;
    }
}
