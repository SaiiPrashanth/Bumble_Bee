using UnityEngine;

public class HoneyManager : MonoBehaviour
{
    public static HoneyManager Instance { get; private set; }
    
    public int Honey { get; private set; }
    public int TotalFlowers { get; private set; } = 50;
    
    private bool hasWon = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void SetTotalFlowers(int total)
    {
        TotalFlowers = total;
    }

    public void AddHoney(int amount)
    {
        if (hasWon) return;
        
        Honey += amount;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHoneyCounter();
        }

        if (Honey >= TotalFlowers && !hasWon)
        {
            WinGame();
        }
    }
    
    private void WinGame()
    {
        hasWon = true;
        Debug.Log("Victory! All honey collected.");
        // Future: Trigger UI win screen
    }
    
    public bool HasWon()
    {
        return hasWon;
    }
}


