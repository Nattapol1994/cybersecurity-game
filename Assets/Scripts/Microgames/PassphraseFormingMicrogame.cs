using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class PassphraseFormingMicrogame : BaseMicrogame
{
    [Header("UI References")]
    [SerializeField] public Transform selectableWordsContainer;
    [SerializeField] public Transform selectedWordsContainer;
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

        // Always at least 3 words, scale with difficulty above that
        requiredWordCount = Mathf.Max(4, Mathf.Clamp(5 + Mathf.FloorToInt((difficulty - 1f) / 0.5f), 3, 8));

        chosenWords.Clear();
        currentWord = configData.words[Random.Range(0, configData.words.Count)];
        chosenWords.Add(currentWord);

        var btnObj = Instantiate(wordButtonPrefab, selectedWordsContainer);
        btnObj.GetComponentInChildren<TMP_Text>().text = currentWord.word;

        GenerateWordChoices();
    }

    void GenerateWordChoices()
    {
        // Clear old buttons
        foreach (Transform child in selectableWordsContainer)
            Destroy(child.gameObject);

        // Ensure we have at least one *valid* (unrelated) and some distractors (related)
        var validChoices = configData.words
            .Where(w => !currentWord.related.Contains(w.word) && w.word != currentWord.word)
            .OrderBy(_ => Random.value)
            .Take(2) // always have at least two unrelated choices
            .ToList();

        var distractors = configData.words
            .Where(w => currentWord.related.Contains(w.word) || w.word == currentWord.word)
            .OrderBy(_ => Random.value)
            .Take(2)
            .ToList();

        var allOptions = validChoices.Concat(distractors)
            .OrderBy(_ => Random.value)
            .ToList();

        foreach (var w in allOptions)
        {
            var btnObj = Instantiate(wordButtonPrefab, selectableWordsContainer);
            btnObj.GetComponentInChildren<TMP_Text>().text = w.word;

            var button = btnObj.GetComponent<Button>();
            button.onClick.AddListener(() => OnWordClicked(w, btnObj));
        }

        feedbackText.text =
            $"Select {requiredWordCount - chosenWords.Count} more words!";
    }

    IEnumerator DisableWrongChoice(GameObject buttonObj, float duration = 0.2f)
    {
        var button = buttonObj.GetComponent<Button>();
        var img = buttonObj.GetComponent<Image>();
        var txt = buttonObj.GetComponentInChildren<TMP_Text>();

        if (img == null) yield break;

        Color originalColor = img.color;
        Color targetColor = Color.red;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            img.color = Color.Lerp(originalColor, targetColor, Mathf.PingPong(t * 5f, 1f));
            yield return null;
        }

        // Disable interaction and set final grey
        if (button) button.interactable = false;
        img.color = Color.gray;
        if (txt) txt.alpha = 0.5f;
    }

    void OnWordClicked(WordRelation selected, GameObject buttonObj)
    {
        // If player picked a related or same word → remove that button only
        if (currentWord.related.Contains(selected.word) || selected.word == currentWord.word)
        {
            // Visual + text feedback
            feedbackText.text = $"'{selected.word}' is too similar! Try another.";
            StartCoroutine(DisableWrongChoice(buttonObj));
            return; // let player continue
        }

        // Otherwise, it's valid — continue chain
        var btnObj = Instantiate(wordButtonPrefab, selectedWordsContainer);
        btnObj.GetComponentInChildren<TMP_Text>().text = selected.word;
        chosenWords.Add(selected);
        currentWord = selected;

        feedbackText.text = $"Select {requiredWordCount - chosenWords.Count} more words!";

        if (chosenWords.Count >= requiredWordCount)
        {
            MicrogameSuccess("SUCCESS! You formed a passphrase just in time.");
        }
        else
        {
            GenerateWordChoices();
        }
    }

    protected override void Cleanup()
    {
        foreach (Transform btn in selectableWordsContainer)
            Destroy(btn.gameObject);
    }
}
