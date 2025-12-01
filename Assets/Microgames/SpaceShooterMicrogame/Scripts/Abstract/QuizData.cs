using System;
using UnityEngine;

[Serializable]
public class QuizQuestion
{
    public string id;
    [TextArea] public string prompt;
    public string[] choices;        
    public int correctIndex;     
    public float questionDisplayTime = 5f;
    public float baseAnswerTime = 8f;
}

[Serializable]
public class QuizDatabase
{
    public QuizQuestion[] questions;
}

public static class QuizUtils
{
    public static QuizQuestion[] PickRandomQuestions(QuizDatabase db, int count)
    {
        if (db == null || db.questions == null || db.questions.Length == 0)
        {
            Debug.LogError("QuizDatabase is empty.");
            return Array.Empty<QuizQuestion>();
        }

        count = Mathf.Clamp(count, 1, db.questions.Length);
        QuizQuestion[] pool = (QuizQuestion[])db.questions.Clone();

        for (int i = 0; i < pool.Length; i++)
        {
            int r = UnityEngine.Random.Range(i, pool.Length);
            (pool[i], pool[r]) = (pool[r], pool[i]);
        }

        QuizQuestion[] result = new QuizQuestion[count];
        Array.Copy(pool, result, count);
        return result;
    }
}
