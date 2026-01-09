using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Hitmarker : MonoBehaviour
{
    public static Hitmarker Instance;

    [Header("UI References")]
    public Image hitmarkerImage;
    public Image CritHitMarkerImage;
    public GameObject damagePopupPrefab;   
    public Canvas popupCanvas;             

    [Header("Settings")]
    public float hitmarkerDuration = 0.2f;
    public AudioClip hitSound;
    public AudioClip critSound;

    
    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;
        hitmarkerImage.enabled = false;
        CritHitMarkerImage.enabled = false;
    }

    public void ShowHit(Vector3 worldPosition, float damage,bool isCrit)
    {
        if (hitmarkerImage != null && !isCrit)
        {
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            hitmarkerImage.enabled = true;
            AudioSettings.Instance?.PlaySFX(hitSound);
            hideRoutine = StartCoroutine(HideHitmarkerAfterDelay());
        }
        if (CritHitMarkerImage != null && isCrit)
        {
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            CritHitMarkerImage.enabled = true;
            AudioSettings.Instance?.PlaySFX(critSound);
            hideRoutine = StartCoroutine(HideHitmarkerAfterDelay());
        }

        
        if (damagePopupPrefab != null && popupCanvas != null && damage > 0f)
        {
            if (isCrit)
            {
                damagePopupPrefab.GetComponent<TMP_Text>().color = Color.red;
                ShowDamagePopup(worldPosition, damage);
                Overdrive.Instance?.AddChargeFromDamage(damage*1.5f);
            }
            else
            {
                damagePopupPrefab.GetComponent<TMP_Text>().color = Color.cyan;
                ShowDamagePopup(worldPosition, damage);
                Overdrive.Instance?.AddChargeFromDamage(damage);
            }
            
        }
    }

    private IEnumerator HideHitmarkerAfterDelay()
    {
        yield return new WaitForSeconds(hitmarkerDuration);
        hitmarkerImage.enabled = false;
        CritHitMarkerImage.enabled = false;
    }

    private void ShowDamagePopup(Vector3 worldPos, float damage)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return; 

        GameObject popup = Instantiate(damagePopupPrefab, popupCanvas.transform);
        popup.transform.position = screenPos;

        TMP_Text tmp = popup.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = Mathf.RoundToInt(damage).ToString();
        }

        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        StartCoroutine(AnimateDamagePopup(popup, cg));
    }

    private IEnumerator AnimateDamagePopup(GameObject popup, CanvasGroup cg)
    {
        RectTransform rt = popup.GetComponent<RectTransform>();
        Vector3 startPos = rt.position;
        Vector3 endPos = startPos + new Vector3(0f, 60f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1f;
            rt.position = Vector3.Lerp(startPos, endPos, t);
            if (cg != null) cg.alpha = 1f - t;
            yield return null;
        }

        Destroy(popup);
    }
}
