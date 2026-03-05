using UnityEngine;

public class MonitorRoomTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WaypointGuide.Instance?.HideGuide();
            gameObject.SetActive(false); // disable trigger after use
        }
    }
}