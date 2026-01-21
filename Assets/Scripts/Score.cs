using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Score : MonoBehaviour
{
    public static Score Instance;

    public int score = 0;
    public int maxScore = 0;

    public Text scoreText;
    public TMP_Text bestScoreText;

    private string difficultyKey;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Naèteme aktuální best podle uložené difficulty
        UpdateForCurrentDifficulty();
        UpdateScoreText();
    }

    // volat z GameSettings.SelectButton() po zmìnì PlayerPrefs("Difficulty", ...)
    public void OnDifficultyChanged()
    {
        UpdateForCurrentDifficulty();
    }

    void UpdateForCurrentDifficulty()
    {
        string diff = PlayerPrefs.GetString("Difficulty", "Normal");
        difficultyKey = "MaxScore_" + diff;
        maxScore = PlayerPrefs.GetInt(difficultyKey, 0);
        UpdateBestText();
    }

    void UpdateBestText()
    {
        if (bestScoreText == null) return;
        string diff = PlayerPrefs.GetString("Difficulty", "Normal");
        bestScoreText.gameObject.SetActive(true);
        bestScoreText.text = $"Best ({diff}): {maxScore}";
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

    // Zavolat pøi konci hry (win / death / návrat do menu)
    public void SaveMaxScore()
    {
        string diff = PlayerPrefs.GetString("Difficulty", "Normal");
        difficultyKey = "MaxScore_" + diff;

        if (score > maxScore)
        {
            maxScore = score;
            PlayerPrefs.SetInt(difficultyKey, maxScore);
            PlayerPrefs.Save();
            UpdateBestText();
            Debug.Log($"New best for {diff}: {maxScore}");
        }
        else
        {
            Debug.Log($"Score ({score}) not higher than best ({maxScore}) for {diff}");
        }
    }
}
