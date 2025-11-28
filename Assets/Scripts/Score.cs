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
        
            string diff = PlayerPrefs.GetString("Difficulty", "Normal");
            string difficultyKey = "MaxScore_" + diff;

            maxScore = PlayerPrefs.GetInt(difficultyKey, 0);

            if (bestScoreText != null)
                bestScoreText.gameObject.SetActive(true);

            bestScoreText.text = "Best score: " + maxScore + diff;
        

    }
    public void OnDifficultyChanged()
    {
        string diff = PlayerPrefs.GetString("Difficulty", "Normal");
        string difficultyKey = "MaxScore_" + diff;

        maxScore = PlayerPrefs.GetInt(difficultyKey, 0);

        if (bestScoreText != null)
            bestScoreText.text = "Best score: " + maxScore;
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void SaveMaxScore()
    {
        string diff = PlayerPrefs.GetString("Difficulty", "Normal");
        string difficultyKey = "MaxScore_" + diff;

        if (score > maxScore)
        {
            maxScore = score;
            PlayerPrefs.SetInt(difficultyKey, maxScore);
            PlayerPrefs.Save();
        }
        Debug.Log("scoreMax");
    }



}
