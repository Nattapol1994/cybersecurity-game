using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class EmailSegment
{
    [TextArea] public string text;
    public bool isPhishing;
}

[System.Serializable]
public class EmailData
{
    public string title;
    [TextArea] public string explanationCorrect;
    [TextArea] public string explanationWrong;
    public EmailSegment[] segments;
}

public class PhishingGameController : BaseMicrogame
{
    [Header("Game Data")]
    public EmailData[] emails;
    public float roundDuration = 30f;

    [Header("UI References")]
    public Image timerFill;
    public TextMeshProUGUI titleText;
    public Transform emailContentParent;   // EmailContent object
    public EmailSegmentUI segmentPrefab;

    [Header("Popup UI")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupTitleText;
    public TextMeshProUGUI popupBodyText;

    private int currentEmailIndex = 0;
    private bool roundActive = false;
    private readonly List<EmailSegmentUI> segmentInstances = new List<EmailSegmentUI>();

    public override void Initialize(float difficulty = 1f)
    {
        // Could adjust roundDuration or other parameters based on difficulty
        LoadEmail(Random.Range(0, emails.Length));
        baseTime = roundDuration;
    }

    private void LoadEmail(int index)
    {
        if (emails == null || emails.Length == 0) return;

        currentEmailIndex = Mathf.Clamp(index, 0, emails.Length - 1);
        var data = emails[currentEmailIndex];

        // UI title
        if (titleText != null)
            titleText.text = data.title;

        // Clear old segments
        foreach (Transform child in emailContentParent)
            Destroy(child.gameObject);
        segmentInstances.Clear();

        // Spawn segments
        foreach (var seg in data.segments)
        {
            var instance = Instantiate(segmentPrefab, emailContentParent);
            instance.Init(seg.text, seg.isPhishing);
            segmentInstances.Add(instance);
        }

        // Reset timer and state
        timer = roundDuration;
        roundActive = true;

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void OnSubmitButton()
    {
        Evaluate(fromTimeout: false);
    }

    private void Evaluate(bool fromTimeout)
    {
        if (!roundActive) return;
        roundActive = false;

        bool allCorrect = true;

        foreach (var segUI in segmentInstances)
        {
            // must select phishing and not select safe segments
            if (segUI.IsPhishing != segUI.IsSelected)
            {
                allCorrect = false;
                break;
            }
        }

        var data = emails[currentEmailIndex];

        if (allCorrect)
        {
            popupTitleText.text = "✅ Correct!";
            popupBodyText.text = data.explanationCorrect;
            //popupPanel.SetActive(true);
            MicrogameSuccess("Correctly identified phishing portions of the email!");
        }
        else
        {
            popupTitleText.text = fromTimeout ? "⏰ Time's up!" : "❌ Not quite.";
            popupBodyText.text = data.explanationWrong;
            //popupPanel.SetActive(true);
            MicrogameFailure("Failed to identify phishing portions of the email...");
        }
    }

    public void OnNextButton()
    {
        int next = (currentEmailIndex + 1) % emails.Length;
        LoadEmail(next);
    }

    protected override void Cleanup()
    {
        // Destroy dynamically spawned segments
        foreach (var seg in segmentInstances)
        {
            if (seg != null)
                Destroy(seg.gameObject);
        }

        segmentInstances.Clear();

        // Optionally hide popup if it's still visible
        if (popupPanel != null)
            popupPanel.SetActive(false);

        // Reset state
        roundActive = false;
    }
}
