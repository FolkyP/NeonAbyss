using UnityEngine;

public class RectPreview : MonoBehaviour
{
    public float duration = 2f;
    public float length = 4f;
    public float width = 1.2f;
    private float timer = 0f;
    private Transform t;

    void Awake() { t = transform; t.localScale = Vector3.one; }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / duration);

        // roste do délky
        float curLength = Mathf.Lerp(0.1f, length, progress);
        transform.localScale = new Vector3(width, 1f, curLength);

        if (timer >= duration)
            Destroy(gameObject);
    }
}
