using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class GameSettings : MonoBehaviour
{
    [SerializeField] public bool isGameOn = false;
    [SerializeField] public bool isGameStopped = false;

    public GameObject playerCanvas;
    public GameObject StartPlane;
    public GameObject exitSc;
    public GameObject settingsMenu;
    public GameObject mainMenu;
    public GameObject playerUI;
    public GameObject deathScreen;

    public GameObject selectMenu;

    public GameObject sureMenu;
    public GameObject menuMid;
    public GameObject[] game;
    public Camera cameraUI;

    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    public GameObject easySelect;
    public GameObject normalSelect;
    public GameObject hardSelect;
    public bool isOverDriveActive = false;

    private Button selectedButton;

    [SerializeField] private Text _fpsText;
    [SerializeField] private float _hudRefreshRate = 1f;

    private float _timer;
    private int _frameCount;
    private float _deltaTime;

    public Text ammoText;
    public Text WeaponText;
    public Text allAmmo;

    [Header("UI References")]
    public Text countdownText; // assign in Inspector

    private bool hasStarted = false;

    public static GameSettings Instance;

    public TMP_Text score;

    [Header("Map Sequence Management")]
    public GameObject[] mapGameObjects;
    public MapIndex selectedMap = MapIndex.Map1;
    [HideInInspector] public int currentMapIndex = 0;

    public enum MapIndex { Map1 = 0, Map2 = 1, Map3 = 2 }
    public enum GameMode { Survival, Final, Waves }
    public GameMode selectedGameMode;

    public WeaponManager weaponManager;
    public CutsceneManager cutsceneManager;
    private bool canStartAfterCutscene = false;
    private void Update()
    {
        if (cutsceneManager.isCutscenePlaying || !canStartAfterCutscene)
        {
            return;
        }
        if (hasStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartCountDown();
            hasStarted = false; // Prevent multiple starts
            canStartAfterCutscene = false;
        }
        if (isGameOn)
        {
            _frameCount++;
            _deltaTime += Time.unscaledDeltaTime;

            if (Time.unscaledTime > _timer)
            {
                int fps = Mathf.RoundToInt(_frameCount / _deltaTime);
                _fpsText.text = $"FPS: {fps}";

                _frameCount = 0;
                _deltaTime = 0f;
                _timer = Time.unscaledTime + _hudRefreshRate;
            }
            if(Input.GetKeyDown(KeyCode.P))
            {
                if (menuMid.activeSelf)
                {
                    menuMid.SetActive(false);
                    Time.timeScale = 1f; // Resume the game
                    isGameStopped = false;
                }
                else
                {
                    OpenMenuMidGame();
                }
            }
            
        }
    }
    void Start()
    {
        Instance = this;
        easyButton.onClick.AddListener(() => SelectButton(easyButton));
        normalButton.onClick.AddListener(() => SelectButton(normalButton));
        hardButton.onClick.AddListener(() => SelectButton(hardButton));

        // Default selection
        SelectButton(normalButton);

        ApplySelectedMap();
    }
    private void OnValidate()
    {
        
        ApplySelectedMap();
    }
    [ContextMenu("Apply Selected Map")]
    public void ApplySelectedMap()
    {

        //stop spawn a change system
        currentMapIndex = (int)selectedMap;

        if (mapGameObjects != null && mapGameObjects.Length > 0)
        {
            for (int i = 0; i < mapGameObjects.Length; i++)
            {
                if (mapGameObjects[i] != null)
                    mapGameObjects[i].SetActive(i == currentMapIndex);
            }
        }
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.ApplyMapIndex(currentMapIndex);
            SpawnManager.Instance.ResetForNewMap();
        }
        if(currentMapIndex > 0)
        {
            BossManager.Instance.StartBossFight();
        }
        //if (sm != null)
        //{
        //    sm.ApplyMapIndex(currentMapIndex);

        //    // Mùžete podle mapy automaticky nastavit herní mód (pøíklad map->mode)
        //    // Pokud chcete jinou logiku map->mode, upravte zde.
        //    switch (selectedMap)
        //    {
        //        case MapIndex.Map1:
        //            selectedGameMode = GameMode.Survival;
        //            break;
        //        case MapIndex.Map2:
        //            selectedGameMode = GameMode.Waves;
        //            break;
        //        case MapIndex.Map3:
        //            selectedGameMode = GameMode.Final;
        //            break;
        //    }

        //    //// Aplikujte pøípadné map-specific nastavení (èasovaèe, spawn rychlost...)
        //    //sm.ConfigureForGameMode(selectedGameMode);
        //}


    }
    private void SelectButton(Button button)
    {
        selectedButton = button;

        // Reset all button colors
        SetButtonNormalColor(easyButton, 0.3f);
        SetButtonNormalColor(normalButton, 0.3f);
        SetButtonNormalColor(hardButton, 0.3f);

        // Highlight selected button
        SetButtonNormalColor(selectedButton, 1f);

        // Toggle difficulty indicators
        easySelect.SetActive(button == easyButton);
        normalSelect.SetActive(button == normalButton);
        hardSelect.SetActive(button == hardButton);

        score.color = Color.white;

        if (button == easyButton)
            score.color = Color.green;
        else if (button == normalButton)
            score.color = Color.yellow;
        else if (button == hardButton)
            score.color = Color.red;

        if (button == easyButton)
            PlayerPrefs.SetString("Difficulty", "Easy");
        else if (button == normalButton)
            PlayerPrefs.SetString("Difficulty", "Normal");
        else if (button == hardButton)
            PlayerPrefs.SetString("Difficulty", "Hard");

        PlayerPrefs.Save();
    }


    private void SetButtonNormalColor(Button button, float alpha)
    {
        ColorBlock cb = button.colors;
        cb.normalColor = new Color(1f, 1f, 1f, alpha);
        button.colors = cb;
        

        // Change all RawImages in children
        RawImage[] images = button.GetComponentsInChildren<RawImage>();
        foreach (RawImage img in images)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
    public void ReturnFromSettings()
    {
        if (isGameOn)
        {
            settingsMenu.SetActive(false);
            playerUI.SetActive(true);
            menuMid.SetActive(true);
        }
        else
        {
            settingsMenu.SetActive(false);
            mainMenu.SetActive(true);
        }
    }
    public void SettingsOpen()
    {
        if(isGameOn)
        {
            playerUI.SetActive(false);
            settingsMenu.SetActive(true);
            menuMid.SetActive(false);
        }
        else
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(true);

        }
    }
    public void ReturnFromSelect()
    {
        selectMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void ReturnOpen()
    {
        selectMenu.SetActive(true);
        mainMenu.SetActive(false);
    }
    public void OpenMenuMidGame()
    {
        
         Time.timeScale = 0f; // Pause the game
        isGameStopped = true;
       
        menuMid.SetActive(true);
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            // Zkontrolujeme, zda zdroj není ten pro UI zvuky
            if (source != AudioSettings.Instance.uiSource && source.loop)
            {
                source.Pause(); // Použijeme Pause, abychom mohli pozdìji volat UnPause
            }
        }
    }
    public void OpenSureLeaveMenu()
    {
        sureMenu.SetActive(true);
    }
    public void EndGame()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void CloseSureLeaveMenu()
    {
        sureMenu.SetActive(false);
    }
    public void Continue()
    {
        Time.timeScale = 1f; // Resume the game
        isGameStopped = false;
        menuMid.SetActive(false);
        sureMenu.SetActive(false);
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in allAudioSources)
        {
            // Pokud je zdroj pozastaven (a není to UI zdroj)
            if (source != AudioSettings.Instance.uiSource && source.time != 0 && !source.isPlaying)
            {
                source.UnPause();
            }
        }
    }
    public void StartGameWithDifficulty()
    {
        
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PrepareCutsceneBeforePlayerSpawn();
        }
        if (selectedButton == easyButton)
        {
            Debug.Log("Starting game with Easy difficulty");
            // Set parameters for easy
        }
        else if (selectedButton == normalButton)
        {
            Debug.Log("Starting game with Normal difficulty");
            // Set parameters for normal
        }
        else if (selectedButton == hardButton)
        {
            Debug.Log("Starting game with Hard difficulty");
            // Set parameters for hard
        }

        cameraUI.gameObject.SetActive(false);
        StartPlane.SetActive(true);
        

        //EnableGameplayAfterIntro();
        Time.timeScale = 1f; // pause while counting down
        hasStarted = true;

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.PlayStartCutscene();
        }
        selectMenu.SetActive(false);


    }

    public void StartCountDown()
    {
        StartCoroutine(CountdownRoutine());
    }

    
    public void LoadNextMap()
    {
        // 1. Zvýšit index
        currentMapIndex++;

        // Ošetøení pøeteèení indexu
        if (currentMapIndex >= mapGameObjects.Length)
            currentMapIndex = mapGameObjects.Length - 1; // Nebo 0, pokud chceš smyèku

        selectedMap = (MapIndex)currentMapIndex;
        Debug.Log("Loading map: " + selectedMap);

        // 2. Aplikovat mapu
        ApplySelectedMap();

        // 3. RESET UI a HRÁÈE
        playerUI.SetActive(true);
        SpawnManager.Instance.ResetForNewMap();
        weaponManager.ResetGun();

        // --- HLAVNÍ ZMÌNA ZDE ---

        // Vypneme herní smyèku (zastaví spawnování, timer atd.)
        isGameOn = false;

        if (currentMapIndex > 0) 
        // Povolíme èekání na mezerník v Update()
        hasStarted = true;

        
    }
    private IEnumerator CountdownRoutine()
    {
        int count = 3;
        countdownText.gameObject.SetActive(true);
        AudioSettings.Instance.PlaySFX(AudioSettings.Instance.countDown);
        while (count > 0)
        {
            countdownText.text = count.ToString();
            Debug.Log("Game starting in " + count);
            yield return new WaitForSeconds(1f);
            count--;
        }

        countdownText.text = "FIGHT!";
        Debug.Log("GO!");
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        // Now start the actual gameplay
        isGameOn = true;
        Time.timeScale = 1f;
    }
    public string GetDifficultyKey()
    {
        if (selectedButton == easyButton)
            return "Easy";
        if (selectedButton == normalButton)
            return "Normal";
        return "Hard";
    }

    public void ExitScreen()
    {
        exitSc.SetActive(!exitSc.activeSelf);
    }
    public void Death()
    {
        deathScreen.SetActive(true);
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;      // Reset time scale in case the game was paused
        

        // Reload the scene to reset all objects and variables
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void HandleCutsceneEnded(PlayableDirector director)
    {
        EnableGameplayAfterIntro();

        // Aktivujeme objekty hry
        foreach (GameObject g in game)
            g.SetActive(true);

        // Spustíme korutinu pro bezpeèný reset zbraní a odpoèet
        StartCoroutine(PrepareGameplayAfterCutscene());
    }

    private IEnumerator PrepareGameplayAfterCutscene()
    {
        // 1. POÈKÁME jeden snímek (null), aby probìhly metody Start() u zbraní
        yield return null;

        // 2. Teï už mají zbranì naètené své náboje, mùžeme je resetovat
        if (weaponManager != null)
        {
            weaponManager.ResetGun();
        }

        // 3. Poèkáme malou chvíli pro "vychladnutí" mezerníku (proti skipu)
        yield return new WaitForSeconds(0.2f);

        canStartAfterCutscene = true;

        // Volitelné: Zobrazit text "Press SPACE to Start" nebo nìco podobného
    }
    void EnableGameplayAfterIntro()
    {
        playerCanvas.SetActive(true);
        StartPlane.SetActive(false);
    }
    void OnEnable()
    {
        StartCoroutine(RegisterCutsceneListenerNextFrame());
    }

    private IEnumerator RegisterCutsceneListenerNextFrame()
    {
        // poèkej jeden frame, aby probìhlo Awake všech objektù
        yield return null;
        if (CutsceneManager.Instance != null)
            CutsceneManager.Instance.OnCutsceneEnded += HandleCutsceneEnded;
        else
            Debug.LogWarning("CutsceneManager.Instance is null in GameSettings.OnEnable");
    }

    void OnDisable()
    {
        if (CutsceneManager.Instance != null)
            CutsceneManager.Instance.OnCutsceneEnded -= HandleCutsceneEnded;
    }



    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
