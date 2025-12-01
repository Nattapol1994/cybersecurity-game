using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("SFX")]
    public AudioClip shoot;
    public AudioClip explosion;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayShoot()
    {
        if (shoot) sfxSource.PlayOneShot(shoot);
    }

    public void PlayExplosion()
    {
        if (explosion) sfxSource.PlayOneShot(explosion);
    }

    public void PlayBGM()
    {
        if (bgmSource && !bgmSource.isPlaying)
            bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource) bgmSource.Stop();
    }
}
