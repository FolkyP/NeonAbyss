using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameSettings : MonoBehaviour
{
    [SerializeField] public bool isGameOn = false;
    [SerializeField] public bool isGameStopped = false;

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

    private void Update()
    {
        if (hasStarted && Input.GetKeyDown(KeyCode.Space))
        {
            StartCountDown();
            hasStarted = false; // Prevent multiple starts
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
        
    }
    public void StartGameWithDifficulty()
    {
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

        foreach (GameObject g in game)
            g.SetActive(true);

        selectMenu.SetActive(false);
        Time.timeScale = 1f; // pause while counting down
        hasStarted = true;
    }

    public void StartCountDown()
    {
        StartCoroutine(CountdownRoutine());
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

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
