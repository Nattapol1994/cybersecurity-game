using UnityEngine;

public class TestMicrogame : BaseMicrogame
{


    public override void Initialize(float difficulty = 1f)
    {
        // Optionally, you can add initialization code here
    }

    void OnMouseDown()
    {
        manager.MicrogameSuccess();
        gameObject.SetActive(false);
    }
    
    protected override void OnTimeout()
    {
        running = false;
        manager.MicrogameFailure();
        gameObject.SetActive(false);
    }
}
