using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;           
    public RectTransform enemyContainer;
    public float baseSpawnInterval = 1.0f;
    public float randomOffset = 0.3f;
    public RectTransform bulletContainer;
    public AudioManager audioManager;

    [HideInInspector] public RectTransform gameArea;
    [HideInInspector] public SpaceShooterMicrogame microgame;

    private Coroutine spawnRoutine;

    public void BeginSpawning(float difficulty)
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnerLoop(difficulty));
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    private IEnumerator SpawnerLoop(float difficulty)
    {
        if (gameArea == null && microgame != null)
            gameArea = microgame.gameArea;

        float interval = Mathf.Max(0.25f, baseSpawnInterval / difficulty);

        while (true)
        {
            SpawnEnemy();
            float t = interval + Random.Range(-randomOffset, randomOffset);
            t = Mathf.Max(0.1f, t);
            yield return new WaitForSeconds(t);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || enemyContainer == null || gameArea == null) return;

        Rect r = gameArea.rect;
        float x = Random.Range(r.xMin + 100f, r.xMax - 100f);
        float y = r.yMax + 50f;

        GameObject go = Instantiate(enemyPrefab, enemyContainer);
        var enemy = go.GetComponent<UIEnemy>();
        var rt = go.GetComponent<RectTransform>();

        if (rt == null)
            rt = go.AddComponent<RectTransform>();

        rt.anchoredPosition = new Vector2(x, y);
        enemy.bulletContainer = bulletContainer;
        enemy.audioManager = audioManager;
        enemy.microgame = microgame ?? GetComponentInParent<SpaceShooterMicrogame>();
        enemy.gameArea = gameArea;
    }

    public void ClearAllEnemies(List<UIEnemy> enemyList)
    {
        for (int i = enemyList.Count - 1; i >= 0; i--)
        {
            if (enemyList[i] != null)
                Destroy(enemyList[i].gameObject);
        }
        enemyList.Clear();
    }
}
