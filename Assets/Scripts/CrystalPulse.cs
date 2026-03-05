using UnityEngine;

public class CrystalPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;

    private Renderer rend;
    private Color colorA = Color.red;
    private Color colorB = Color.green;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend == null)
            rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // smooth 0..1
        rend.material.color = Color.Lerp(colorA, colorB, t);
    }
}