using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class Monitor : MonoBehaviour
{
    public CrystalDepositPoint crystalDepositPoint;
    public GameObject pressE_UI;

    public Material monitorOnMaterial;
    public GameObject monitorSec;
    public GameObject motirorTer;

    public Text timeText;

    [Header("Timers")]
    public float timerCloseGate = 10f;
    public float timerPause = 20f;
    public float timerSurvive = 30f;

    private bool playerInRange;
    private bool hasBeenUsed;

    public AudioClip gate;
    public AudioClip beep;

    public Transform leftDoor;
    public Transform rightDoor;
    public Collider gateCollider;

    public Vector3 leftDoorOffset = new Vector3(-2f, 0, 0);
    public Vector3 rightDoorOffset = new Vector3(2f, 0, 0);
    public float speed = 1f;

    private bool opening;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private float nextBeepTime;

    public WeaponManager weaponManager;

    public Collider elevator; //Cutscena + nacteni finalbosse 

    private bool phaseFinished = false;
    public GameObject deathcolliderColor;
    

    private void Start()
    {
        timeText.gameObject.SetActive(false);
        pressE_UI.SetActive(false);

        monitorOnMaterial.color = Color.green;
        monitorSec.GetComponent<MeshRenderer>().material.color = Color.green;
        motirorTer.GetComponent<MeshRenderer>().material.color = Color.green;

        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        leftOpenPos = leftClosedPos + leftDoorOffset;
        rightOpenPos = rightClosedPos + rightDoorOffset;

    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !hasBeenUsed)
        {
            StartSequence();
        }

        if (opening)
        {
            leftDoor.localPosition = Vector3.Lerp(
                leftDoor.localPosition,
                leftOpenPos,
                Time.deltaTime * speed
            );

            rightDoor.localPosition = Vector3.Lerp(
                rightDoor.localPosition,
                rightOpenPos,
                Time.deltaTime * speed
            );
        }

        if (elevator != null)
        {
            CheckElevatorTrigger();

        }
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

    private void StartSequence()
    {
        hasBeenUsed = true;
        pressE_UI.SetActive(false);

        PlayerCam.Instance.Shake(5f, 0.5f);

        monitorOnMaterial.color = Color.red;
        monitorSec.GetComponent<MeshRenderer>().material.color = Color.red;
        motirorTer.GetComponent<MeshRenderer>().material.color = Color.red;

        StartCoroutine(MainSequence());
    }

    private IEnumerator MainSequence()
    {
        // 10s  zavøení brány
        deathcolliderColor.SetActive(true);
        yield return Countdown(timerCloseGate,1);
        crystalDepositPoint.CloseGate();
        deathcolliderColor.SetActive(false);

        // 20s pauza
        AudioSettings.Instance.MuteMusic(true);

        yield return Countdown(timerPause,2);
        StartPhase2();
        AudioSettings.Instance.MuteMusic(false);


        // 30s pøežití
        yield return Countdown(timerSurvive,3);
        EndSurvive();
        
    }

    private IEnumerator Countdown(float duration,int cisilko)
    {
        float timeLeft = duration;
        nextBeepTime = 0f;

        timeText.gameObject.SetActive(true);

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.CeilToInt(timeLeft % 60f);

            switch (cisilko)
            {
                case 1:
                    timeText.text = $"Get out! {minutes:00}:{seconds:00}";
                    break;
                case 2:
                    timeText.text = $"Time-out {minutes:00}:{seconds:00}";
                    break;
                case 3:
                    timeText.text = $"Survive! {minutes:00}:{seconds:00}";
                    break;
            }

            if (timeLeft <= 5f)
            {
                timeText.color = Color.red;

                if (Time.time >= nextBeepTime)
                {
                    nextBeepTime = Time.time + 1f;
                    AudioSettings.Instance.PlaySFX(beep);
                }
            }
            else
            {
                timeText.color = Color.green;
            }

            yield return null;
        }

        timeText.gameObject.SetActive(false);
    }

   

    private void StartPhase2()
    {
        Debug.Log("START PHASE 2");

        //opening = true;

        if (SpawnManager.Instance != null)
            SpawnManager.Instance.EnterPhase2();

            weaponManager.LoadAllGuns();
    }

    private void EndSurvive()
    {
        Debug.Log("Survive complete – phase finished");
        opening = true;
        AudioSettings.Instance.PlaySFX(gate);
        gateCollider.enabled = false;
        timeText.gameObject.SetActive(false);

        if (SpawnManager.Instance != null)
            SpawnManager.Instance.StopAndKillAll();
        phaseFinished = true;
        elevator.gameObject.SetActive(true);

    }
    private void CheckElevatorTrigger()
    {
        

        if (!phaseFinished)
            return; // elevator je aktivní až po dokonèení fáze

        Bounds bounds = elevator.bounds;

        Collider[] hits = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            elevator.transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                phaseFinished = false;

                StartCoroutine(StartElevatorSequence());
                return;
            }
        }
    }
    private IEnumerator StartElevatorSequence()
    {
        Debug.Log("ELEVATOR CUTSCENE START");


        GameSettings.Instance.playerUI.SetActive(false);


        if (CutsceneManager.Instance != null && CutsceneManager.Instance.endMapConfig != null)
        {
            CutsceneManager.Instance.endMapConfig.useCutsceneCamera = false;
            CutsceneManager.Instance.PlayEndMapCutscene(); // volá PlayCutscene(endMapConfig)
            GameSettings.Instance.LoadNextMap();

        }
        else
        {
            Debug.LogWarning("CutsceneManager nebo endMapConfig není nastavený.");
        }

        yield return new WaitUntil(() => CutsceneManager.Instance == null || !CutsceneManager.Instance.isCutscenePlaying);


        yield return null;

    }
}
