using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LogoutMicrogame : BaseMicrogame
{
    [System.Serializable]
    public class FakeApp
    {
        public string appName;
        public Color themeColor;
    }

    [Header("Prefabs & Settings")]
    [SerializeField] private GameObject windowPrefab;
    [SerializeField] private List<FakeApp> fakeApps = new();

    private readonly List<GameObject> activeWindows = new();

    public override void Initialize(float difficulty = 1f)
    {
        Cleanup();

        int windowCount = Mathf.RoundToInt(Mathf.Lerp(3, 6, difficulty));
        var apps = new List<FakeApp>(fakeApps);
        Shuffle(apps);

        for (int i = 0; i < windowCount && i < apps.Count; i++)
            SpawnAppWindow(apps[i]);
    }

    void SpawnAppWindow(FakeApp app)
    {
        GameObject window = Instantiate(windowPrefab, transform);
        var rect = window.GetComponent<RectTransform>();

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition = new Vector2(
            Random.Range(-250f, 250f),
            Random.Range(-150f, 150f)
        );

        // Apply visuals
        var bg = window.transform.Find("Background")?.GetComponent<Image>();
        if (bg) bg.color = app.themeColor;

        var nameText = window.transform.Find("AppNameText")?.GetComponent<TMP_Text>();
        if (nameText) nameText.text = app.appName;

        var logoutButton = window.transform.Find("LogOutButton")?.GetComponent<Button>();
        if (logoutButton) logoutButton.onClick.AddListener(() => OnLogoutClicked(window));

        activeWindows.Add(window);
    }

    void OnLogoutClicked(GameObject window)
    {
        activeWindows.Remove(window);
        Destroy(window);

        if (activeWindows.Count == 0)
            MicrogameSuccess();
    }

    protected override void OnTimeout()
    {
        if (activeWindows.Count > 0)
            MicrogameFailure();
    }

    protected override void Cleanup()
    {
        foreach (var w in activeWindows)
            if (w) Destroy(w);
        activeWindows.Clear();
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
