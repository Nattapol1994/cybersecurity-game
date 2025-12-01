using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerController : MonoBehaviour
{
    [Header("References")]
    public Image shipImage;                   
    public RectTransform rect;
    public RectTransform gameArea;
    [Header("Effects")]
    public GameObject playerExplosionPrefab; 

    [Header("Animation")]
    public Sprite[] animationFrames;      
    public float animationSpeed = 0.12f;
    private int currentFrame = 0;

    [Header("Bullets")]
    public GameObject bulletPrefab;          
    public RectTransform bulletContainer;
    public float fireRate = 0.2f;

    public AudioManager audioManager;

    [Header("Health")]
    public int maxHP = 3;
    public TMP_Text hptext;

    [Header("Movement")]
    public float moveSpeed = 2000f;  

    [HideInInspector] public SpaceShooterMicrogame microgame;
    public event Action OnPlayerDead;

    private float nextFire;
    private int currentHP;
    private bool canControl;

    private bool isInvulnerable = false;
    private float flickerDuration = 0.6f;
    private float flickerInterval = 0.08f;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
        if (shipImage == null)
            shipImage = GetComponent<Image>();
    }

    public void ResetPlayer()
    {
        currentHP = maxHP;
        UpdateHPUI();
        canControl = false;
        shipImage.enabled = true;
        isInvulnerable = false;

        if (gameArea != null)
        {
            Rect r = gameArea.rect;
            rect.anchoredPosition = new Vector2(0, r.yMin + 150f);
        }

        currentFrame = 0;
        if (animationFrames != null && animationFrames.Length > 0)
            shipImage.sprite = animationFrames[0];

        StopAllCoroutines();
        StartCoroutine(AnimationLoop());
    }

    public void BeginRound()
    {
        canControl = true;
        nextFire = 0f;
    }

    public void StopPlayer()
    {
        canControl = false;
    }

    private void Update()
    {
        if (!canControl) return;
        if (gameArea == null) return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameArea, mousePos, null, out localPos);

        rect.anchoredPosition = Vector2.MoveTowards(
            rect.anchoredPosition,
            localPos,
            moveSpeed * Time.deltaTime
        );

        Rect r = gameArea.rect;
        var p = rect.anchoredPosition;
        p.x = Mathf.Clamp(p.x, r.xMin + 50f, r.xMax - 50f);
        p.y = Mathf.Clamp(p.y, r.yMin + 50f, r.yMax - 50f);
        rect.anchoredPosition = p;
    }

    private void HandleShooting()
    {
        if (!Input.GetMouseButton(0)) return;
        if (Time.time < nextFire) return;

        nextFire = Time.time + fireRate;

        GameObject go = Instantiate(bulletPrefab, bulletContainer);
        var b = go.GetComponent<UIBullet>();
        var rt = go.GetComponent<RectTransform>();

        rt.anchoredPosition = rect.anchoredPosition + new Vector2(0, 60f);

        b.isPlayerBullet = true;
        b.direction = Vector2.up;
        b.gameArea = gameArea;
        b.microgame = microgame ?? GetComponentInParent<SpaceShooterMicrogame>();

        audioManager.PlayShoot();
    }

    public void TakeHit()
    {
        if (isInvulnerable) return;

        currentHP--;
        UpdateHPUI();

        if (currentHP <= 0)
        {
            canControl = false;
            shipImage.enabled = false;
            SpawnPlayerExplosion();
            audioManager.PlayExplosion();
            OnPlayerDead?.Invoke();
            return;
        }

        StartCoroutine(FlickerRoutine());
    }

    private void SpawnPlayerExplosion()
    {
        if (playerExplosionPrefab == null || gameArea == null) return;

        GameObject fx = Instantiate(playerExplosionPrefab, gameArea);
        RectTransform fxRect = fx.GetComponent<RectTransform>();

        if (fxRect != null)
            fxRect.anchoredPosition = rect.anchoredPosition;
    }

    private IEnumerator FlickerRoutine()
    {
        isInvulnerable = true;
        float t = 0f;

        while (t < flickerDuration)
        {
            shipImage.enabled = !shipImage.enabled;
            yield return new WaitForSeconds(flickerInterval);
            t += flickerInterval;
        }

        shipImage.enabled = true;
        isInvulnerable = false;
    }

    private IEnumerator AnimationLoop()
    {
        if (animationFrames == null || animationFrames.Length == 0)
            yield break;

        while (true)
        {
            shipImage.sprite = animationFrames[currentFrame];
            currentFrame = (currentFrame + 1) % animationFrames.Length;

            yield return new WaitForSeconds(animationSpeed);
        }
    }

    private void UpdateHPUI()
    {
        hptext.text = currentHP.ToString();
    }

    public void ClearAllBullets()
    {
        if (bulletContainer == null) return;
        for (int i = bulletContainer.childCount - 1; i >= 0; i--)
            Destroy(bulletContainer.GetChild(i).gameObject);
    }
}
