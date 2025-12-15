using UnityEngine;

public class GroundPreview : MonoBehaviour
{
    public float duration = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.8f, 1, 1f);
    private float timer = 0f;
    private Transform t;

    void Awake() { t = transform; }

    void Update()
    {
        timer += Time.deltaTime;
        float tNorm = Mathf.Clamp01(timer / duration);
        float s = scaleCurve.Evaluate(tNorm);
        t.localScale = new Vector3(s, s, s);

        // Optional: pulse alpha or emission via material here (if material supports it).

        if (timer >= duration)
            Destroy(gameObject);
    }
}
