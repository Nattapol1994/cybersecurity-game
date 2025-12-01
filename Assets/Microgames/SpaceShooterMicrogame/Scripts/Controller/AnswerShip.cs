using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerShip : MonoBehaviour
{
    public RectTransform rect;
    public Image shipImage;
    public TextMeshProUGUI answerText;
    public float speed = 200f;

    [HideInInspector] public RectTransform gameArea;
    [HideInInspector] public SpaceShooterMicrogame microgame;

    [HideInInspector] public int choiceIndex;
    [HideInInspector] public bool isCorrect;

    [Header("Effects")]
    public GameObject explosionPrefab;

    private void Awake()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();
        if (shipImage == null)
            shipImage = GetComponent<Image>();
        if (answerText == null)
            answerText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (rect == null)
            rect = GetComponent<RectTransform>();

        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        if (microgame != null)
        {
            microgame.RegisterAnswerShip(this);
            if (gameArea == null)
                gameArea = microgame.gameArea;
        }
    }

    private void OnDisable()
    {
        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        if (microgame != null)
            microgame.UnregisterAnswerShip(this);
    }

    public void Initialize(string text, bool correct)
    {
        isCorrect = correct;
        if (answerText != null)
            answerText.text = text;
    }

    private void Update()
    {
        if (rect == null || gameArea == null) return;


        Rect r = gameArea.rect;
        if (rect.anchoredPosition.y < r.yMin - 100f)
        {
            microgame?.OnAnswerShipTimeout(this);
            Destroy(gameObject);
        }
    }

    public void HitByPlayerBullet()
    {
        if (microgame == null)
            microgame = GetComponentInParent<SpaceShooterMicrogame>();

        microgame?.OnAnswerShipHit(this);

        var fx = Instantiate(explosionPrefab, microgame.gameArea);
        RectTransform fxRT = fx.GetComponent<RectTransform>();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            microgame.gameArea,
            rect.TransformPoint(rect.rect.center),
            null,         
            out localPos
        );

        fxRT.anchoredPosition = localPos;

        Destroy(gameObject);
    }
}
