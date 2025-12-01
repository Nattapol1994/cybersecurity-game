using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class PassphraseFormingMicrogame : BaseMicrogame
{
    [Header("UI References")]
    [SerializeField] private Transform selectableWordsContainer;
    [SerializeField] private Transform selectedWordsContainer;
    [SerializeField] private GameObject wordButtonPrefab;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button submitButton;

    private List<string> selectedFragments = new();
    private List<FragmentData> fragmentPool = new();
    private List<string> badFragments = new();

    //private int minLength = 8;
    private int targetLength = 8;

    public override void Initialize(float difficulty = 1f)
    {
        // Scale password length
        targetLength = Mathf.RoundToInt(Mathf.Lerp(8, 20, (difficulty - 0.5f) / 1.5f));
        baseTime = 20;

        LoadFragments();
        SpawnWordButtons();

        submitButton.onClick.AddListener(OnSubmit);
        feedbackText.text = "Assemble a strong password and press Submit!";
    }

    private List<FragmentData> GenerateFragments()
    {
        const string LOWER = "abcdefghijklmnopqrstuvwxyz";
        const string UPPER = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string NUMBERS = "0123456789";
        const string SPECIALS = "!@#$%&*?";

        List<FragmentData> fragments = new()
        {
          // --- STEP 1: Ensure required diversity ---
          new FragmentData(RandomFragmentFrom(UPPER), false),   // At least one capital
          new FragmentData(RandomFragmentFrom(NUMBERS), false), // At least one number
          new FragmentData(RandomFragmentFrom(SPECIALS), false)// At least one special
        };

        // --- STEP 2: Generate remaining "safe" ones (mix of types) ---
        while (fragments.Count < 10)
        {
            float r = Random.value;
            string chars;
            if (r < 0.5f) chars = LOWER;
            else if (r < 0.7f) chars = UPPER;
            else if (r < 0.9f) chars = NUMBERS;
            else chars = SPECIALS;

            fragments.Add(new FragmentData(RandomFragmentFrom(chars), false));
        }

        // --- STEP 3: Generate "unsafe" (polluting) ones ---
        // Mostly lowercase or common bad fragments
        // while (fragments.Count < 10)
        // {
        //     string frag;
        //     float r = Random.value;

        //     // Some chance for lower-only, some chance for common bad patterns
        //     // Example common bad patterns - can be expanded later
        //         string[] commonBad = { "123", "abc", "pass", "qwe", "000", "111", "xyz" };
        //         frag = commonBad[Random.Range(0, commonBad.Length)];
            

        //     fragments.Add(new FragmentData(frag, true));
        // }

        // --- STEP 4: Shuffle for randomness ---
        return fragments.OrderBy(_ => Random.value).ToList();
    }

    // Helper: generate a 2–3 character fragment
    private string RandomFragmentFrom(string source)
    {
        int len = Random.Range(2, 4);
        char[] chars = new char[len];
        for (int i = 0; i < len; i++)
            chars[i] = source[Random.Range(0, source.Length)];
        return new string(chars);
    }

    private void LoadFragments()
    {
        // Example — replace with JSON or real config
        fragmentPool = GenerateFragments();

        badFragments = fragmentPool.Where(f => f.isBad).Select(f => f.text).ToList();
    }

    private void SpawnWordButtons()
    {
        foreach (Transform child in selectableWordsContainer)
            Destroy(child.gameObject);

        foreach (var frag in fragmentPool)
        {
            var btnObj = Instantiate(wordButtonPrefab, selectableWordsContainer);
            var text = btnObj.GetComponentInChildren<TMP_Text>();
            text.text = frag.text;

            var button = btnObj.GetComponent<Button>();
            button.onClick.AddListener(() => ToggleFragment(frag.text, btnObj));
        }
    }

    private void ToggleFragment(string frag, GameObject btnObj)
    {
        bool alreadySelected = selectedFragments.Contains(frag);

        if (alreadySelected)
        {
            selectedFragments.Remove(frag);
            btnObj.transform.SetParent(selectableWordsContainer);
        }
        else
        {
            selectedFragments.Add(frag);
            btnObj.transform.SetParent(selectedWordsContainer);
        }

        UpdateFeedback();
    }

    private void UpdateFeedback()
    {
        string assembled = string.Join("", selectedFragments);
        var (valid, message) = ValidatePassword(assembled);
        feedbackText.text = message;
    }

    private (bool, string) ValidatePassword(string pass)
    {
        if (badFragments.Any(bad => pass.Contains(bad)))
            return (false, "Contains unsafe fragment!");
        if (pass.Length < targetLength)
            return (false, $"Too short! ({pass.Length}/{targetLength})");
        if (!pass.Any(char.IsUpper))
            return (false, "Needs a capital letter!");
        if (!pass.Any(char.IsLower))
            return (false, "Needs a lowercase letter!");
        if (!pass.Any(char.IsDigit))
            return (false, "Needs a number!");
        if (!pass.Any(c => "!@#$%^&*".Contains(c)))
            return (false, "Needs a special symbol!");

        return (true, "Looks strong!");
    }

    private void OnSubmit()
    {
        string pass = string.Join("", selectedFragments);
        var (valid, message) = ValidatePassword(pass);

        if (valid)
            MicrogameSuccess("Nice! Strong passphrase!");
        else
            MicrogameFailure("Failure! Passphrase too weak!");
    }

    protected override void Cleanup()
    {
        submitButton.onClick.RemoveAllListeners();
    }
}

[System.Serializable]
public class FragmentData
{
    public string text;
    public bool isBad;
    public FragmentData(string t, bool bad) { text = t; isBad = bad; }
}
