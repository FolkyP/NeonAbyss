using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class CutsceneConfig
{
    public string label = "Cutscene";
    public PlayableDirector director;
    [Tooltip("PlayerPrefs key to mark this cutscene as 'played'")]
    public string prefsKey = "Cutscene_Played";
    [Tooltip("If true, skip can be used immediately (after skipDelay) even if not played before")]
    public bool skippableInitially = false;
    [Tooltip("If true, skip will be allowed after the cutscene has been played once (persisted by prefsKey)")]
    public bool allowSkipAfterFirstPlay = true;
    [Tooltip("Seconds to wait before allowing skip (and showing skip UI)")]
    public float skipDelay = 2f;
}

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("Cutscene configs")]
    public CutsceneConfig startConfig;
    public CutsceneConfig endMapConfig;
    public CutsceneConfig endGameConfig;
    [Header("Cutscene-specific objects")]
    public GameObject cutscene1Texts;
    [Header("Cameras")]
    public GameObject cutsceneCamera;
    public GameObject playerCamera;

    [Header("UI")]
    public GameObject skipText; // doporuèený samostatný Canvas

    PlayableDirector currentDirector;
    CutsceneConfig currentConfig;
    bool canSkip;
    public bool isCutscenePlaying = false;
    public System.Action<PlayableDirector> OnCutsceneEnded;
    GameObject activeCutsceneObject;

    public GameObject tutorial1;
    void Awake()
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

    void Update()
    {
        // Lepší detekce stisku než Input.anyKey (jednorázové stisknutí)
        if (canSkip && Input.GetKey(KeyCode.Space))
        {
            SkipCutscene();
        }
    }

    // veøejné volání pro spouštìní
    public void PlayStartCutscene() => PlayCutscene(startConfig);
    public void PlayEndMapCutscene() => PlayCutscene(endMapConfig);
    public void PlayEndGameCutscene() => PlayCutscene(endGameConfig);

    void PlayCutscene(CutsceneConfig config)
    {
        if (config == null || config.director == null)
        {
            Debug.LogWarning("CutsceneManager: PlayCutscene - config nebo director je null.");
            return;
        }

        currentConfig = config;
        currentDirector = config.director;

        Disable(); // nastaví isCutscenePlaying = true
        SwitchToCutsceneCamera();
        if (cutscene1Texts != null && config == startConfig)
        {
            activeCutsceneObject = cutscene1Texts;
            activeCutsceneObject.SetActive(true);
        }
        // zabezpeèení souèástí UI (aby neskákal NullReference)
        if (skipText != null) skipText.SetActive(false);
        canSkip = false;

        // rozhodnutí, jestli povolit skip (a po jakém delay)
        bool hasBeenPlayed = PlayerPrefs.GetInt(config.prefsKey, 0) == 1;

        if (config.skippableInitially)
        {
            // lze skipnout hned (po delay)
            if (config.skipDelay <= 0f)
                EnableSkip();
            else
                Invoke(nameof(EnableSkip), config.skipDelay);
        }
        else if (config.allowSkipAfterFirstPlay && hasBeenPlayed)
        {
            if (config.skipDelay <= 0f)
                EnableSkip();
            else
                Invoke(nameof(EnableSkip), config.skipDelay);
        }
        // jinak: skip není povolen (napø. první intro)

        currentDirector.stopped += OnCutsceneFinished;
        currentDirector.Play();
    }

    void SkipCutscene()
    {
        if (currentDirector == null) return;

        currentDirector.stopped -= OnCutsceneFinished;
        currentDirector.Stop();

        EndCutscene();
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        director.stopped -= OnCutsceneFinished;
        EndCutscene();
    }

    void EndCutscene()
    {
        // zrušíme plánované povolení skipu (pokud nebylo ještì provedené)
        CancelInvoke(nameof(EnableSkip));

        canSkip = false;
        if (skipText != null) skipText.SetActive(false);

        SwitchToPlayerCamera();
         // nastaví isCutscenePlaying = false

        // oznaèíme tuto konkrétní cutscénu jako pøehranou (pokud máme currentConfig)
        if (currentConfig != null && !string.IsNullOrEmpty(currentConfig.prefsKey))
        {
            PlayerPrefs.SetInt(currentConfig.prefsKey, 1);
            PlayerPrefs.Save();
        }

        OnCutsceneEnded?.Invoke(currentDirector);
        if (activeCutsceneObject != null)
        {
            activeCutsceneObject.SetActive(false);
            activeCutsceneObject = null;
        }
        if (currentConfig == startConfig)
        {
            //// Pokud tutorial ještì nebyl dokonèen, zobrazíme ho
            //if (PlayerPrefs.GetInt("Tutorial_Completed", 0) == 0)
            //{
            //    GameSettings.Instance.InputLocked = true;
            //    tutorial1.SetActive(true);
            //}
            //else
            //{
            //    // Jinak rovnou povolíme input a pokraèujeme
            //    GameSettings.Instance.InputLocked = false;
            //}
            GameSettings.Instance.InputLocked = true;
            tutorial1.SetActive(true);
        }

        currentDirector = null;
        currentConfig = null;
        Enable();
    }

    void EnableSkip()
    {
        canSkip = true;
        if (skipText != null) skipText.SetActive(true);
    }

    void SwitchToCutsceneCamera()
    {
        if (cutsceneCamera != null) cutsceneCamera.SetActive(true);

        // Místo SetActive(false) vypneme jen obraz a zvuk
        if (playerCamera != null)
        {
            var camComponent = playerCamera.GetComponent<Camera>();
            if (camComponent != null) camComponent.enabled = false;

            var listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    void SwitchToPlayerCamera()
    {
        if (cutsceneCamera != null) cutsceneCamera.SetActive(false);

        // Znovu zapneme obraz a zvuk hráèe
        if (playerCamera != null)
        {
            var camComponent = playerCamera.GetComponent<Camera>();
            if (camComponent != null) camComponent.enabled = true;

            var listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;
        }
    }

    public void Disable()
    {
        isCutscenePlaying = true;
    }

    public void Enable()
    {
        isCutscenePlaying = false;
    }

    // volitelná veøejná pomocná funkce pro nucené preskocení (napø. volání z UI)
    public void ForceSkipCurrentCutscene()
    {
        if (canSkip) SkipCutscene();
    }

    public void PrepareCutsceneBeforePlayerSpawn()
    {
        isCutscenePlaying = true; // Nastavíme hned, aby Update v GameSettings nereagoval
        SwitchToCutsceneCamera();
    }
}
