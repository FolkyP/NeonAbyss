using UnityEngine;
using System.Collections;

public class ShieldCrystal : MonoBehaviour
{
    public int health = 5;
    public bool destroyed = false;
    public AudioClip breakSound;               // zvuk pøi rozbití
    public float destroyDelay = 0.15f;         // malý delay pro animace
    public bool destroyParentPanelOnBreak = true;
   
    public void TakeDamage(int dmg)
    {
        if (destroyed) return;

        health -= dmg;
        if (health <= 0)
        {
            StartCoroutine(BreakRoutine());
        }
    }
    IEnumerator BreakRoutine()
    {
        destroyed = true;

       

        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, Camera.main != null ? Camera.main.transform.position : transform.position);
        }

        yield return new WaitForSeconds(destroyDelay);

        GameObject parentPanel = transform.parent != null ? transform.parent.gameObject : null;
        BossManager.Instance.CrystalDestroyed(parentPanel);

        // zniè krystal (nebo deaktivuj)
        Destroy(gameObject);
    }
}

