using System.Collections;
using System.Collections.Generic;
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
        MicrogameFailure();
    }

    public void ReduceTimer(float amount)
    {
        timer -= amount;
        if (timer < 0) timer = 0;
    }

    public void MicrogameSuccess()
    {
        running = false;
        IsDone = true;
        WasSuccessful = true;
        finishTime = timer;
    }

    public void MicrogameFailure()
    {
        running = false;
        IsDone = true;
        WasSuccessful = false;
        Cleanup();
    }
}
