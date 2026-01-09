using UnityEngine;

public class OverdriveRadiusVisualizer : MonoBehaviour
{
    [Header("References")]
    public Overdrive overdrive;          // optional, uses Overdrive.Instance if null
    public GameObject radiusPrefab;      // prefab s Quad/Plane mesh + prùhlednou kruhovou texturou

    [Header("Visual Settings")]
    public Color notReadyColor = new Color(1f, 1f, 1f, 0.25f);
    public Color readyColor = new Color(1f, 0.6f, 0.1f, 0.75f);
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.03f;
    public float groundOffset = 0.02f;

    [Header("Ground Snap")]
    public LayerMask groundMask;

    private GameObject radiusInstance;
    private Material radiusMaterial;
    private float baseDiameter;          // prùmìr = overdriveRange * 2
    private Vector3 baseRotationEuler;

    // automaticky detekované lokální osy, na kterých musíme mìnit velikost (0=x,1=y,2=z)
    private int axisA = 0;
    private int axisB = 2;
    public PlayerMovement playerMovement;
    public bool hideWhenAirborne = true;
    private void Start()
    {
        if (overdrive == null)
            overdrive = Overdrive.Instance;

        if (radiusPrefab == null)
        {
            Debug.LogError("OverdriveRadiusVisualizer: radiusPrefab není nastavený.");
            return;
        }

        SpawnRadius();

        // ihned synchronizovat stav (kritické)
        UpdateVisual(overdrive != null ? overdrive.currentPercent : 0f);

        if (overdrive != null)
            overdrive.OnChargeChanged += UpdateVisual;
    }

    private void OnDestroy()
    {
        if (overdrive != null)
            overdrive.OnChargeChanged -= UpdateVisual;
    }

    private void SpawnRadius()
    {
        radiusInstance = Instantiate(radiusPrefab, transform);
        radiusInstance.name = radiusPrefab.name + "_Instance";

        Renderer rend = radiusInstance.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogError("OverdriveRadiusVisualizer: radiusPrefab nemá Renderer.");
            return;
        }

        radiusMaterial = rend.material;

        // zjistíme mesh, abychom detekovali aktivní osy (u Quad to budou X a Y, u Plane X a Z)
        MeshFilter mf = radiusInstance.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Vector3 meshSize = mf.sharedMesh.bounds.size;
            // považujeme osu za "aktivní", pokud má nenulovou velikost v mesh bounds
            bool xActive = meshSize.x > 0.001f;
            bool yActive = meshSize.y > 0.001f;
            bool zActive = meshSize.z > 0.001f;

            // vybereme dvì osy, které jsou aktivní; prioritnì X potom Y potom Z
            if (xActive && yActive)
            {
                axisA = 0; // x
                axisB = 1; // y
            }
            else if (xActive && zActive)
            {
                axisA = 0; // x
                axisB = 2; // z
            }
            else if (yActive && zActive)
            {
                axisA = 1; // y
                axisB = 2; // z
            }
            else
            {
                // fallback -> použijeme X a Z
                axisA = 0;
                axisB = 2;
            }

            Debug.Log($"OverdriveRadiusVisualizer: detected mesh size {meshSize} -> active axes {axisA},{axisB}");
        }
        else
        {
            // žádný mesh: fallback na X/Z (typické pro Plane)
            axisA = 0;
            axisB = 2;
            Debug.LogWarning("OverdriveRadiusVisualizer: MeshFilter/sharedMesh nenalezen - fallback na osy X/Z.");
        }

        // spoèítáme baseDiameter
        baseDiameter = overdrive != null ? overdrive.overdriveRange * 2f : 1f;

        // sestavíme scale v závislosti na detekovaných osách
        Vector3 scale = Vector3.one;
        scale[axisA] = baseDiameter;
        scale[axisB] = baseDiameter;
        // tøetí osa necháme 1 tak, aby quad/plane zùstalo tenké
        radiusInstance.transform.localScale = scale;

        baseRotationEuler = radiusInstance.transform.localEulerAngles;

        radiusInstance.SetActive(false);
    }

   private void LateUpdate()
    {
        if (radiusInstance == null) return;

        // vyhodnotit, zda by se vizualizér mìl zobrazit
        bool ready = (overdrive != null && overdrive.currentPercent >= 100f);
        bool grounded = (playerMovement != null) ? playerMovement.grounded : true;
        bool shouldShow = ready && (!hideWhenAirborne || grounded);

        radiusInstance.SetActive(shouldShow);

        if (!shouldShow)
            return;

        // pokud viditelný -> snap na zem a pøípadnì pulse
        SnapToGround();

        if (ready)
            Pulse();
    }

    private void UpdateVisual(float percent)
    {
        if (radiusInstance == null || radiusMaterial == null) return;

        bool ready = percent >= 100f;
        radiusInstance.SetActive(ready);

        Color c = Color.Lerp(notReadyColor, readyColor, Mathf.Clamp01(percent / 100f));
        if (radiusMaterial.HasProperty("_Color"))
            radiusMaterial.SetColor("_Color", c);
        else if (radiusMaterial.HasProperty("_BaseColor"))
            radiusMaterial.SetColor("_BaseColor", c);

        // synchronizuj baseDiameter pokud se mìní runtime
        if (overdrive != null)
        {
            baseDiameter = overdrive.overdriveRange * 2f;
            if (!(overdrive.currentPercent >= 100f))
            {
                Vector3 s = radiusInstance.transform.localScale;
                s[axisA] = baseDiameter;
                s[axisB] = baseDiameter;
                radiusInstance.transform.localScale = s;
            }
        }
    }

    private void Pulse()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float currentDiameter = baseDiameter * pulse;

        Vector3 s = radiusInstance.transform.localScale;
        s[axisA] = currentDiameter;
        s[axisB] = currentDiameter;
        radiusInstance.transform.localScale = s;
    }

    private void SnapToGround()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 2f;
        float maxDistance = 5f;

        if (Physics.Raycast(origin, Vector3.down, out hit, maxDistance, groundMask))
        {
            radiusInstance.transform.position = hit.point + hit.normal * groundOffset;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            radiusInstance.transform.rotation = rot * Quaternion.Euler(baseRotationEuler);
        }
        else
        {
            radiusInstance.transform.position = transform.position + Vector3.up * groundOffset;
            radiusInstance.transform.rotation = Quaternion.Euler(baseRotationEuler);
        }
    }
}
