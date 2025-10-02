using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioSettings : MonoBehaviour
{
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

    [Header("Audio")]
    public AudioSource mySounds;
    public AudioClip hover;
    public AudioClip click;
    public void HoverSound()
    {
        mySounds.PlayOneShot(hover);
    }
    public void ClickSound()
    {
        mySounds.PlayOneShot(click);
    }
    private void Start()
    {
        // Load saved values (default 1f if not found)
        savedMaster = PlayerPrefs.GetFloat("MasterVolume", 80f);
        savedMusic = PlayerPrefs.GetFloat("MusicVolume", 80f);
        savedSfx = PlayerPrefs.GetFloat("SfxVolume", 80f);

        // Initialize working copy
        workingMaster = savedMaster;
        workingMusic = savedMusic;
        workingSfx = savedSfx;

        // Apply to sliders & labels
        masterSound.value = workingMaster;
        musicSound.value = workingMusic;
        sfxSound.value = workingSfx;

        UpdateTexts();

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
    }

    private void OnMusicChanged(float value)
    {
        workingMusic = value;
        musicText.text = value.ToString() + "%";
    }

    private void OnSfxChanged(float value)
    {
        workingSfx = value;
        sfxText.text = value.ToString() + "%";
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
