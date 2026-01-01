using UnityEngine;

public class GroundPreview : MonoBehaviour
{
    public float duration = 2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.8f, 1, 1f);
    private float timer = 0f;
    private Transform t;
    private Vector3 baseScale;

    void Awake()
    {
        t = transform;
        baseScale = t.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float tNorm = Mathf.Clamp01(timer / duration);
        float s = scaleCurve.Evaluate(tNorm);
        t.localScale = baseScale * s;

        if (timer >= duration)
            Destroy(gameObject);
    }

}
