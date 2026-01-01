using UnityEngine;

public class LaserPreview : MonoBehaviour
{
    public float duration = 2f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration) Destroy(gameObject);
        // Optionally animate material emission here.
    }
}
