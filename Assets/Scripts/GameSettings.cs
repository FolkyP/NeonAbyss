using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private bool isGameOn = false;
    [SerializeField] public bool isGameStopped = false;

    public GameObject exitSc;
    public GameObject settingsMenu;
    public GameObject mainMenu;
    public GameObject playerUI;

    public GameObject selectMenu;

    public GameObject sureMenu;
    public GameObject menuMid;
    public GameObject[] game;
    public Camera cameraUI;

    public Button easyButton;
    public Button normalButton;
    public Button hardButton;


    private Button selectedButton;

    [SerializeField] private Text _fpsText;
    [SerializeField] private float _hudRefreshRate = 1f;

    private float _timer;
    private int _frameCount;
    private float _deltaTime;

    public Text ammoText;
    public Text WeaponText;
    public Text allAmmo;
    private void Update()
    {
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
        easyButton.onClick.AddListener(() => SelectButton(easyButton));
        normalButton.onClick.AddListener(() => SelectButton(normalButton));
        hardButton.onClick.AddListener(() => SelectButton(hardButton));

        // Default selection
        SelectButton(normalButton);
    }

    private void SelectButton(Button button)
    {
        selectedButton = button;

        // Reset all
        SetButtonNormalColor(easyButton, 0.3f);
        SetButtonNormalColor(normalButton, 0.3f);
        SetButtonNormalColor(hardButton, 0.3f);

        // Highlight selected
        SetButtonNormalColor(selectedButton, 1f);
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
            // Set game parameters for Easy difficulty
        }
        else if (selectedButton == normalButton)
        {
            Debug.Log("Starting game with Normal difficulty");
            // Set game parameters for Normal difficulty
        }
        else if (selectedButton == hardButton)
        {
            Debug.Log("Starting game with Hard difficulty");
            // Set game parameters for Hard difficulty
        }
        cameraUI.gameObject.SetActive(false);
        foreach (GameObject g in game)
        {
            g.SetActive(true);
        }
        // Load the game scene or start the game
        isGameOn = true;
        Time.timeScale = 1f; // Ensure the game is running
        selectMenu.SetActive(false);
    }
    public void ExitScreen()
    {
        exitSc.SetActive(!exitSc.activeSelf);
    }
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
