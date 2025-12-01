using UnityEngine;

public class UIBullet : MonoBehaviour
{
    public bool isPlayerBullet = true;
    public float speed = 800f;        
    public Vector2 direction = Vector2.up;

    [HideInInspector] public SpaceShooterMicrogame microgame;
    [HideInInspector] public RectTransform rect;
    [HideInInspector] public RectTransform gameArea;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        if (microgame != null)
            microgame.RegisterBullet(this);
    }

    private void OnDisable()
    {
        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        if (microgame != null)
            microgame.UnregisterBullet(this);
    }

    private void Update()
    {
        if (rect == null || gameArea == null) return;

        rect.anchoredPosition += direction.normalized * speed * Time.deltaTime;

        var bounds = gameArea.rect;
        var p = rect.anchoredPosition;
        if (p.x < bounds.xMin - 100f || p.x > bounds.xMax + 100f ||
            p.y < bounds.yMin - 100f || p.y > bounds.yMax + 100f)
        {
            Destroy(gameObject);
        }
    }
}
