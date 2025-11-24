using UnityEngine;

public class SimpleSpiderLeg : MonoBehaviour
{
    public Transform body;            // celé tìlo pavouka
    public Transform target;          // FL_Target / FR_Target / BL_Target / BR_Target
    public Transform tip;             // Foot_end_end, nebo null (pak se použije target)

    public SimpleSpiderLeg oppositeLeg; // diagonální noha

    public float stepDistance = 0.5f;
    public float stepHeight = 0.25f;
    public float stepSpeed = 5f;

    Vector3 lastPos;
    Vector3 nextPos;
    float progress = 1f;

    void Start()
    {
        lastPos = target.position;
        nextPos = target.position;
    }

    void  LateUpdate()

    {
        // Krok se spustí jen když diagonální noha nekroèí
        if (progress >= 1f && !oppositeLeg.IsStepping())
        {
            Vector3 desired = GetDesiredFootPosition();

            if (Vector3.Distance(desired, nextPos) > stepDistance)
            {
                lastPos = target.position;
                nextPos = desired;
                progress = 0f;
            }
        }

        // Animace kroku
        if (progress < 1f)
        {
                Debug.Log(name + " stepping");

            progress += Time.deltaTime * stepSpeed;
            float t = Mathf.Clamp01(progress);

            Vector3 pos = Vector3.Lerp(lastPos, nextPos, t);
            //pos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;
            pos.y += stepHeight;   // noha bude *vždy nahoøe*


            target.position = pos;
        }

        // Natáèení kosti
        transform.position = target.position;

        if (tip != null)
            transform.LookAt(tip.position);
        else
            transform.LookAt(target.position);
    }

    Vector3 GetDesiredFootPosition()
    {
        // Raycast pøímo dolù z pozice nohy
        Ray r = new Ray(body.position + transform.localPosition, Vector3.down);

        if (Physics.Raycast(r, out RaycastHit hit, 2f))
            return hit.point;

        return target.position;
    }

    public bool IsStepping() => progress < 1f;
}
