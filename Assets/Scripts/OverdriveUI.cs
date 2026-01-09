using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverdriveUI : MonoBehaviour
{
    //[Header("UI refs")]
    //public Slider chargeSlider;         // nastavit min 0 max 100
    //public TMP_Text percentText;
    //public GameObject fullFlash;        // volitelné: vizuální efekt když je plný

    //private void Start()
    //{
    //    if (Overdrive.Instance != null)
    //    {
    //        Overdrive.Instance.OnChargeChanged += UpdateUI;
    //        Overdrive.Instance.OnFullyCharged += OnFullyCharged;
    //        UpdateUI(Overdrive.Instance.currentPercent);
    //    }
    //}

    //private void OnDestroy()
    //{
    //    if (Overdrive.Instance != null)
    //    {
    //        Overdrive.Instance.OnChargeChanged -= UpdateUI;
    //        Overdrive.Instance.OnFullyCharged -= OnFullyCharged;
    //    }
    //}

    //private void UpdateUI(float percent)
    //{
    //    if (chargeSlider != null)
    //    {
    //        chargeSlider.value = percent;
    //    }

    //    if (percentText != null)
    //    {
    //        percentText.text = Mathf.RoundToInt(percent).ToString() + "%";
    //    }

    //    if (fullFlash != null)
    //        fullFlash.SetActive(percent >= 100f);
    //}

    //private void OnFullyCharged()
    //{
    //    // krátká animace / zvuk / bliknutí
    //    // zde mùžeš spustit animator nebo particle efekt
    //    // example:
    //    // StartCoroutine(FlashCoroutine());
    //}
}
