using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGuide : MonoBehaviour
{
    public static WaypointGuide Instance;
    [Header("Packet Settings")]
    public Material packetMaterial;
    [Header("References")]
    public Transform player;
    public Transform generatorWaypoint;
    public Transform monitorWaypoint;

    [Header("Packet Settings")]
    public float lineHeight = 1f;
    public float packetSpeed = 6f;
    public float spawnInterval = 0.3f;   // time between each packet spawn
    public float packetWidth = 0.15f;
    public float packetLength = 0.5f;    // how long each "bit" is

    private Transform currentTarget;
    private bool isActive = false;
    private Coroutine spawnCoroutine;

    private List<PacketBit> activePackets = new List<PacketBit>();
    private Color currentColor = Color.cyan;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (!isActive || currentTarget == null || player == null) return;

        Vector3 start = new Vector3(player.position.x, player.position.y, player.position.z);
        Vector3 end = new Vector3(currentTarget.position.x, currentTarget.position.y, currentTarget.position.z);

        for (int i = activePackets.Count - 1; i >= 0; i--)
        {
            PacketBit p = activePackets[i];
            if (p == null || p.obj == null)
            {
                activePackets.RemoveAt(i);
                continue;
            }

            p.traveled += Time.deltaTime * packetSpeed;
            float total = Vector3.Distance(start, end);

            if (p.traveled >= total)
            {
                Destroy(p.obj);
                activePackets.RemoveAt(i);
                continue;
            }

            Vector3 dir = (end - start).normalized;
            p.obj.transform.position = start + dir * p.traveled;
            p.obj.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void SpawnPacket()
    {
        if (currentTarget == null || player == null) return;

        Vector3 start = new Vector3(player.position.x, player.position.y + lineHeight, player.position.z);
        Vector3 end = new Vector3(currentTarget.position.x, currentTarget.position.y + lineHeight, currentTarget.position.z);
        Vector3 dir = (end - start).normalized;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(obj.GetComponent<Collider>());
        obj.transform.position = start;
        obj.transform.rotation = Quaternion.LookRotation(dir);
        obj.transform.localScale = new Vector3(packetWidth, packetWidth, packetLength);

        Renderer r = obj.GetComponent<Renderer>();
        r.material = new Material(packetMaterial);
        r.material.color = currentColor;
        r.material.EnableKeyword("_EMISSION");
        r.material.SetColor("_EmissionColor", currentColor);

        activePackets.Add(new PacketBit { obj = obj, traveled = 0f });
    }

    private IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            SpawnPacket();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    

    private void ClearPackets()
    {
        foreach (var p in activePackets)
            if (p.obj != null) Destroy(p.obj);
        activePackets.Clear();
    }

    public void ShowGuideToGenerator()
    {
        ClearPackets();
        currentTarget = generatorWaypoint;
        currentColor = Color.cyan;
        isActive = true;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void ShowGuideToMonitor()
    {
        ClearPackets();
        currentTarget = monitorWaypoint;
        currentColor = Color.yellow;
        isActive = true;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void HideGuide()
    {
        isActive = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        ClearPackets();
        currentTarget = null;
    }

    private class PacketBit
    {
        public GameObject obj;
        public float traveled;
    }
}