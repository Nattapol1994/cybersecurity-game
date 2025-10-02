using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextUtils;
public class WifiSelectionMicrogame : BaseMicrogame
{
    public GameObject buttonPrefab; // assign a simple TMP button prefab here
    public int totalOptions = 3;
    public List<string> wifiNames = new List<string>() { "Home_Network", "OfficeWiFi", "Cafe_WiFi" }; // add more names as desired. 
    // TODO: separate into a config file
    public float spreadX = 300f; // horizontal spacing
    public float spreadY = 100f; // vertical spacing

    private List<GameObject> spawnedButtons = new List<GameObject>();
    public Transform buttonContainer;
    private bool initialized = false;

    public override void Initialize(float difficulty = 1f)
    {
        if (initialized) return;
        initialized = true;

        GenerateWifiOptions(difficulty);
    }

    void GenerateWifiOptions(float difficulty = 1f)
{
    // Pick a correct SSID
    string correctSSID = wifiNames[Random.Range(0, wifiNames.Count)];

    // Decide how many fakes to generate:
    // At least 4, plus +1 fake per 0.5 difficulty above 1
    int fakeCount = 4 + Mathf.FloorToInt((difficulty - 1f) / 0.5f);
    fakeCount = Mathf.Max(fakeCount, 4); // ensure minimum 4

    // Generate fake SSIDs
    List<string> options = new List<string> { correctSSID };
    int attempts = 0;
    while (options.Count < fakeCount + 1 && attempts < 200)
    {
        string fake = ScrambleSSID(correctSSID);
        if (!options.Contains(fake))
            options.Add(fake);
        attempts++;
    }

    // Shuffle options
        for (int i = 0; i < options.Count; i++)
        {
            int r = Random.Range(i, options.Count);
            (options[i], options[r]) = (options[r], options[i]);
        }

    // Create instruction text showing the correct SSID
    GameObject instructionObj = new GameObject("CorrectSSIDLabel", typeof(RectTransform));
    instructionObj.transform.SetParent(transform, false);
    var tmp = instructionObj.AddComponent<TMPro.TextMeshProUGUI>();
    tmp.text = $"Connect to: {correctSSID}";
    tmp.fontSize = 36;
    tmp.alignment = TMPro.TextAlignmentOptions.Center;
    tmp.rectTransform.anchoredPosition = new Vector2(0, 200f); // position at top center

    // Instantiate buttons vertically on the left side
    for (int i = 0; i < options.Count; i++)
    {
        GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
        btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = options[i];

        // vertical layout: stacked downward
        float yPos = 100f - (i * spreadY);
        btnObj.transform.localPosition = new Vector3(-300f, yPos, 0); // offset to the left

        Button btn = btnObj.GetComponent<Button>();
        string ssidCopy = options[i]; // capture for closure
        btn.onClick.AddListener(() => OnButtonClicked(ssidCopy == correctSSID));

        spawnedButtons.Add(btnObj);
    }
}

    void OnButtonClicked(bool correct)
    {
        if (correct)
            MicrogameSuccess();
        else
            MicrogameFailure();
        // hide the microgame
            foreach (var btn in spawnedButtons)
                Destroy(btn);
        gameObject.SetActive(false);
    }

    protected override void OnTimeout()
    {
        base.OnTimeout();  
        // Time's up, treat as failure

        // Clean up dynamically spawned buttons
        foreach (var btn in spawnedButtons)
            Destroy(btn);
        spawnedButtons.Clear();

        // Hide this microgame panel
        gameObject.SetActive(false);
    }
}
