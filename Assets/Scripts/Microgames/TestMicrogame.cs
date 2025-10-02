using UnityEngine;

public class TestMicrogame : BaseMicrogame
{


    public override void Initialize(float difficulty = 1f)
    {
        // Optionally, you can add initialization code here
    }

    void OnMouseDown()
    {
        MicrogameSuccess();
        gameObject.SetActive(false);
    }
}
