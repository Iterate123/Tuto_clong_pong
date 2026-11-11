using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Éléments d'UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI levelText;
    
    [Header("Panneaux")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject pausePanel;
    
    [Header("Boutons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resumeButton;
    
    private GameManager gameManager;
    private bool isPaused = false;

    private void Start()
    {
        gameManager = GameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager non trouvé. Assurez-vous qu'il existe dans la scène.");
            return;
        }
        
        // 订阅游戏事件
        SubscribeToGameEvents();
        
        // 初始化面板
        InitializePanels();
        
        // 设置按钮
        SetupButtons();
        
        // 初始化ui
        Invoke("UpdateUI", 0.1f); // 初始呼叫，以免出现问题
    }
    
    private void SubscribeToGameEvents()
    {
        gameManager.OnScoreChanged += UpdateScore;
        gameManager.OnLifeLost += UpdateLives;
        gameManager.OnGameOver += ShowGameOver;
        gameManager.OnLevelComplete += ShowLevelComplete;
        gameManager.OnGameStart += UpdateLevel; // 添加一个更新事件
    }
    
    private void InitializePanels()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
    }
    
    private void SetupButtons()
    {
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (resumeButton) resumeButton.onClick.AddListener(TogglePause);
    }
    
    private void UpdateUI()
    {
        UpdateScore();
        UpdateLives();
        UpdateTime();
        UpdateLevel(); // 级别文件更新
    }
    
    private void Update()
    {
        UpdateTime();
        
        // 检查暂停键
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    
    private void UpdateScore()
    {
        if (scoreText)
        {
            scoreText.text = "Score: " + gameManager.GetScore().ToString("D6");
        }
    }
    
    private void UpdateLives()
    {
        if (livesText)
        {
            livesText.text = "Vies: " + gameManager.GetLives().ToString();
        }
    }
    
    private void UpdateTime()
    {
        if (timeText && gameManager.IsGameRunning())
        {
            float time = gameManager.GetRemainingTime();
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = string.Format("Temps: {0:00}:{1:00}", minutes, seconds);
        }
    }
    
    private void UpdateLevel()
    {
        if (levelText)
        {
            levelText.text = "Niveau: " + gameManager.GetLevel();
        }
    }
    
    private void ShowGameOver()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    private void ShowLevelComplete()
    {
        if (levelCompletePanel)
        {
            levelCompletePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// 隐藏最终面板关卡
    /// </summary>
    public void HideLevelCompletePanel()
    {
        if (levelCompletePanel)
        {
            levelCompletePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 隐藏游戏结束面板
    /// </summary>
    public void HideGameOverPanel()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    private void TogglePause()
    {
        isPaused = !isPaused;
        
        if (pausePanel)
        {
            pausePanel.SetActive(isPaused);
        }
        
        Time.timeScale = isPaused ? 0f : 1f;
    }
    
    private void RestartGame()
    {
        // 确保游戏没有暂停
        if (isPaused)
        {
            Time.timeScale = 1f;
            isPaused = false;
            
            // 隐藏游戏面板
            if (pausePanel) pausePanel.SetActive(false);
        }
        
        // 隐藏游戏结束和关卡的提示词
        HideGameOverPanel();
        HideLevelCompletePanel();
        
        // 调用GameManager的重启功能
        gameManager.RestartGame();
        
        // 更新用户界面
        Invoke("UpdateUI", 0.5f);
    }
    
    private void OnDestroy()
    {
        // 取消订阅功能以免用户泄露
        if (gameManager != null)
        {
            gameManager.OnScoreChanged -= UpdateScore;
            gameManager.OnLifeLost -= UpdateLives;
            gameManager.OnGameOver -= ShowGameOver;
            gameManager.OnLevelComplete -= ShowLevelComplete;
            gameManager.OnGameStart -= UpdateLevel; // 取消订阅启动活动
        }
        
        // 在损坏时间的情况下启动时间轴
        Time.timeScale = 1f;
    }
}
