using UnityEngine;
using UnityEngine.UI;

public class UIPlanetGenerator : MonoBehaviour
{
    public RectTransform gameArea;
    public GameObject[] planetPrefabs;    // at least 1
    public float minSpawnY = 300f;        // how high above screen to appear
    public float minXPadding = 200f;      // avoid edges if needed

    private UIPlanet activePlanet;

    private void Start()
    {
        SpawnPlanet();
    }

    private void SpawnPlanet()
    {
        if (planetPrefabs == null || planetPrefabs.Length == 0) return;
        if (gameArea == null) return;

        // Pick a random prefab
        var prefab = planetPrefabs[Random.Range(0, planetPrefabs.Length)];

        GameObject obj = Instantiate(prefab, transform);
        UIPlanet planet = obj.GetComponent<UIPlanet>();
        RectTransform rt = obj.GetComponent<RectTransform>();

        activePlanet = planet;

        planet.gameArea = gameArea;
        planet.generator = this;

        // spawn at random X, above screen
        Rect r = gameArea.rect;
        rt.anchoredPosition = new Vector2(
            Random.Range(r.xMin + minXPadding, r.xMax - minXPadding),
            r.yMax + minSpawnY
        );

        // ensure planet doesn't block input
        var img = obj.GetComponent<Image>();
        if (img != null) img.raycastTarget = false;
    }

    // Called by UIPlanet when it exits screen
    public void OnPlanetExited(UIPlanet p)
    {
        // simple: spawn a new one immediately
        SpawnPlanet();
    }
}
