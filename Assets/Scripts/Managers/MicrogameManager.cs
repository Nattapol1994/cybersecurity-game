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
    private bool success;
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
            // pick random microgame prefab
            var prefab = microgames[Random.Range(0, microgames.Count)];

            // Show instruction for 1 sec
            instructionText.text = prefab.GetComponent<BaseMicrogame>()?.instruction ?? "GO!";
            instructionText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            instructionText.gameObject.SetActive(false);

            // Find the Canvas 
            Canvas canvas = FindObjectOfType<Canvas>();

            // Spawn under the Canvas instead of world space
            currentMicrogame = Instantiate(prefab, canvas.transform);

            // Make sure it’s active
            currentMicrogame.SetActive(true);

            // assign manager and initialize via base class
            BaseMicrogame microgame = currentMicrogame.GetComponent<BaseMicrogame>();
            if (microgame != null)
            {
                microgame.manager = this;
                microgame.Initialize(difficulty); // safe, polymorphic call
            }

            // reset state
            success = false;
            float timer = baseMicrogameTime / difficulty;
            timer = Mathf.Clamp(timer, 1f, baseMicrogameTime); // limit min time

            difficulty = Mathf.Clamp(difficulty, minDifficulty, maxDifficulty);
            difficultyText.text = $"Difficulty: {difficulty:F1}";

            // wait for player input or timeout
            while (timer > 0 && !success)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            // outcome
            if (success)
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

            UpdateUI();

            Destroy(currentMicrogame);
            roundNumber++;

            yield return new WaitForSeconds(0.5f);
        }

        // game over
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

    // Called by prefab when clicked
    public void MicrogameSuccess()
    {
        success = true;
    }

    public void MicrogameFailure()
    {
        success = false;
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
