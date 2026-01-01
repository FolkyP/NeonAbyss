using UnityEngine;

public class ShockwavePreview : MonoBehaviour
{
    public float duration = 2f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
        // optionally animate scale or alpha
    }
}
