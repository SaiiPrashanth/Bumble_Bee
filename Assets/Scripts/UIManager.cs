using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI honeyCounterText;
    [SerializeField] private GameObject honeyCounterPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI restartCountdownText;
    
    private bool isRestarting = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
    }
    
    void Start()
    {
        InitializeUI();
        UpdateHoneyCounter();
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
    
    void InitializeUI()
    {
        // If UI elements are missing, try to find or create them
        if (honeyCounterText == null)
        {
            // Attempt to find existing
            var existingHoney = GameObject.Find("Honey");
            if (existingHoney != null)
            {
                honeyCounterText = existingHoney.GetComponent<TextMeshProUGUI>();
            }
        }

        if (honeyCounterText == null || gameOverPanel == null)
        {
            CreateFallbackUI();
        }
    }
    
    void CreateFallbackUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Honey Counter
        if (honeyCounterPanel == null)
        {
            honeyCounterPanel = new GameObject("HoneyCounterPanel");
            honeyCounterPanel.transform.SetParent(canvas.transform, false);
            RectTransform rt = honeyCounterPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.one;
            rt.anchorMax = Vector2.one;
            rt.pivot = Vector2.one;
            rt.anchoredPosition = new Vector2(-20, -20);
        }

        if (honeyCounterText == null)
        {
            GameObject textObj = new GameObject("HoneyText");
            textObj.transform.SetParent(honeyCounterPanel.transform, false);
            honeyCounterText = textObj.AddComponent<TextMeshProUGUI>();
            honeyCounterText.fontSize = 24;
            honeyCounterText.color = Color.yellow;
            honeyCounterText.text = "0/50";
        }
        
        // Game Over Panel
        if (gameOverPanel == null)
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(canvas.transform, false);
            RectTransform rt = gameOverPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 300);
            
            Image bg = gameOverPanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Game Over Text
            GameObject goText = new GameObject("GameOverText");
            goText.transform.SetParent(gameOverPanel.transform, false);
            gameOverText = goText.AddComponent<TextMeshProUGUI>();
            gameOverText.text = "GAME OVER";
            gameOverText.color = Color.red;
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.fontSize = 36;
            
            // Countdown Text
            GameObject cdText = new GameObject("CountdownText");
            cdText.transform.SetParent(gameOverPanel.transform, false);
            restartCountdownText = cdText.AddComponent<TextMeshProUGUI>();
            restartCountdownText.alignment = TextAlignmentOptions.Center;
            restartCountdownText.fontSize = 28;
            
            RectTransform cdRect = cdText.GetComponent<RectTransform>();
            cdRect.anchoredPosition = new Vector2(0, -50);
            
            gameOverPanel.SetActive(false);
        }
    }
    
    public void UpdateHoneyCounter()
    {
        if (HoneyManager.Instance != null && honeyCounterText != null)
        {
            honeyCounterText.text = $"{HoneyManager.Instance.Honey}/{HoneyManager.Instance.TotalFlowers}";
        }
    }
    
    public void ShowGameOver()
    {
        if (isRestarting) return;
        isRestarting = true;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null && HoneyManager.Instance != null)
            {
                gameOverText.text = $"GAME OVER\nHoney: {HoneyManager.Instance.Honey}/{HoneyManager.Instance.TotalFlowers}";
            }
        }
        
        StartCoroutine(RestartSequence());
    }
    
    IEnumerator RestartSequence()
    {
        Time.timeScale = 0.5f;
        
        for (int i = 3; i > 0; i--)
        {
            if (restartCountdownText != null) restartCountdownText.text = $"Restarting in {i}...";
            yield return new WaitForSecondsRealtime(1f);
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

