using System.Collections;
using System.Collections.Generic;   
using UnityEngine;

public abstract class BaseMicrogame : MonoBehaviour
{
    [HideInInspector] public MicrogameManager manager;

    [Header("Timing")]
    public float baseTime = 5f; // per prefab default

    protected float timer;
    protected bool running;

    [TextArea] public string instruction = "Perform the task!";

    // Called after the manager is assigned
    public abstract void Initialize(float difficulty = 1f);

    public virtual void StartMicrogame(float timeLimit)
    {
        timer = Mathf.Max(timeLimit, baseTime);
        running = true;
    }

    protected virtual void Update()
    {
        if (!running) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            OnTimeout();
    }

    protected abstract void OnTimeout();
}
