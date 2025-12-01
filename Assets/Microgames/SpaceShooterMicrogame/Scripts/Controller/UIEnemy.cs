using UnityEngine;
using UnityEngine.UI;

public class UIEnemy : MonoBehaviour
{
    public RectTransform rect;
    public Image shipImage;
    public float speed = 200f;        
    public int maxHP = 1;
    public AudioManager audioManager;

    [Header("Shooting")]
    public GameObject enemyBulletPrefab;      
    public RectTransform bulletContainer;
    public float fireInterval = 1.5f;

    [Header("Effects")]
    public GameObject explosionPrefab;

    [HideInInspector] public RectTransform gameArea;
    [HideInInspector] public SpaceShooterMicrogame microgame;

    private int currentHP;
    private float fireTimer;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
        if (shipImage == null)
            shipImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        if (microgame != null)
        {
            microgame.RegisterEnemy(this);
            if (gameArea == null)
                gameArea = microgame.gameArea;
        }

        currentHP = maxHP;
        fireTimer = 0f;
    }

    private void OnDisable()
    {
        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        if (microgame != null)
            microgame.UnregisterEnemy(this);
    }

    private void Update()
    {
        if (gameArea == null || rect == null) return;

        rect.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        var bounds = gameArea.rect;
        if (rect.anchoredPosition.y < bounds.yMin - 200f)
        {
            Destroy(gameObject);
            return;
        }

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            Fire();
        }
    }

    private void Fire()
    {
        if (enemyBulletPrefab == null || bulletContainer == null) return;
        if (microgame == null) return;

        GameObject go = Instantiate(enemyBulletPrefab, bulletContainer);
        var b = go.GetComponent<UIBullet>();
        var rt = go.GetComponent<RectTransform>();

        if (rt == null)
            rt = go.AddComponent<RectTransform>();

        rt.anchoredPosition = rect.anchoredPosition;

        Vector2 target = microgame.PlayerRect.anchoredPosition;
        Vector2 dir = (target - rect.anchoredPosition).normalized;

        b.isPlayerBullet = false;
        b.direction = dir;
        b.gameArea = microgame.gameArea;
        b.microgame = microgame;
    }

    public void OnHitByPlayer()
    {
        currentHP--;
        if (currentHP <= 0)
        {
            if (explosionPrefab != null && gameArea != null)
            {
                var fx = Instantiate(explosionPrefab, gameArea);
                var rt = fx.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = rect.anchoredPosition;
            }
            microgame.AddScore(100);
            audioManager.PlayExplosion();
            Destroy(gameObject);
        }
    }
}
