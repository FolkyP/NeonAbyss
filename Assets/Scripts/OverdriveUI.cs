using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverdriveUI : MonoBehaviour
{
    [Header("UI refs")]
    public Slider chargeSlider;         // slider: min=0, max=100
    public TMP_Text percentText;

    [Tooltip("Seznam UI GameObjects, jejich CanvasGroup bude plynule pøecházet mezi 0.5 a 1.0")]
    public List<GameObject> ui = new List<GameObject>();

    [Tooltip("Flash GameObject (CanvasGroup) pøechází mezi 0.2 a 0.5)")]
    public GameObject flash;

    [Header("Transition")]
    public float transitionDuration = 0.18f;

    // interní
    private List<CanvasGroup> uiGroups = new List<CanvasGroup>();
    private CanvasGroup flashGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Ensure slider bounds
        if (chargeSlider != null)
        {
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 100f;
        }

        // Ensure CanvasGroups for ui items
        uiGroups.Clear();
        foreach (var go in ui)
        {
            if (go == null) continue;
            uiGroups.Add(GetOrAddCanvasGroup(go));
        }

        if (flash != null)
            flashGroup = GetOrAddCanvasGroup(flash);
    }

    private void Start()
    {
        // Robustní pøihlášení: poèkej pár frameù, pokud Instance ještì není pøipravená
        StartCoroutine(WaitAndSubscribeToOverdrive(5, 0.02f));
    }

    private IEnumerator WaitAndSubscribeToOverdrive(int maxFrames, float waitPerFrame)
    {
        int i = 0;
        while (Overdrive.Instance == null && i < maxFrames)
        {
            i++;
            yield return new WaitForSeconds(waitPerFrame);
        }

        if (Overdrive.Instance == null)
        {
            Debug.LogWarning($"OverdriveUI: Overdrive.Instance was null after waiting {i} frames. UI will not receive updates.");
            // Still set initial visuals to 0 (or keep current)
            OnChargeChanged(0f);
            yield break;
        }

        // Subscribe
        Overdrive.Instance.OnChargeChanged += OnChargeChanged;
        Overdrive.Instance.OnFullyCharged += OnFullyCharged;

        // Inicializuj UI podle aktuální hodnoty
        OnChargeChanged(Overdrive.Instance.currentPercent);

        Debug.Log("OverdriveUI: subscribed to Overdrive events.");
    }

    private void OnDestroy()
    {
        if (Overdrive.Instance != null)
        {
            Overdrive.Instance.OnChargeChanged -= OnChargeChanged;
            Overdrive.Instance.OnFullyCharged -= OnFullyCharged;
        }
    }

    private void OnChargeChanged(float percent)
    {
        // debug log pro ovìøení, že dostáváme volání
        Debug.Log($"OverdriveUI.OnChargeChanged: {percent:0.##}%");

        // Update slider and percent text
        if (chargeSlider != null)
            chargeSlider.value = percent;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(percent).ToString() + "%";

        // Decide target alphas
        bool isFull = percent >= 100f - 0.001f;
        float targetUiAlpha = isFull ? 1f : 0.5f;
        float targetFlashAlpha = isFull ? 0.75f : 0.2f;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeGroupsTo(uiGroups, flashGroup, targetUiAlpha, targetFlashAlpha, transitionDuration));
    }

    private void OnFullyCharged()
    {
        Debug.Log("OverdriveUI: OnFullyCharged triggered.");
        OnChargeChanged(100f);
    }

    private IEnumerator FadeGroupsTo(List<CanvasGroup> groups, CanvasGroup flash, float targetUiAlpha, float targetFlashAlpha, float duration)
    {
        float elapsed = 0f;
        List<float> startAlphas = new List<float>(groups.Count);
        for (int i = 0; i < groups.Count; i++)
            startAlphas.Add(groups[i] != null ? groups[i].alpha : 0f);

        float startFlash = flash != null ? flash.alpha : 0f;

        if (duration <= 0f)
        {
            for (int i = 0; i < groups.Count; i++)
                if (groups[i] != null) groups[i].alpha = targetUiAlpha;
            if (flash != null) flash.alpha = targetFlashAlpha;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float k = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < groups.Count; i++)
                if (groups[i] != null)
                    groups[i].alpha = Mathf.Lerp(startAlphas[i], targetUiAlpha, k);

            if (flash != null)
                flash.alpha = Mathf.Lerp(startFlash, targetFlashAlpha, k);

            yield return null;
        }

        for (int i = 0; i < groups.Count; i++)
            if (groups[i] != null) groups[i].alpha = targetUiAlpha;
        if (flash != null) flash.alpha = targetFlashAlpha;

        fadeCoroutine = null;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        if (go == null) return null;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
