using System;
using UnityEngine;

public class SingleShotAudioManager : MonoBehaviour
{
    public static SingleShotAudioManager Instance;

    public AudioSource audioSource;
    public AudioClip hurtSound;
    public AudioClip collectSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayHurtSound()
    {
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or HurtSound is not assigned in SingleShotAudioManager.");
        }
    }

    public void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or CollectSound is not assigned in SingleShotAudioManager.");
        }
    }
}
