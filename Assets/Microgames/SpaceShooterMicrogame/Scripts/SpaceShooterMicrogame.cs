using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpaceShooterMicrogame : BaseMicrogame
{
    [Header("UI Area")]
    public RectTransform gameArea;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;

private int score = 0;

    [Header("Quiz Config")]
    public TextAsset quizJson;
    public TextMeshProUGUI questionText;

    [Header("Gameplay")]
    public UIPlayerController player;
    public UIEnemySpawner enemySpawner;

    [Tooltip("Prefab GameObject with AnswerShip + Image + RectTransform")]
    public GameObject answerShipPrefab;
    public RectTransform answerShipContainer;

    [Header("UI Timer")]
    public TextMeshProUGUI answerTimerText;

    [Header("Timing")]
    public int rounds = 2;
    [Tooltip("Delay after enemies start before answer ships appear")]
    public float preAnswerDelay = 0.5f;

    private QuizDatabase quizDB;
    private QuizQuestion[] selectedQuestions;
    private int currentRound;

    private float currentAnswerTime;
    private float currentDifficulty;

private readonly List<UIBullet> bullets = new();
private readonly List<UIEnemy> enemies = new();
private readonly List<AnswerShip> answerShips = new();

    public UIPlayerController Player => player;
    public RectTransform PlayerRect => player != null ? (RectTransform)player.transform : null;

    #region Registration
    public void RegisterBullet(UIBullet b)
    {
        if (b != null && !bullets.Contains(b))
            bullets.Add(b);
    }

    public void UnregisterBullet(UIBullet b)
    {
        bullets.Remove(b);
    }

    public void RegisterEnemy(UIEnemy e)
    {
        if (e != null && !enemies.Contains(e))
            enemies.Add(e);
    }

    public void UnregisterEnemy(UIEnemy e)
    {
        enemies.Remove(e);
    }

    public void RegisterAnswerShip(AnswerShip a)
    {
        if (a != null && !answerShips.Contains(a))
            answerShips.Add(a);
    }

    public void UnregisterAnswerShip(AnswerShip a)
    {
        answerShips.Remove(a);
    }
    #endregion

    public override void Initialize(float difficulty = 1f)
    {
        currentDifficulty = difficulty;

        // Load quiz JSON
        quizDB = JsonUtility.FromJson<QuizDatabase>(quizJson.text);
        selectedQuestions = QuizUtils.PickRandomQuestions(quizDB, rounds);

        // Player setup
        player.microgame = this;
        player.gameArea = gameArea;
        player.ResetPlayer();
        player.OnPlayerDead += HandlePlayerDead;

        // Enemy spawner
        enemySpawner.gameArea = gameArea;
        enemySpawner.microgame = this;

        if (questionText != null)
            questionText.gameObject.SetActive(false);
    }

    public override void StartMicrogame(float timeLimit)
    {
        // base handles timer bar / timeout
        base.StartMicrogame(timeLimit);
        StartCoroutine(RunMicrogameFlow());
    }

    public void AddScore(int amount)
{
    score += amount;
    if (scoreText != null)
        scoreText.text = score.ToString();

}

    private void Update()
    {
        base.Update();   // keep BaseMicrogame timer logic
        CheckBulletCollisions();
    }

    private IEnumerator RunMicrogameFlow()
    {
        for (int i = 0; i < rounds; i++)
        {
            currentRound = i;
            yield return StartCoroutine(PlayRound(selectedQuestions[i]));

            if (IsDone)   // failed or (later) microgame already ended
                yield break;
        }

        MicrogameSuccess("ALL CLEAR!");
        finishTime = baseTime; // HACK: set finishTime based on score for scoring purposes
    }

    private IEnumerator PlayRound(QuizQuestion q)
    {
        // 1. Show question
        if (questionText != null)
        {
            questionText.text = q.prompt;
            questionText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(q.questionDisplayTime);

        // 2. Hide question
        if (questionText != null)
            questionText.gameObject.SetActive(false);

        // 3. Start gameplay (player + enemies)
        player.BeginRound();
        enemySpawner.BeginSpawning(currentDifficulty);

        // 4. Delay then spawn answers
        yield return new WaitForSeconds(preAnswerDelay);
        SpawnAnswerShips(q);
        
        currentAnswerTime = Mathf.Clamp(q.baseAnswerTime / currentDifficulty, 3f, 20f);
        float answerTimer = currentAnswerTime;

        answerTimerText.gameObject.SetActive(true);
        answerTimerText.text = Mathf.Ceil(answerTimer).ToString();
        // Wait while:
        //   - we still have answer ships (round not answered)
        //   - microgame not already finished
        //   - time remains
        while (!IsDone && answerTimer > 0f && answerShips.Count > 0)
        {
            answerTimer -= Time.deltaTime;
            answerTimerText.text = Mathf.Ceil(answerTimer).ToString();
            yield return null;
        }

        if (IsDone)        // wrong, timeout, or player death
            yield break;

        if (answerShips.Count > 0 && answerTimer <= 0f)
        {
            answerTimerText.gameObject.SetActive(false);
            MicrogameFailure("Time's up!");
            yield break;
        }

        // If we reach here, player answered correctly this round.
        // Clean up for next round.
        answerTimerText.gameObject.SetActive(false);
        enemySpawner.StopSpawning();
        enemySpawner.ClearAllEnemies(enemies);
        player.ClearAllBullets();
        ClearAnswerShips();

        yield return new WaitForSeconds(0.5f);
    }

    private void SpawnAnswerShips(QuizQuestion q)
    {
        ClearAnswerShips();

        Rect r = gameArea.rect;
        float spacing = r.width / 4f;
        float startX = -spacing;         // positions ~ -spacing, 0, +spacing
        float y = r.yMax - 150f;

        for (int i = 0; i < 3; i++)
        {
            GameObject go = Instantiate(answerShipPrefab, answerShipContainer);
            var ship = go.GetComponent<AnswerShip>();
            var rt = go.GetComponent<RectTransform>();

            // Safety
            if (rt == null)
                rt = go.AddComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(startX + spacing * i, y);

            ship.gameArea = gameArea;
            ship.microgame = this;
            ship.choiceIndex = i;
            ship.Initialize(q.choices[i], i == q.correctIndex);
        }
    }

    private void ClearAnswerShips()
    {
        for (int i = answerShips.Count - 1; i >= 0; i--)
        {
            if (answerShips[i] != null)
                Destroy(answerShips[i].gameObject);
        }
        answerShips.Clear();
        answerTimerText.gameObject.SetActive(false);
    }

    // Called from AnswerShip when hit by player bullet
    public void OnAnswerShipHit(AnswerShip ship)
    {
        if (IsDone) return;

        if (ship.isCorrect)
        {
            // Correct choice for this round
            ClearAnswerShips();
            enemySpawner.StopSpawning();
            enemySpawner.ClearAllEnemies(enemies);
            player.ClearAllBullets();
            AddScore(1000);
            // Round completes when answerShips.Count == 0 in PlayRound()
        }
        else
        {
            answerTimerText.gameObject.SetActive(false);
            MicrogameFailure("Wrong answer!");
        }
    }

    // Called from AnswerShip when it reaches bottom / timeout
    public void OnAnswerShipTimeout(AnswerShip ship)
    {
        if (IsDone) return;
        answerTimerText.gameObject.SetActive(false);
        MicrogameFailure("Too late!");
    }

    private void HandlePlayerDead()
    {
        if (IsDone) return;
        answerTimerText.gameObject.SetActive(false);
        MicrogameFailure("Your ship was destroyed!");
    }

    private void CheckBulletCollisions()
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var b = bullets[i];
            if (b == null) { bullets.RemoveAt(i); continue; }

            var bRect = b.rect;
            if (bRect == null) continue;

            if (b.isPlayerBullet)
            {
                // Player bullets vs enemies
                for (int e = enemies.Count - 1; e >= 0; e--)
                {
                    var enemy = enemies[e];
                    if (enemy == null) { enemies.RemoveAt(e); continue; }

                    if (UIOverlap(bRect, enemy.rect))
                    {
                        enemy.OnHitByPlayer();
                        Destroy(b.gameObject);
                        break;
                    }
                }

                if (b == null) continue;

                // Player bullets vs answer ships
                for (int a = answerShips.Count - 1; a >= 0; a--)
                {
                    var ans = answerShips[a];
                    if (ans == null) { answerShips.RemoveAt(a); continue; }

                    if (UIOverlap(bRect, ans.rect))
                    {
                        ans.HitByPlayerBullet();
                        Destroy(b.gameObject);
                        break;
                    }
                }
            }
            else
            {
                // Enemy bullet vs player
                if (player != null && player.shipImage.enabled &&
                    UIOverlap(bRect, PlayerRect))
                {
                    player.TakeHit();
                    Destroy(b.gameObject);
                }
            }
        }
    }

    private bool UIOverlap(RectTransform a, RectTransform b)
    {
        if (a == null || b == null) return false;

        Vector3[] ac = new Vector3[4];
        Vector3[] bc = new Vector3[4];

        a.GetWorldCorners(ac);
        b.GetWorldCorners(bc);

        Rect ra = new Rect(ac[0], ac[2] - ac[0]);
        Rect rb = new Rect(bc[0], bc[2] - bc[0]);

        return ra.Overlaps(rb);
    }

    
    protected override void Cleanup()
    {
        if (player != null)
            player.OnPlayerDead -= HandlePlayerDead;

        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
            enemySpawner.ClearAllEnemies(enemies);
        }

        if (player != null)
            player.ClearAllBullets();

        ClearAnswerShips();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}
