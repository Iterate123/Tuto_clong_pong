using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 在游戏中配置碰撞层
            SetupCollisionLayers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private const string BALL_LAYER_NAME = "Balls";
    private const int BALL_LAYER = 8; //在unity中，layer 8通常用于自定义层，确保在项目设置中创建了一个名为"Balls"的层，并将其分配给所有球体对象。
    private const int MAX_LEVEL = 4; // 游戏中的最大关卡数

    [Header("Niveau")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float levelTime = 120f;
    [SerializeField] private int initialBallCount = 1;

    [Header("Joueur")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int lives = 3;

    [Header("Balles")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private List<Color> ballColors = new List<Color>();

    // État du jeu
    private bool isGameRunning = false;
    private int score = 0;
    private float remainingTime;
    private int remainingLives;

    // Événements du jeu
    public delegate void GameEvent();
    public event GameEvent OnGameStart;
    public event GameEvent OnGameOver;
    public event GameEvent OnLevelComplete;
    public event GameEvent OnScoreChanged;
    public event GameEvent OnLifeLost;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (isGameRunning)
        {
            UpdateTimer();
            CheckLevelCompletion();
        }
    }

    private void InitializeGame()
    {
        score = 0;
        remainingLives = lives;
        remainingTime = levelTime;
        isGameRunning = true;
        
        SpawnPlayer();
        SpawnInitialBalls();
        
        OnGameStart?.Invoke();
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;
        
        if (remainingTime <= 0)
        {
            GameOver();
        }
    }

    private void CheckLevelCompletion()
    {
        //检查关卡是否完成：如果场上没有球，则关卡完成
        Ball[] balls = FindObjectsOfType<Ball>();
        
        if (balls.Length == 0)
        {
            LevelComplete();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(0, -4f, 0);
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }

    private void SpawnInitialBalls()
    {
        for (int i = 0; i < initialBallCount; i++)
        {
            SpawnBall(3, new Vector3(-3f + i * 6f, 2f, 0));
        }
    }

    #region Gestion des balles

    /// <summary>
    /// Crée une nouvelle balle du type spécifié à la position donnée
    /// </summary>
    /// <param name="size">Taille de la balle (3 = grande, 1 = petite)</param>
    /// <param name="position">Position où créer la balle</param>
    /// <returns>La référence au GameObject de la balle créée</returns>
    public GameObject SpawnBall(int size, Vector3 position)
    {
        GameObject ballObject = Instantiate(ballPrefab, position, Quaternion.identity);
        Ball ball = ballObject.GetComponent<Ball>();
        
        if (ball != null)
        {
            ball.Initialize(size, GetRandomBallColor());
        }
        
        return ballObject;
    }

    private Color GetRandomBallColor()
    {
        if (ballColors.Count > 0)
        {
            int randomIndex = Random.Range(0, ballColors.Count);
            return ballColors[randomIndex];
        }
        
        return Color.white;
    }

    #endregion

    #region Gestion du score et du joueur

    public void AddScore(int points)
    {
        score += points;
        OnScoreChanged?.Invoke();
    }

    public void PlayerHit()
    {
        remainingLives--;
        OnLifeLost?.Invoke();
        
        if (remainingLives <= 0)
        {
            GameOver();
        }
        else
        {
            // 重置玩家位置
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.Reset();
            }
        }
    }

    #endregion

    #region Gestion du niveau

    private void LevelComplete()
    {
        isGameRunning = false;
        OnLevelComplete?.Invoke();
        
        // 进入下一关或者重置游戏
        currentLevel++;
        
        // 完成最高级别后进入下一关
        if (currentLevel > MAX_LEVEL)
        {
            currentLevel = 1;
        }
        
        Invoke(nameof(LoadNextLevel), 2f);
    }

    private void LoadNextLevel()
    {
        // 清理当前层级的物品
        CleanupCurrentLevel();
        
        // 隐藏关卡完成面板
        /*UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.HideLevelCompletePanel();
        }*/
        
        // 将玩家居中
        CenterPlayer();
        
        // 重置定时器
        remainingTime = levelTime;
        
        // 更改关卡背景
        RefreshBackground();
        
        //生成该关卡的球
        SpawnBallsForLevel();
        
        // 开始关卡
        isGameRunning = true;
        
        // 触发游戏开始事件
        OnGameStart?.Invoke();
    }
    
    private void CleanupCurrentLevel()
    {
        // 消灭所有剩余的子弹
        Ball[] balls = FindObjectsOfType<Ball>();
        foreach (Ball ball in balls)
        {
            Destroy(ball.gameObject);
        }
        
        // 消灭所有剩余的鱼叉
        Harpoon[] harpoons = FindObjectsOfType<Harpoon>();
        foreach (Harpoon harpoon in harpoons)
        {
            Destroy(harpoon.gameObject);
        }
    }
    
    private void CenterPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.Reset();
        }
    }
    
    private void RefreshBackground()
    {
        // 找到LevelGenerator并更新背景 
        /*
        LevelGenerator levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator != null)
        {
            levelGenerator.UpdateBackground(currentLevel);
        }
        */
    }
    
    private void SpawnBallsForLevel()
    {
        //球的数量与当前关卡数相同
        int ballsToSpawn = currentLevel;
        
        // 将最大子弹数量限制在合理的范围内
        if (ballsToSpawn < 1) ballsToSpawn = 1;
        
        //水平间距球体
        float spacing = 6f / ballsToSpawn;
        float startX = -3f + spacing/2;
        
        for (int i = 0; i < ballsToSpawn; i++)
        {
            Vector3 position = new Vector3(startX + i * spacing, 2f, 0);
            SpawnBall(3, position); // La taille 3 correspond aux grandes balles
        }
    }

    private void GameOver()
    {
        isGameRunning = false;
        OnGameOver?.Invoke();
        
        // 2s后重新加载场景
        Invoke(nameof(RestartGame), 2f);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion

    #region Accesseurs

    // ui及其他系统的访问器
    public int GetScore() => score;
    public int GetLives() => remainingLives;
    public float GetRemainingTime() => remainingTime;
    public bool IsGameRunning() => isGameRunning;
    public int GetLevel() => currentLevel;

    #endregion

    /// <summary>
    /// 配置游戏的碰撞层
    /// </summary>
    private void SetupCollisionLayers()
    {
        // 获取球层的索引
        int ballLayer = LayerMask.NameToLayer(BALL_LAYER_NAME);
        
        // 如果该图层不在项目中，则使用默认的BALL_LAYER索引，并发出警告
        if (ballLayer == -1)
        {
            ballLayer = BALL_LAYER;
            Debug.LogWarning($"Layer '{BALL_LAYER_NAME}' non trouvé. Vous devriez créer ce layer dans les paramètres du projet.");
        }
        
        // 设置球体的碰撞层，使其忽略与自身的碰撞
        // 这使得球体不会与其他球体发生碰撞，从而避免了不必要的物理交互
        Physics2D.IgnoreLayerCollision(ballLayer, ballLayer, true);
        
        Debug.Log($"Configuration des collisions: les balles (layer {ballLayer}) s'ignorent entre elles.");
    }
}
