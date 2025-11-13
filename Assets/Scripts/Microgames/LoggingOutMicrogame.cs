using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class LogoutMicrogame : BaseMicrogame
{
    [System.Serializable]
    public class FakeApp
    {
        public string appName;
        public Color themeColor;
    }

    [Header("Prefabs & Settings")]
    [SerializeField] private List<GameObject> windowPrefabs;

    private readonly List<GameObject> activeWindows = new();

    public override void Initialize(float difficulty = 1f)
    {
        float normalized = Mathf.InverseLerp(0.5f, 2f, difficulty);
        int windowCount = Mathf.RoundToInt(Mathf.Lerp(3, 6, normalized));
        SpawnMultipleWindows(windowPrefabs, windowCount);
    }

   void SpawnMultipleWindows(List<GameObject> prefabs, int count = 3)
{
    List<GameObject> availablePrefabs = new List<GameObject>(prefabs);

    // Shuffle
    for (int i = availablePrefabs.Count - 1; i > 0; i--)
    {
        int j = Random.Range(0, i + 1);
        var temp = availablePrefabs[i];
        availablePrefabs[i] = availablePrefabs[j];
        availablePrefabs[j] = temp;
    }

    int spawnCount = Mathf.Min(count, availablePrefabs.Count);

    for (int i = 0; i < spawnCount; i++)
    {
        SpawnAppWindow(availablePrefabs[i]);
    }
}

    void SpawnAppWindow(GameObject windowPrefab)
    {
        GameObject window = Instantiate(windowPrefab, transform);
        var rect = window.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();


        RectTransform visualRect = window.transform.Find("Background")?.GetComponent<RectTransform>() ?? rect;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(window.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(visualRect);

        // Sizes
        Vector2 windowSize = visualRect.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        // Safe bounds
        float minX = -canvasSize.x / 2 + windowSize.x / 2;
        float maxX =  canvasSize.x / 2 - windowSize.x / 2;
        float minY = -canvasSize.y / 2 + windowSize.y / 2;
        float maxY =  canvasSize.y / 2 - windowSize.y / 2;

        // Random position
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        Vector2 pos = new Vector2(x, y);
        rect.anchoredPosition = pos;

        var logoutButton = window.transform.Find("LogOutButton")?.GetComponent<Button>();
        if (logoutButton)
            logoutButton.onClick.AddListener(() => OnLogoutClicked(window));

        activeWindows.Add(window);
    }

    protected override void Cleanup()
    {
        foreach (var window in activeWindows)
            Destroy(window);
        activeWindows.Clear();
    }

    void OnLogoutClicked(GameObject window)
    {
        activeWindows.Remove(window);
        Destroy(window);

        if (activeWindows.Count == 0)
            MicrogameSuccess("SUCCESS! You logged out of all windows.");
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
