using UnityEngine;
using UnityEngine.UI;

public class UIPlanet : MonoBehaviour
{
    public float speed = 60f;
    public RectTransform rect;
    public RectTransform gameArea;

    [HideInInspector] public UIPlanetGenerator generator;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rect == null || gameArea == null) return;

        rect.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        Rect r = gameArea.rect;

        if (rect.anchoredPosition.y < r.yMin - 300f)
        {
            generator?.OnPlanetExited(this);
            Destroy(gameObject);
        }
    }
}
