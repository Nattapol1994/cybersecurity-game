using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseMicrogame : MonoBehaviour
{
    [HideInInspector] public MicrogameManager manager;

    [Header("Timing")]
    public float baseTime = 5f; // per prefab default
    public float finishTime = 0f;

    protected float timer;
    protected bool running;
    public Image timerBar; // assign in prefab

    [Header("End Delay")]
    [SerializeField] public float endDelay = 3.0f; // time to wait after success/failure

    [Header("Feedback Banner")]
    public GameObject bannerPrefab;   // assign a prefab in the Inspector (or we’ll auto-create one)
    private GameObject bannerInstance;
    protected Text bannerText;
    private CanvasGroup bannerCanvasGroup;

    private Color startColor = Color.yellow;
    private Color endColor = Color.red;

    [TextArea] public string instruction = "Perform the task!";

    public bool IsDone { get; private set; } = false;
    public bool WasSuccessful { get; private set; } = false;

    // Called after the manager is assigned
    public abstract void Initialize(float difficulty = 1f);

    protected abstract void Cleanup();

    public virtual void StartMicrogame(float timeLimit)
    {
        timer = Mathf.Max(timeLimit, baseTime);
        running = true;
    }

    protected virtual void Update()
    {
        if (!running) return;

        timer -= Time.deltaTime;
        if (timerBar != null)
        {
            timerBar.fillAmount = timer / baseTime;
            timerBar.color = Color.Lerp(endColor, startColor, timer / baseTime);
        }
        if (timer <= 0f)
            OnTimeout();
    }

    protected virtual void OnTimeout()
    {
        MicrogameFailure("TIME'S UP!");
    }

    public IEnumerator ShowResultBanner(string message, bool success)
    {
        // --- Create or assign banner instance ---
        if (bannerPrefab == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            bannerInstance = new GameObject("ResultBanner");
            bannerInstance.transform.SetParent(this.transform, false);

            Image bg = bannerInstance.AddComponent<Image>();
            bg.color = Color.black;

            RectTransform rt = bannerInstance.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.45f);
            rt.anchorMax = new Vector2(1, 0.55f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;


            GameObject textObj = new GameObject("BannerText");
            textObj.transform.SetParent(bannerInstance.transform, false);
            bannerText = textObj.AddComponent<Text>();
            bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bannerText.alignment = TextAnchor.MiddleCenter;
            bannerText.fontSize = 48;

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            bannerCanvasGroup = bannerInstance.AddComponent<CanvasGroup>();
        }
        else
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            bannerInstance = Instantiate(bannerPrefab, canvas.transform);
            bannerText = bannerInstance.GetComponentInChildren<Text>();
            bannerCanvasGroup = bannerInstance.GetComponent<CanvasGroup>();
        }

        // Set main message
        bannerText.text = message;
        bannerText.color = success ? Color.green : Color.red;

        // --- Create a score banner underneath (if score > 0) ---
        GameObject scoreBanner = null;
        CanvasGroup scoreCg = null;
        if (success)
        {
            scoreBanner = new GameObject("ScoreBanner");
            scoreBanner.transform.SetParent(this.transform, false);

            Image bg = scoreBanner.AddComponent<Image>();
            bg.color = Color.black;

            RectTransform rt = scoreBanner.GetComponent<RectTransform>();
            // Anchor below main banner
            rt.anchorMin = new Vector2(0.4f, 0.35f);
            rt.anchorMax = new Vector2(0.6f, 0.45f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            GameObject textObj = new GameObject("ScoreText");
            textObj.transform.SetParent(scoreBanner.transform, false);
            Text scoreText = textObj.AddComponent<Text>();
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.fontSize = 48;
            scoreText.color = Color.yellow;
            scoreText.text = $"Time Bonus: +{(int)(1000 * (finishTime / baseTime))}";

            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            scoreCg = scoreBanner.AddComponent<CanvasGroup>();
            scoreCg.alpha = 1f;
        }

        // --- Fade timings ---
        float holdTime = endDelay * 0.8f;
        float fadeTime = endDelay * 0.2f;

        // Hold main banner fully visible
        bannerCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(holdTime);

        // Fade out main banner
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bannerCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            if (scoreCg != null) scoreCg.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        bannerCanvasGroup.alpha = 0f;
        if (scoreCg != null) scoreCg.alpha = 0f;

        // Destroy both banners
        Destroy(bannerInstance);
        if (scoreBanner != null) Destroy(scoreBanner);
    }

    public void ReduceTimer(float amount)
    {
        timer -= amount;
        if (timer < 0) timer = 0;
    }

    public void MicrogameSuccess(string message)
    {
        if (IsDone) return; // prevent double calls
            HandleMicrogameEnd(success: true, message);
    }

    public void MicrogameFailure(string message)
    {
        if (IsDone) return; // prevent double calls
            HandleMicrogameEnd(success: false, message);
    }

    private void HandleMicrogameEnd(bool success, string message)
    {
        running = false;
        IsDone = true;
        WasSuccessful = success;
        finishTime = timer;

        // Stop timer visuals and freeze UI here
        // timerBar.enabled = false;
        StartCoroutine(ShowResultBanner(message, success));
    }
}
