using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SafeLinkMicrogame : BaseMicrogame
{
    public Button[] linkButtons;      // Assign 5 buttons in Inspector
    public TMP_Text questionText;     // Text at the top: "Which link is safe?"

    private string correctLink;

    private List<string> thaiSites = new List<string> {
        "www.google.com",
        "www.youtube.com",
        "www.facebook.com",
        "www.pantip.com",
        "www.sanook.com",
        "www.kapook.com",
        "www.dek-d.com",
        "www.trueid.net",
        "www.kaidee.com",
        "www.thairath.co.th"
    };

    // If you are NOT using a MicrogameManager yet, this lets it run standalone
    void Start()
    {
        if (manager == null)
        {
            Initialize(1f);   // difficulty = 1 (normal)
        }
    }

    // Required by BaseMicrogame
    public override void Initialize(float difficulty = 1f)
    {
        // Optionally use difficulty to adjust baseTime (easier → more time)
        baseTime = Mathf.Lerp(15f, 7f, difficulty);  // from 15s down to 7s

        if (questionText != null)
        {
            questionText.text = "Which link is safe?";
        }

        SetupRound();
        StartMicrogame(baseTime);   // this starts BaseMicrogame's timer
    }

    // Required by BaseMicrogame (cleanup when finished)
    protected override void Cleanup()
    {
        if (linkButtons == null) return;
        foreach (var btn in linkButtons)
        {
            if (btn != null)
                btn.onClick.RemoveAllListeners();
        }
    }

    private void SetupRound()
    {
        if (linkButtons == null || linkButtons.Length == 0) return;

        // 1) Pick one real Thai site
        correctLink = thaiSites[Random.Range(0, thaiSites.Count)];

        // 2) Generate 4 fake links
        List<string> fakeLinks = GenerateFakeLinks(correctLink);

        // 3) Combine 1 real + 4 fake & shuffle
        List<string> allLinks = new List<string>();
        allLinks.Add(correctLink);
        allLinks.AddRange(fakeLinks);
        allLinks = allLinks.OrderBy(x => Random.value).ToList();

        // 4) Assign to buttons
        for (int i = 0; i < linkButtons.Length && i < allLinks.Count; i++)
        {
            var btn = linkButtons[i];
            if (btn == null) continue;

            string thisLink = allLinks[i];

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = thisLink;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnLinkClick(thisLink));
        }
    }

    private void OnLinkClick(string clickedLink)
    {
        if (!running) return;    // ignore clicks after game ended/time up

        if (clickedLink == correctLink)
        {
            string msg = "Correct! " + correctLink + " is the real website.";

            // Show on the big yellow text
            if (questionText != null)
                questionText.text = msg;

            running = false;          // stop accepting answers
            MicrogameSuccess(msg);    // still call base if you want timer/manager logic
        }
        else
        {
            string reason = GetFakeReason(clickedLink, correctLink);
            string msg =
                "Wrong link!\n" +
                clickedLink +
                "\nReason: " + reason;

            // Show on the big yellow text
            if (questionText != null)
                questionText.text = msg;

            running = false;           // stop accepting answers
            MicrogameFailure(msg);     // optional, for timer/manager
        }
    }



    private List<string> GenerateFakeLinks(string realLink)
    {
        var fakes = new List<string>();
        string baseName = realLink.Replace("www.", "").Split('.')[0];

        // Double a middle letter (misspelling)
        if (baseName.Length >= 3)
        {
            char c = baseName[1];
            fakes.Add("www." + baseName.Insert(1, c.ToString()) + ".com");
        }
        else
        {
            fakes.Add("www." + baseName + baseName + ".com");
        }

        // Extra words, wrong TLD, numbers
        fakes.Add("www." + baseName + "-login.com");
        fakes.Add("www." + baseName + ".co");
        fakes.Add("www." + baseName + "123.com");

        return fakes;
    }

    private string GetFakeReason(string wrong, string correct)
    {
        // Compare only the domain part (without www. and TLD)
        string correctBase = correct.Replace("www.", "").Split('.')[0];
        string wrongBase = wrong.Replace("www.", "").Split('.')[0];

        // 1) Number instead of letter (g00gle)
        if (wrong.Contains("0") && correct.Contains("o"))
            return "Numbers are used instead of letters (0 instead of o).";

        // 2) Extra words like -login, -secure
        if (wrong.Contains("-login") || wrong.Contains("-secure") || wrong.Contains("-verify"))
            return "Extra words (login/secure/verify) were added to the domain.";

        // 3) Wrong or unusual TLD
        if (wrong.EndsWith(".co") || wrong.EndsWith(".xyz") || wrong.EndsWith(".top"))
            return "The website ends with a different or uncommon domain (.co, .xyz, etc.).";

        // 4) Random numbers at the end
        if (wrongBase != correctBase && wrongBase.StartsWith(correctBase) && wrongBase.Any(char.IsDigit))
            return "Random numbers were added to the end of the name.";

        // 5) Extra / duplicated letters (gooogle, goggle, etc.)
        if (wrongBase.Length != correctBase.Length)
            return "The spelling of the website name is different (extra or missing letters).";

        // Fallback
        return "The spelling or structure does not match the real website.";
    }

}
