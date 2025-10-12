using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Hitmarker : MonoBehaviour
{
    public static Hitmarker Instance;

    [Header("UI References")]
    public Image hitmarkerImage;
    public GameObject damagePopupPrefab;   // assign your TMP prefab here
    public Canvas popupCanvas;             // assign your main UI canvas

    [Header("Settings")]
    public float hitmarkerDuration = 0.2f;
    public AudioClip hitSound;

    private AudioSource audioSource;
    private Coroutine hideRoutine;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        hitmarkerImage.enabled = false;
    }

    public void ShowHit(Vector3 worldPosition, float damage)
    {
        // --- Hitmarker flash ---
        if (hitmarkerImage != null)
        {
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            hitmarkerImage.enabled = true;
            hideRoutine = StartCoroutine(HideHitmarkerAfterDelay());
        }

        if (hitSound != null)
            audioSource?.PlayOneShot(hitSound);

        // --- Floating damage number ---
        if (damagePopupPrefab != null && popupCanvas != null && damage > 0f)
        {
            ShowDamagePopup(worldPosition, damage);
        }
    }

    private IEnumerator HideHitmarkerAfterDelay()
    {
        yield return new WaitForSeconds(hitmarkerDuration);
        hitmarkerImage.enabled = false;
    }

    private void ShowDamagePopup(Vector3 worldPos, float damage)
    {
        // Convert world position to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return; // ignore if behind camera

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
