using UnityEngine;
using UnityEngine.UI;

public class UIStar : MonoBehaviour
{
    public float speed = 60f;         
    public RectTransform rect;
    public RectTransform gameArea;

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

        if (rect.anchoredPosition.y < r.yMin)
        {
            rect.anchoredPosition = new Vector2(
                Random.Range(r.xMin, r.xMax),
                r.yMax + 20f
            );
        }
    }
}
