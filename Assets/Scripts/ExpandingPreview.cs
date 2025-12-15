using UnityEngine;

public class ExpandingPreview : MonoBehaviour
{
    public float duration = 2f;
    public float maxScale = 6f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        float tNorm = Mathf.Clamp01(timer / duration);
        float scale = Mathf.Lerp(0.1f, maxScale, tNorm);
        transform.localScale = new Vector3(scale, scale, scale);

        // Optionally change material alpha or color based on tNorm.

        if (timer >= duration)
            Destroy(gameObject);
    }
}
