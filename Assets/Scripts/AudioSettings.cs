using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("Audio Sources")]
    [Tooltip("Main background music source")]
    [SerializeField] private AudioSource musicSource;

    [Tooltip("General sound effects source (UI, gameplay, etc.)")]
    [SerializeField] private AudioSource sfxSource;
    public void HoverSound()
    {
        PlaySFX(hover);
    }
    public void ClickSound()
    {
        PlaySFX(click);
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
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
}
