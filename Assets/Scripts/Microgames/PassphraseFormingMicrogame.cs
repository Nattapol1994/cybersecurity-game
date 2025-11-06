using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class PassphraseFormingMicrogame : BaseMicrogame
{
    [Header("UI References")]
    [SerializeField] public Transform buttonContainer;
    [SerializeField] public GameObject wordButtonPrefab;
    [SerializeField] public TMP_Text feedbackText;

    private WordRelationList configData;
    private List<WordRelation> chosenWords = new();
    private WordRelation currentWord;
    private int requiredWordCount = 4;

    public override void Initialize(float difficulty = 1f)
    {
        // Load from JSON (assuming the file is placed in StreamingAssets/DataConfig/)
        configData = ConfigLoader.LoadConfig<WordRelationList>("passphrase_config.json");

        if (configData == null || configData.words.Count == 0)
        {
            Debug.LogError("Failed to load passphrase config or no words found!");
            return;
        }

        requiredWordCount = Mathf.Clamp(5 + Mathf.FloorToInt((difficulty - 1f) / 0.5f), 3, 8);
        chosenWords.Clear();

        currentWord = configData.words[Random.Range(0, configData.words.Count)];
        chosenWords.Add(currentWord);

        feedbackText.text = $"Start: {currentWord.word}";
        GenerateWordChoices();
    }

    void GenerateWordChoices()
    {
        // Clear old buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Include all unrelated words as valid choices
        var validChoices = configData.words
            .Where(w => !currentWord.related.Contains(w.word) && w.word != currentWord.word)
            .OrderBy(_ => Random.value)
            .Take(1) // at least one correct option
            .ToList();

        // Fill with distractors (related words) for challenge
        var distractors = configData.words
            .Where(w => currentWord.related.Contains(w.word) || w.word == currentWord.word)
            .OrderBy(_ => Random.value)
            .Take(3)
            .ToList();

        var allOptions = validChoices.Concat(distractors).OrderBy(_ => Random.value).ToList();

        foreach (var w in allOptions)
        {
            var btnObj = Instantiate(wordButtonPrefab, buttonContainer);
            btnObj.GetComponentInChildren<TMP_Text>().text = w.word;
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnWordClicked(w));
        }

        feedbackText.text =
            $"Passphrase: {string.Join(" ", chosenWords.Select(w => w.word))}\n" +
            $"({chosenWords.Count}/{requiredWordCount})";
    }

    void OnWordClicked(WordRelation selected)
    {
        // Fail if the player picked a word that is related or the same as current
        if (currentWord.related.Contains(selected.word) || selected.word == currentWord.word)
        {
            MicrogameFailure();
            return;
        }

        // Otherwise, append word and continue
        chosenWords.Add(selected);
        currentWord = selected;

        if (chosenWords.Count >= requiredWordCount)
            MicrogameSuccess();
        else
            GenerateWordChoices();
    }

    protected override void Cleanup()
    {
        foreach (Transform btn in buttonContainer)
            Destroy(btn.gameObject);
    }
}
