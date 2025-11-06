using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // for UI text

public class MicrogameManager : MonoBehaviour
{
    [Header("Microgame Settings")]
    public List<GameObject> microgames; 
    public float baseMicrogameTime = 3f;    
    public int lives = 3;

    [Header("UI References")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public GameObject gameOverPanel; // assign in Inspector
    public TextMeshProUGUI difficultyText;

    [Header("Difficulty")]
    public float difficulty = 1f; // 1 = normal, >1 = harder, <1 = easier
    public float difficultyStep = 0.1f; // how much difficulty changes per round
    public float minDifficulty = 0.5f;
    public float maxDifficulty = 2f;

    private GameObject currentMicrogame;
    private int score;
    private int roundNumber;

    void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateUI();
        StartCoroutine(RunMicrogames());
    }

    IEnumerator RunMicrogames()
    {
        while (lives > 0)
        {
            // Pick random microgame prefab
            var prefab = microgames[Random.Range(0, microgames.Count)];

            // Show instruction for 1 sec
            BaseMicrogame microPrefab = prefab.GetComponent<BaseMicrogame>();
            instructionText.text = microPrefab?.instruction ?? "GO!";
            showIntermission();
            yield return new WaitForSeconds(1f);
            hideIntermission();

            // Find Canvas and spawn microgame under it
            Canvas canvas = FindFirstObjectByType<Canvas>();
            currentMicrogame = Instantiate(prefab, canvas.transform);
            currentMicrogame.transform.localPosition = Vector3.zero;
            currentMicrogame.transform.localScale = Vector3.one;
            currentMicrogame.SetActive(true);

            // Assign manager and initialize microgame
            BaseMicrogame microgame = currentMicrogame.GetComponent<BaseMicrogame>();
            if (microgame != null)
            {
                microgame.manager = this;
                microgame.Initialize(difficulty);
                microgame.StartMicrogame(microgame.baseTime / difficulty);
            }

            // Wait until microgame is done (success or timeout)
            while (!microgame.IsDone)
            {
                yield return null;
            }

            // Handle outcome
            if (microgame.WasSuccessful)   
            {
                score++;
                Debug.Log("Success!");
                difficulty += difficultyStep;
            }
            else
            {
                lives--;
                Debug.Log("Fail!");
                difficulty -= difficultyStep;
            }

            difficulty = Mathf.Clamp(difficulty, minDifficulty, maxDifficulty);
            difficultyText.text = $"Difficulty: {difficulty:F1}";
            UpdateUI();

            // Destroy microgame prefab
            Destroy(currentMicrogame);
            roundNumber++;

            yield return new WaitForSeconds(0.5f);
        }

        // Game over
        GameOver();
    }
    void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
        livesText.text = $"Lives: {lives}";
    }

    void GameOver()
    {
        instructionText.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        Debug.Log("Game Over!");
    }

    void showIntermission()
    {
        instructionText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        livesText.gameObject.SetActive(true);
        difficultyText.gameObject.SetActive(true);
    }

    void hideIntermission()
    {
        instructionText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        livesText.gameObject.SetActive(false);
        difficultyText.gameObject.SetActive(false);
    }   

    // restart button (hook up in UI)
    public void Restart()
    {
        Debug.Log("Restart clicked!");
        score = 0;
        lives = 3;
        roundNumber = 0;
        gameOverPanel.SetActive(false);
        UpdateUI();
        StopAllCoroutines();
        StartCoroutine(RunMicrogames());
    }
}
