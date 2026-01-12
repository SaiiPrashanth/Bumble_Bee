using System.Collections.Generic;
using UnityEngine;

public class WaspZoneSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetBee;
    [SerializeField] private GameObject waspPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int totalWasps = 16;
    [SerializeField] private float minDistanceFromBee = 50f;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(500f, 100f, 500f);
    [SerializeField] private float minSpawnHeight = 10f;
    [SerializeField] private float maxSpawnHeight = 80f;
    [SerializeField] private float waspRoamRadius = 200f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color areaColor = new Color(1f, 0.5f, 0f, 0.3f);

    private List<Vector3> _spawnedPositions = new List<Vector3>();

    private void Start()
    {
        if (waspPrefab == null)
        {
            Debug.LogError("WaspZoneSpawner: Wasp Prefab is not assigned!");
            return;
        }

        if (targetBee == null)
        {
            // Try to find the Bee if not assigned
            GameObject beeObj = GameObject.FindGameObjectWithTag("Player");
            if (beeObj == null) beeObj = GameObject.Find("Bee");
            
            if (beeObj != null)
            {
                targetBee = beeObj.transform;
            }
            else
            {
                Debug.LogWarning("WaspZoneSpawner: Target Bee not found. Wasps will roam freely without a target.");
            }
        }

        SpawnWasps();
    }

    private void SpawnWasps()
    {
        int count = 0;
        int attempts = 0;
        int maxAttempts = totalWasps * 10;

        _spawnedPositions.Clear();
        Debug.Log($"WaspZoneSpawner: Spawning {totalWasps} wasps...");

        while (count < totalWasps && attempts < maxAttempts)
        {
            attempts++;

            // Calculate random position within bounds
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
                Random.Range(minSpawnHeight, maxSpawnHeight),
                Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
            );

            // Check distance from bee if bee exists
            if (targetBee != null && Vector3.Distance(randomPos, targetBee.position) < minDistanceFromBee)
            {
                continue;
            }

            SpawnSingleWasp(randomPos);
            _spawnedPositions.Add(randomPos);
            count++;
        }

        Debug.Log($"WaspZoneSpawner: Spawned {count} wasps.");
    }

    private void SpawnSingleWasp(Vector3 position)
    {
        GameObject waspObj = Instantiate(waspPrefab, position, Quaternion.identity);
        waspObj.name = $"Wasp_{_spawnedPositions.Count + 1}";

        WaspBehaviour wasp = waspObj.GetComponent<WaspBehaviour>();
        if (wasp != null)
        {
            wasp.spawnPoint = position;
            wasp.roamRadius = waspRoamRadius;
            wasp.target = targetBee;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw Spawn Area
        Gizmos.color = areaColor;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);

        // Draw Min/Max Height guides
        Gizmos.color = new Color(areaColor.r, areaColor.g, areaColor.b, 0.1f);
        Vector3 center = transform.position;
        Vector3 size = spawnAreaSize;
        
        Vector3 minH = new Vector3(center.x, minSpawnHeight, center.z);
        Vector3 maxH = new Vector3(center.x, maxSpawnHeight, center.z);
        
        Gizmos.DrawWireCube(minH, new Vector3(size.x, 0.1f, size.z));
        Gizmos.DrawWireCube(maxH, new Vector3(size.x, 0.1f, size.z));

        // Draw Spawned Positions
        Gizmos.color = Color.yellow;
        foreach (var pos in _spawnedPositions)
        {
            Gizmos.DrawSphere(pos, 1f);
            Gizmos.DrawLine(new Vector3(pos.x, 0, pos.z), pos);
        }

        // Draw Min Distance from Bee
        if (targetBee != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(targetBee.position, minDistanceFromBee);
        }
    }
}
