using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Dimensions du niveau")]
    [SerializeField] private float levelWidth = 16f;
    [SerializeField] private float levelHeight = 9f;
    
    [Header("Limites")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallThickness = 0.5f;
    
    [Header("Obstacles")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private Transform obstaclesParent;
    
    [Header("Plateformes")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private List<Vector2> platformPositions = new List<Vector2>();
    
    [Header("Arrière-plan")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Sprite[] backgroundSprites;
    
    private void Start()
    {
        // 设置摄像头
        SetupCamera();
        
        //生成边界
        CreateBoundaries();
        
      
        // 设置障碍物和平台
        CreateObstacles();
        CreatePlatforms();
        
        //配置背景
        SetBackground();
    }
    
    private void SetupCamera()
    {
        if (Camera.main != null)
        {
            // 设置摄像头以正确显示关卡
            Camera.main.orthographicSize = levelHeight / 2f;
            Camera.main.transform.position = new Vector3(0, 0, -10);
        }
    }
    
    private void CreateBoundaries()
    {
        if (wallPrefab == null) return;
        
        // 创建左右两侧的墙壁和顶部/底部的墙壁
        float wallHeight = levelHeight + wallThickness * 2;
        float sideX = levelWidth / 2f + wallThickness / 2f;
        
        // 左墙
        CreateWall(new Vector3(-sideX, 0, 0), new Vector3(wallThickness, wallHeight, 1));
        
        // 右墙
        CreateWall(new Vector3(sideX, 0, 0), new Vector3(wallThickness, wallHeight, 1));
        
        // 底部
        float bottomY = -levelHeight / 2f - wallThickness / 2f;
        CreateWall(new Vector3(0, bottomY, 0), new Vector3(levelWidth + wallThickness * 2, wallThickness, 1));
        
        // 天花板
        float topY = levelHeight / 2f + wallThickness / 2f;
        GameObject ceiling = CreateWall(new Vector3(0, topY, 0), new Vector3(levelWidth + wallThickness * 2, wallThickness, 1));
        
        // 给天花板设置标签为 "Ceiling"
        if (ceiling != null)
        {
            ceiling.tag = "Ceiling";
        }
    }
    
    private GameObject CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
        wall.transform.localScale = scale;
        wall.transform.SetParent(transform);
        
        // 默认情况下，墙壁的标签为 "Wall"，以便在游戏中识别
        wall.tag = "Wall";
        
        return wall;
    }
    
    private void CreateObstacles()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0 || obstaclesParent == null) return;
        
        // 在这里根据当前关卡数生成障碍物
        // 根据当前关卡数生成障碍物的数量和位置
        // 你可以根据需要调整生成逻辑
        
        // 简单示例：根据当前关卡数生成障碍物
        int currentLevel = GameManager.Instance ? GameManager.Instance.GetLevel() : 1;
        int obstacleCount = Mathf.Min(currentLevel, 5);
        
        for (int i = 0; i < obstacleCount; i++)
        {
            float x = Random.Range(-levelWidth / 2f + 1f, levelWidth / 2f - 1f);
            float y = Random.Range(-levelHeight / 2f + 2f, levelHeight / 2f - 1f);
            
            int prefabIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obstacle = Instantiate(obstaclePrefabs[prefabIndex], new Vector3(x, y, 0), Quaternion.identity);
            obstacle.transform.SetParent(obstaclesParent);
        }
    }
    
    private void CreatePlatforms()
    {
        if (platformPrefab == null || platformPositions.Count == 0) return;
        
        foreach (Vector2 position in platformPositions)
        {
            GameObject platform = Instantiate(platformPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
            platform.transform.SetParent(transform);
        }
    }
    
    private void SetBackground()
    {
        if (backgroundRenderer == null || backgroundSprites == null || backgroundSprites.Length == 0) return;
        
        // 选择背景图像的索引基于当前关卡数
        int currentLevel = GameManager.Instance ? GameManager.Instance.GetLevel() : 1;
        int backgroundIndex = (currentLevel - 1) % backgroundSprites.Length;
        
        backgroundRenderer.sprite = backgroundSprites[backgroundIndex];
        
        //调整背景大小以覆盖整个屏幕
        float width = levelWidth + 2;
        float height = levelHeight + 2;
        backgroundRenderer.size = new Vector2(width, height);
    }
    
    /// <summary>
    /// 根据当前关卡数更新背景图像。此方法在关卡切换时由 GameManager 调用。
    /// 
    /// </summary>
    /// <param name="level">当前水平</param>
    public void UpdateBackground(int level)
    {
        if (backgroundRenderer == null || backgroundSprites == null || backgroundSprites.Length == 0) return;
        
        // 计算背景图像的索引基于当前关卡数
        int backgroundIndex = (level - 1) % backgroundSprites.Length;
        
        // 更新背景渲染器的精灵
        backgroundRenderer.sprite = backgroundSprites[backgroundIndex];
        
        // 调整背景大小以覆盖整个屏幕
        float width = levelWidth + 2;
        float height = levelHeight + 2;
        backgroundRenderer.size = new Vector2(width, height);
        
        // 记录日志以调试背景更新
        Debug.Log($"Arrière-plan mis à jour pour le niveau {level} (index sprite: {backgroundIndex})");
    }
}