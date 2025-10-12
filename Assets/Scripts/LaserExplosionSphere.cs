using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class LaserExplosionSphere : MonoBehaviour
{
    [Header("Light Settings")]
    public Light pointLight;
    public float startIntensity = 10f;
    public float lightRange = 8f;
    public float lightFadeTime = 1f;

    [Header("Auto Destroy")]
    public float lifetime = 1.5f;

    private ParticleSystem ps;
    private AudioSource audioSource;
    private float startTime;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        // create an AudioSource automatically
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

   

  

    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }
    
}
