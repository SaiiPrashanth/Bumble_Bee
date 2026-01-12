using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private int honeyAmount = 1;

    void Start()
    {
        EnsureCollider();
    }

    void EnsureCollider()
    {
        if (!TryGetComponent<Collider>(out var col)) return;
        
        col.isTrigger = true;
        
        if (col is SphereCollider sphere)
        {
            sphere.radius = Mathf.Max(sphere.radius, 2f);
        }
        else if (col is BoxCollider box)
        {
            box.size = Vector3.Max(box.size, Vector3.one * 3f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if other is a player or collector
        if (other.CompareTag("Player") || other.CompareTag("Bee") || other.name == "FlowerCollector")
        {
            Collect();
        }
    }

    void Collect()
    {
        if (HoneyManager.Instance != null)
        {
            HoneyManager.Instance.AddHoney(honeyAmount);
        }
        
        Destroy(gameObject);
    }
}