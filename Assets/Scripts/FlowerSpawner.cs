using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private Transform landscapeParent;
    
    [Header("Spawn Settings")]
    [SerializeField] private int totalFlowers = 50;
    [SerializeField] private float edgeBuffer = 50f;
    [SerializeField] private float spawnHeightCheck = 500f;
    
    private List<Collider> validSpawnSurfaces = new List<Collider>();
    private Bounds spawnBounds;
    private bool hasSpawned;
    
    void Start()
    {
        if (HoneyManager.Instance != null)
        {
            HoneyManager.Instance.SetTotalFlowers(totalFlowers);
        }
        
        FindSpawnSurfaces();
        CalculateBounds();
        
        StartCoroutine(SpawnSequence());
    }
    
    void FindSpawnSurfaces()
    {
        if (landscapeParent == null) return;
        
        var colliders = landscapeParent.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            // Filter by name or assume all are valid if needed
            string name = col.name.ToLower();
            if (name.Contains("grass") || name.Contains("lawn") || name.Contains("field"))
            {
                validSpawnSurfaces.Add(col);
            }
        }
        
        // Fallback: Use all if no specific "grass" found
        if (validSpawnSurfaces.Count == 0)
        {
            foreach (var col in colliders)
            {
                if (col.bounds.size.magnitude > 1f) validSpawnSurfaces.Add(col);
            }
        }
    }
    
    void CalculateBounds()
    {
        if (validSpawnSurfaces.Count > 0)
        {
            spawnBounds = validSpawnSurfaces[0].bounds;
            foreach (var col in validSpawnSurfaces) spawnBounds.Encapsulate(col.bounds);
        }
        else
        {
            spawnBounds = new Bounds(Vector3.zero, Vector3.one * 500f);
        }
        
        // Shrink for buffer
        Vector3 size = spawnBounds.size;
        size.x -= edgeBuffer * 2;
        size.z -= edgeBuffer * 2;
        spawnBounds.size = size;
    }
    
    IEnumerator SpawnSequence()
    {
        if (hasSpawned) yield break;
        hasSpawned = true;
        
        int count = 0;
        int attempts = 0;
        int maxAttempts = totalFlowers * 10;
        
        while (count < totalFlowers && attempts < maxAttempts)
        {
            if (SpawnFlower())
            {
                count++;
                if (count % 5 == 0) yield return null; // Yield every 5 spawns to spread load
            }
            attempts++;
        }
        
        Debug.Log($"FlowerSpawner: Spawned {count}/{totalFlowers} flowers.");
    }
    
    bool SpawnFlower()
    {
        if (flowerPrefab == null || validSpawnSurfaces.Count == 0) return false;
        
        Vector3 randomPos = new Vector3(
            Random.Range(spawnBounds.min.x, spawnBounds.max.x),
            spawnHeightCheck,
            Random.Range(spawnBounds.min.z, spawnBounds.max.z)
        );
        
        if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 1000f, -1, QueryTriggerInteraction.Ignore))
        {
            if (validSpawnSurfaces.Contains(hit.collider))
            {
                // Validate height
                if (hit.point.y < 0f || hit.point.y > 150f) return false;
                
                Vector3 spawnPos = hit.point + Vector3.up * 0.1f;
                Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                
                Instantiate(flowerPrefab, spawnPos, spawnRot, transform);
                return true;
            }
        }
        
        return false;
    }
}

