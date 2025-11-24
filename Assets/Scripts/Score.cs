using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score Instance;

    public int score = 0;
    public int maxScore = 0;


    public Text scoreText;
    public Text bestScoreText;

    

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
        // Naètení nejlepšího výsledku
        maxScore = PlayerPrefs.GetInt("MaxScore", 0);

        if (bestScoreText != null)
            bestScoreText.text = "Best: " + maxScore;

       
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void SaveMaxScore()
    {
        // Pokud je dosažen nový rekord
        if (score > maxScore)
        {
            maxScore = score;
            PlayerPrefs.SetInt("MaxScore", maxScore);
        }
    }

   
}
