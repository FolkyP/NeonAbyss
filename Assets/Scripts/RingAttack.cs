using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class RingAttack : MonoBehaviour
{
    [Header("Ring Timing / Shape")]
    public float expandSpeed = 6f;        // units per second (radius increase)
    public float maxRadius = 12f;         // when exceeded, ring destroys itself
    public float thickness = 1.2f;        // ring band width (meters)
    public float visualLineWidth = 0.12f; // line renderer width
    [Range(8, 128)]
    public int lineSegments = 64;         // smoothness of circle

    [Header("Damage")]
    public int damage = 25;
    public LayerMask playerLayerMask = ~0; // set to only Player layer for efficiency
    public bool hitEachPlayerOnce = true;  // true: a player can be damaged only once by this ring

    [Header("Visuals (optional)")]
    public Material lineMaterial;         // assign a material for the ring line (optional)
    public Color lineColor = Color.red;

    // runtime
    private float currentRadius = 0f;
    private LineRenderer lr;
    private HashSet<Collider> alreadyHit = new HashSet<Collider>();

    void Awake()
    {
        // Setup LineRenderer if desired
        lr = gameObject.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.positionCount = lineSegments;
        lr.useWorldSpace = false; // we will rotate/position object in world; points are local
        lr.widthCurve = new AnimationCurve(new Keyframe(0, visualLineWidth), new Keyframe(1, visualLineWidth));
        lr.numCornerVertices = 6;
        lr.numCapVertices = 6;
        if (lineMaterial != null) lr.material = lineMaterial;
        lr.startColor = lr.endColor = lineColor;
    }

    void Start()
    {
        // optional: ensure first frame visual is correct
        UpdateLineRenderer();
    }

    void Update()
    {
        // expand
        currentRadius += expandSpeed * Time.deltaTime;

        // update visual
        UpdateLineRenderer();

        // check collisions for players
        CheckAndApplyDamage();

        // destroy when done
        if (currentRadius - (thickness * 0.5f) > maxRadius)
        {
            Destroy(gameObject);
        }
    }

    void UpdateLineRenderer()
    {
        if (lr == null) return;

        float angleStep = 360f / lineSegments;
        Vector3[] points = new Vector3[lineSegments];

        for (int i = 0; i < lineSegments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Sin(angle) * currentRadius;
            float z = Mathf.Cos(angle) * currentRadius;
            points[i] = new Vector3(x, 0f, z);
        }

        lr.positionCount = lineSegments;
        lr.SetPositions(points);
    }

    void CheckAndApplyDamage()
    {
        // 1. Determine safe vertical height (how high player must jump)
        // You can make this a public variable if you want to tune it in Inspector
        float jumpSafetyHeight = 0.5f;

        // Limit overlap radius to region that can contain ring
        float checkRadius = currentRadius + thickness * 0.5f;

        // Query global candidates
        Collider[] cols = Physics.OverlapSphere(transform.position, checkRadius, playerLayerMask, QueryTriggerInteraction.Collide);

        for (int i = 0; i < cols.Length; i++)
        {
            Collider c = cols[i];
            if (c == null) continue;

            if (hitEachPlayerOnce && alreadyHit.Contains(c)) continue;

            Vector3 closest = c.ClosestPoint(transform.position);

            // --- FIX STARTS HERE ---

            // 2. Check Height (Y axis) FIRST
            // If the point of contact is too high above the ring's center, the player jumped over it.
            float verticalDistance = closest.y - transform.position.y;
            if (verticalDistance > jumpSafetyHeight)
                continue; // Player is safe in the air

            // 3. Calculate Flat Distance (XZ only)
            // We ignore Y here to treat the attack as a flat circle
            float flatDistance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z),
                new Vector2(closest.x, closest.z)
            );

            // 4. Check if the flat distance is within the ring's thickness band
            if (Mathf.Abs(flatDistance - currentRadius) <= thickness * 0.5f)
            {
                // Apply Damage
                var pl = c.GetComponentInParent<PlayerLife>();
                if (pl != null)
                {
                    pl.TakeDamage(damage);
                }
                else
                {
                    c.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                }

                if (hitEachPlayerOnce)
                    alreadyHit.Add(c);
            }
            // --- FIX ENDS HERE ---
        }
    }


}
