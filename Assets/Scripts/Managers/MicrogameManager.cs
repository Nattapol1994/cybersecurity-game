using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // for UI text

public class MicrogameManager : MonoBehaviour
{
    [Header("Microgame Settings")]
    public List<GameObject> microgames; 
    public float baseMicrogameTime = 3f;    
    public int lives = 5;

    [Header("UI References")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI scoreText;
    public LifeDisplay lifeDisplay; 
    public GameObject gameOverPanel; // assign in Inspector
    public TextMeshProUGUI difficultyText;

    [Header("Difficulty")]
    public float difficulty = 1f; // 1 = normal, >1 = harder, <1 = easier
    public float difficultyStep = 0.1f; // how much difficulty changes per round
    public float minDifficulty = 0.5f;
    public float maxDifficulty = 2f;

    private GameObject currentMicrogame;
    private int score = 0;
    private int roundNumber;

    void Start()
    {
        gameOverPanel.SetActive(false);
        lifeDisplay.InitializeLives(lives);
        UpdateUI();
        StartCoroutine(RunMicrogames());
        instructionText.gameObject.SetActive(false);
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
            // Show score/lives first, then instruction
            yield return StartCoroutine(showIntermission());

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
                score += (int)(1000 * (microgame.finishTime / microgame.baseTime));
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
            UpdateUI();

            yield return new WaitForSeconds(microgame.endDelay);

            // Destroy microgame prefab
            Destroy(currentMicrogame);
            roundNumber++;

            yield return new WaitForSeconds(0f);
        }

        // Game over
        GameOver();
    }
    void UpdateUI()
    {
        scoreText.text = $"Score: {score}";
        lifeDisplay.UpdateLives(lives);
    }

    void GameOver()
    {
        instructionText.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        Debug.Log("Game Over!");
    }

    IEnumerator showIntermission()
    {
        // Show Score and Lives
        scoreText.gameObject.SetActive(true);
        lifeDisplay.gameObject.SetActive(true);

        // Wait for 1 second to show score/lives
        yield return new WaitForSeconds(1f);
        
        // Hide score and lives
        scoreText.gameObject.SetActive(false);
        lifeDisplay.gameObject.SetActive(false);

        // Show Instruction
        instructionText.gameObject.SetActive(true);

        // Wait for 1 second to show instruction
        yield return new WaitForSeconds(1f);

        // Hide all UI elements after the sequence
        instructionText.gameObject.SetActive(false);
    }

    // restart button (hook up in UI)
    public void Restart()
    {
        Debug.Log("Restart clicked!");
        score = 0;
        lives = 5;
        roundNumber = 0;
        gameOverPanel.SetActive(false);
        UpdateUI();
        StopAllCoroutines();
        StartCoroutine(RunMicrogames());
    }
}
