using UnityEngine;
using TMPro;

public class CrystalDepositPoint : MonoBehaviour
{
    public GameObject pressE_UI;          // UI text/ikonka "Press E"
    public CrystalManager crystalManager; // reference na CrystalManager
    public GameObject[] indicators;       // 4 objekty, které se aktivují podle vložení

    private bool playerInRange = false;
    private bool hasBeenUsed = false;



    public AudioClip gate;
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
    private void Start()
    {
        pressE_UI.SetActive(false);
        DisableIndicators();

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
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            AddCrystalsToDeposit();
        }
        if (opening)
        {
            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, leftOpenPos, Time.deltaTime * speed);
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, rightOpenPos, Time.deltaTime * speed);
        }
    }

    private void AddCrystalsToDeposit()
    {
        int amount = crystalManager.crystalCount;

        if (amount <= 0) return;   


        UpdateIndicators(amount);
        pressE_UI.SetActive(false); // zabrání opakovanému vložení
    }

    private void UpdateIndicators(int amount)
    {
        for (int i = 0; i < indicators.Length; i++)
        {
            indicators[i].SetActive(i < amount);
        }
        if (amount >= 4 && !hasBeenUsed)
            OnAllCrystalsPlaced();
    }
    private void OnAllCrystalsPlaced()
    {
        hasBeenUsed = true;
        opening = true;
        AudioSettings.Instance.PlaySFX(gate);
        if (gateCollider != null)
            gateCollider.enabled = false;
    }

    private void DisableIndicators()
    {
        foreach (var ind in indicators)
            ind.SetActive(false);
    }
}
