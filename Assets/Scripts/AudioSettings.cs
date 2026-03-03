using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance;
    [Header("Sliders")]
    [SerializeField] private Slider masterSound;
    [SerializeField] private Slider musicSound;
    [SerializeField] private Slider sfxSound;

    [Header("Texts")]
    [SerializeField] private Text masterText;
    [SerializeField] private Text musicText;
    [SerializeField] private Text sfxText;

    // Working (unsaved) values
    private float workingMaster;
    private float workingMusic;
    private float workingSfx;

    // Saved values (from PlayerPrefs)
    private float savedMaster;
    private float savedMusic;
    private float savedSfx;

    
    public AudioClip hover;
    public AudioClip click;
    public AudioClip countDown;

    [Header("Audio Sources")]
    [Tooltip("Main background music source")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("General sound effects source (UI, gameplay, etc.)")]
    [SerializeField] private AudioSource sfxSource; // <-- TENTO POUŽÍVÁ ENEMY AI

    [Tooltip("Dedicated UI sound effects source (Clicks, Hovers)")]
    [SerializeField] public AudioSource uiSource; // <-- NOVÝ ZDROJ

    [Header("Music Tracks")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip bossMusic;
    public AudioClip victoryMusic;

    private Coroutine musicFadeCoroutine;
    public void HoverSound()
    {
        PlayUISFX(hover);
    }
    public void ClickSound()
    {
        PlayUISFX(click);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void PlaySFX(AudioClip clip) // Tuto funkci volá EnemyAI, ale také UI (HoverSound, ClickSound)
    {
        // Ponecháme PlaySFX pro kompatibilitu s EnemyAI, který používá sfxSource
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }
    public void PlaySFXAbility(AudioClip clip) // Tuto funkci volá EnemyAI, ale také UI (HoverSound, ClickSound)
    {
        // Ponecháme PlaySFX pro kompatibilitu s EnemyAI, který používá sfxSource
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip,3.5f);
    }
    // Nová funkce pro UI zvuky
    private void PlayUISFX(AudioClip clip)
    {
        if (clip != null && uiSource != null)
            uiSource.PlayOneShot(clip);
    }
    private void Start()
    {
        // Load saved values (default 1f if not found)

        savedMaster = PlayerPrefs.GetFloat("MasterVolume", 100f);
        savedMusic = PlayerPrefs.GetFloat("MusicVolume", 100f);
        savedSfx = PlayerPrefs.GetFloat("SfxVolume", 100f);

        // Initialize working copy
        workingMaster = savedMaster;
        workingMusic = savedMusic;
        workingSfx = savedSfx;

        // Apply to sliders & labels
        masterSound.value = workingMaster;
        musicSound.value = workingMusic;
        sfxSound.value = workingSfx;

        UpdateTexts();
        ApplyVolumes();
        // Add listeners
        masterSound.onValueChanged.AddListener(OnMasterChanged);
        musicSound.onValueChanged.AddListener(OnMusicChanged);
        sfxSound.onValueChanged.AddListener(OnSfxChanged);

        AttachButtonSounds();
        PlayMusic(mainMenuMusic);
    }

    private void OnMasterChanged(float value)
    {
        workingMaster = value;
        masterText.text = value.ToString() + "%";
        UpdateTexts();
        ApplyVolumes();
    }

    private void OnMusicChanged(float value)
    {
        workingMusic = value;
        musicText.text = value.ToString() + "%";
        UpdateTexts();
        ApplyVolumes();
    }

    private void OnSfxChanged(float value)
    {
        workingSfx = value;
        sfxText.text = value.ToString() + "%";
        UpdateTexts();
        ApplyVolumes(); 
    }
    private void ApplyVolumes()
    {
        float master = workingMaster / 100f;
        float music = workingMusic / 100f;

        float sfx = workingSfx / 100f;

        AudioListener.volume = master;

        if (musicSource != null)
            musicSource.volume = music;

        if (sfxSource != null)
            sfxSource.volume = sfx;

        if (uiSource != null)
            uiSource.volume = sfx;
    }

    private void UpdateTexts()
    {
        masterText.text = workingMaster.ToString() + "%";
        musicText.text = workingMusic.ToString() + "%";
        sfxText.text = workingSfx.ToString() + "%";
    }
    private void AttachButtonSounds()
    {
        // This will find ALL buttons in the scene, even inactive ones
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();

        foreach (Button btn in buttons)
        {
            // Skip if the button is part of Unity's internal prefab stage / not in scene
            if (btn.gameObject.hideFlags != 0) continue;

            // Add click sound
            btn.onClick.AddListener(ClickSound);

            // Add hover sound via EventTrigger
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = btn.gameObject.AddComponent<EventTrigger>();
            }

            // Prevent duplicate hover entries
            bool alreadyHasHover = trigger.triggers.Exists(e => e.eventID == EventTriggerType.PointerEnter);
            if (!alreadyHasHover)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerEnter
                };
                entry.callback.AddListener((eventData) => { HoverSound(); });
                trigger.triggers.Add(entry);
            }
        }
    }
    public float GetSfxVolume()
    {
        return sfxSource != null ? sfxSource.volume : 1f;
    }

    // Save button
    public void SaveChanges()
    {
        PlayerPrefs.SetFloat("MasterVolume", workingMaster);
        PlayerPrefs.SetFloat("MusicVolume", workingMusic);
        PlayerPrefs.SetFloat("SfxVolume", workingSfx);
        PlayerPrefs.Save();

        savedMaster = workingMaster;
        savedMusic = workingMusic;
        savedSfx = workingSfx;

        // APLIKUJE NOVÉ HODNOTY OKAMŽITÌ, JAKO BY SE POHYBEM SLIDERU
        ApplyVolumes();
    }

    // Cancel button
    public void CancelChanges()
    {
        workingMaster = savedMaster;
        workingMusic = savedMusic;
        workingSfx = savedSfx;

        masterSound.value = workingMaster;
        musicSound.value = workingMusic;
        sfxSound.value = workingSfx;

        UpdateTexts();
    }
    public void PlayMusic(AudioClip newClip, bool loop = true)
    {
        if (musicSource == null || newClip == null) return;

        // Pokud už tato hudba hraje, nic nedìlej
        if (musicSource.clip == newClip && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();
    }
    // Vypne hudbu (nebo ji pauzne)
    public void MuteMusic(bool mute)
    {
        if (musicSource == null) return;

        if (mute)
            musicSource.Pause(); // Hudba zùstane na stejném místì
        else
            musicSource.UnPause(); // Hudba pokraèuje
    }
    public void CrossfadeTo(AudioClip newClip, float fadeTime = 1f, bool loop = true)
    {
        if (musicSource == null || newClip == null) return;
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip, fadeTime, loop));
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float fadeTime, bool loop)
    {
        float startVolume = musicSource.volume;

        // Fade out
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        float targetVolume = workingMusic / 100f;

        // Fade in
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeTime);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFadeCoroutine = null;
    }

    // Alternativa: Úplné zastavení
    public void StopMusic() => musicSource.Stop();
    public void PlayMenu() => PlayMusic(mainMenuMusic);
    public void PlayGameplay() => PlayMusic(gameplayMusic);
    public void PlayBoss() => PlayMusic(bossMusic);
    public void PlayVictory() => PlayMusic(victoryMusic, false);
}
