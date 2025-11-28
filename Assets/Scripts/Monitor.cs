using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Monitor : MonoBehaviour
{
    public GameObject pressE_UI;
    public Material monitorOnMaterial;

    public GameObject monitorSec;
    public GameObject motirorTer;

    public Text timeText;
    public float countdown = 20f;

    private bool countdownRunning = false;
    private bool playerInRange = false;
    private bool hasBeenUsed = false;

    public AudioClip gate;
    public AudioClip beep;

    public Transform leftDoor;
    public Transform rightDoor;
    public Collider gateCollider;

    public Vector3 leftDoorOffset = new Vector3(-2f, 0, 0);
    public Vector3 rightDoorOffset = new Vector3(2f, 0, 0);
    public float speed = 1f;

    private bool opening = false;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private float nextBeepTime = 0f;   //  kontrola pípnutí

    private void Start()
    {
        timeText.gameObject.SetActive(false);
        pressE_UI.SetActive(false);
        monitorOnMaterial.color = Color.green;
        monitorSec.GetComponent<MeshRenderer>().material.color = Color.green;
        motirorTer.GetComponent<MeshRenderer>().material.color = Color.green;
        // Uloží pùvodní pozice
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        // Vypoèítá pozice otevøení
        leftOpenPos = leftClosedPos + leftDoorOffset;
        rightOpenPos = rightClosedPos + rightDoorOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            pressE_UI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pressE_UI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !hasBeenUsed)
        {
            StartEndPhase();
        }

        if (opening)
        {
            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, leftOpenPos, Time.deltaTime * speed);
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, rightOpenPos, Time.deltaTime * speed);
        }

        if (countdownRunning)
        {
            countdown -= Time.deltaTime;

            if (countdown > 5f)
            {
                timeText.color = Color.green;
            }
            else
            {
                timeText.color = Color.red;

                // ----- Beep pouze jednou za sekundu -----
                if (Time.time >= nextBeepTime)
                {
                    nextBeepTime = Time.time + 1f; // nastaví další èas pípnout
                    AudioSettings.Instance.PlaySFX(beep);
                }
            }

            UpdateTimeDisplay();

            if (countdown <= 0f)
            {
                countdown = 0f;
                countdownRunning = false;
                EndCountdown();
            }
        }
    }

    private void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(countdown / 60f);
        int seconds = Mathf.FloorToInt(countdown % 60f);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void StartEndPhase()
    {
        hasBeenUsed = true;
        PlayerCam.Instance.Shake(5f, 0.5f);
        monitorOnMaterial.color = Color.red;
        monitorSec.GetComponent<MeshRenderer>().material.color = Color.red;
        motirorTer.GetComponent<MeshRenderer>().material.color = Color.red;
        timeText.gameObject.SetActive(true);
        countdownRunning = true;

        pressE_UI.SetActive(false);

        if (SpawnManager.Instance != null)
            SpawnManager.Instance.EnterPhase2();
    }

    private void EndCountdown()
    {
        Debug.Log("Èas vypršel! Spouštím finální funkci.");

        opening = true;
        AudioSettings.Instance.PlaySFX(gate);
        gateCollider.enabled = false;
        timeText.gameObject.SetActive(false);
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.StopAndKillAll();

    }
}
