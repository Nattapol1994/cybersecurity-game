using UnityEngine;
using UnityEngine.UI;

public class UIStarGenerator : MonoBehaviour
{
    public RectTransform gameArea;
    public GameObject starPrefab;
    public int starCount = 50;

    private void Start()
    {
        if (gameArea == null || starPrefab == null) return;

        Rect r = gameArea.rect;

        for (int i = 0; i < starCount; i++)
        {
            GameObject s = Instantiate(starPrefab, transform);
            var rt = s.GetComponent<RectTransform>();
            var logic = s.GetComponent<UIStar>();

            rt.anchoredPosition = new Vector2(
                Random.Range(r.xMin, r.xMax),
                Random.Range(r.yMin, r.yMax)
            );

            logic.gameArea = gameArea;

            var img = s.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
        }
    }
}
