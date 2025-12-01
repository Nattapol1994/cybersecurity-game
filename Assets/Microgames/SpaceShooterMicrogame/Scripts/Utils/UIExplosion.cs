using UnityEngine;
using UnityEngine.UI;

public class UIExplosion : MonoBehaviour
{
    public Image image;               
    public Sprite[] frames;             
    public float frameRate = 0.05f;     
    public bool autoPlay = true;

    private int index = 0;
    private float timer = 0f;

    void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    void OnEnable()
    {
        index = 0;
        timer = 0f;

        if (autoPlay && frames.Length > 0)
            image.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0)
            return;

        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer -= frameRate;
            index++;

            if (index < frames.Length)
            {
                image.sprite = frames[index];
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
