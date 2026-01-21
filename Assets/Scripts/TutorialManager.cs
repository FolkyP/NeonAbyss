using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    private const string TutorialPrefsKey = "Tutorial_Completed";

    [Header("Tutorial steps")]
    public List<GameObject> tutorialSteps = new List<GameObject>();

    [Header("Buttons")]
    public Button backButton;
    public Button nextButton;
    public Button finishButton;
    public TMP_Text count;

    private int currentIndex = 0;
    void Start()
    {
        //if (PlayerPrefs.GetInt(TutorialPrefsKey, 0) == 1)
        //{
        //    GameSettings.Instance.InputLocked = false;
        //    gameObject.SetActive(false);
        //    return;
        //}
        if (tutorialSteps.Count == 0)
        {
            Debug.LogWarning("TutorialController: žádné tutorialSteps.");
            return;
        }


        // Vypnout všechny
        for (int i = 0; i < tutorialSteps.Count; i++)
            tutorialSteps[i].SetActive(false);

        // Zapnout první
        currentIndex = 0;
        tutorialSteps[currentIndex].SetActive(true);

        // Button listeners
        backButton.onClick.AddListener(PreviousStep);
        nextButton.onClick.AddListener(NextStep);

        UpdateButtons();
    }
    public void NextStep()
    {
        if (currentIndex >= tutorialSteps.Count - 1)
            return;

        tutorialSteps[currentIndex].SetActive(false);
        currentIndex++;
        tutorialSteps[currentIndex].SetActive(true);

        UpdateButtons();
    }

    public void PreviousStep()
    {
        if (currentIndex <= 0)
            return;

        tutorialSteps[currentIndex].SetActive(false);
        currentIndex--;
        tutorialSteps[currentIndex].SetActive(true);

        UpdateButtons();
    }
    public void FinishTutorial()
    {
        PlayerPrefs.SetInt(TutorialPrefsKey, 1);
        PlayerPrefs.Save();

        GameSettings.Instance.InputLocked = false;
        gameObject.SetActive(false); // vypne celý panel
    }

    void UpdateButtons()
    {
        count.text = (currentIndex + 1).ToString() + "/" + (tutorialSteps.Count).ToString();
        backButton.interactable = currentIndex > 0;

        bool isFirst = currentIndex == 0;
        bool isLast = currentIndex == tutorialSteps.Count - 1;

        backButton.interactable = !isFirst;
        nextButton.interactable = !isLast;
        finishButton.interactable = isLast;



    }
    
}
