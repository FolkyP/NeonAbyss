using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VignetteEffect : MonoBehaviour
{
    public Shader vignetteShader;
    private Material vignetteMaterial;

    [ColorUsage(false, true)]
    public Color vignetteColor = Color.black;
    [Range(0f, 1f)]
    public float intensity = 0.5f;
    [Range(0.1f, 3f)]
    public float smoothness = 1.5f;

    void Start()
    {
        if (vignetteShader == null)
        {
            Debug.LogError("VignetteEffect: shader chybí!");
            enabled = false;
            return;
        }

        vignetteMaterial = new Material(vignetteShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (vignetteMaterial == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        vignetteMaterial.SetColor("_Color", vignetteColor);
        vignetteMaterial.SetFloat("_Intensity", intensity);
        vignetteMaterial.SetFloat("_Smoothness", smoothness);

        Graphics.Blit(src, dest, vignetteMaterial);
    }

    /// Nastaví intenzitu podle „rychlosti“ (0..1)
    public void SetIntensityFromSpeed(float normalizedSpeed,Color c)
    {
        intensity = normalizedSpeed; // nebo mapujte k funkci, napø. exponenciálnì
        vignetteColor = c;
    }
}
