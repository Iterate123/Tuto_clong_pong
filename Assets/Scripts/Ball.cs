using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Ball : MonoBehaviour
{
    [Header("Paramètres de la balle")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float baseSize = 1f;
    [SerializeField] private int basePoints = 100;
    
    [Header("Paramètres de rebond")]
    [SerializeField] private float baseJumpHeight = 3f;
    [SerializeField] private float sizeJumpMultiplier = 0.5f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float minHorizontalSpeed = 1f; // 最小水平速度
    [SerializeField] private float wallCheckDistance = 0.1f; // 距离墙壁的检查距离
    
    // Composants
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    // État de la balle
    private int size;
    private float currentSpeed;
    private float jumpForce;
    private float stuckCheckTimer = 0f;
    private const float STUCK_CHECK_INTERVAL = 0.2f; // 检查球是否卡住的时间间隔
    
    private const string BALL_LAYER_NAME = "Balls";
    private const int BALL_LAYER = 8; // 默认的球层索引，如果未找到指定的层

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configurer cette balle pour qu'elle soit sur le layer "Balls"
        SetupCollisionLayer();
    }

    /// <summary>
    /// Configure le layer de la balle et s'assure que les balles s'ignorent entre elles
    /// </summary>
    private void SetupCollisionLayer()
    {
        // Obtenir l'index du layer "Balls"
        int ballLayer = LayerMask.NameToLayer(BALL_LAYER_NAME);
        if (ballLayer == -1)
        {
            // Si le layer n'existe pas, utiliser le layer par défaut prédéfini
            ballLayer = BALL_LAYER;
            Debug.LogWarning($"Layer '{BALL_LAYER_NAME}' non trouvé. Utilisation du layer {ballLayer} à la place. Créez ce layer dans les paramètres du projet.");
        }
        
        // Appliquer le layer à la balle
        gameObject.layer = ballLayer;
    }

    public void Initialize(int ballSize, Color ballColor)
    {
        size = ballSize;
        
        // 定义球的缩放比例
        float scale = baseSize * (size * 0.5f);
        transform.localScale = new Vector3(scale, scale, 1f);
        
        // 设置碰撞器的半径
        GetComponent<CircleCollider2D>().radius = 0.5f;
        
        // 设置球的颜色
        spriteRenderer.color = ballColor;
        
        // 根据大小调整速度（较小的球 = 更快的速度）
        currentSpeed = baseSpeed * (1f + (4 - size) * 0.25f);
        
        // 根据大小调整跳跃力（较小的球 = 更高的跳跃力）
        jumpForce = baseJumpHeight * (1f + size * sizeJumpMultiplier);
        
        // 设置重力缩放
        rb.gravityScale = gravityScale;
        
        // 设置初始速度
        SetInitialVelocity();
    }

    private void SetInitialVelocity()
    {
        // 随机选择水平方向：-1（向左）或 1（向右）
        float directionX = Random.Range(0, 2) * 2 - 1; // -1 ou 1
        
        // 应用初始速度
        rb.linearVelocity = new Vector2(directionX * currentSpeed, jumpForce);
    }

    /// <summary>
    /// Initialise la balle avec une vitesse et une direction spécifiques
    /// </summary>
    /// <param name="directionX">Direction horizontale (-1 pour gauche, 1 pour droite)</param>
    /// <param name="initialVerticalVelocity">Vitesse verticale initiale</param>
    public void SetCustomVelocity(float directionX, float initialVerticalVelocity)
    {
        // 给出方向
        directionX = Mathf.Sign(directionX);
        if (directionX == 0) directionX = 1; // Par défaut, aller à droite si 0
        
        // 指定速度
        rb.linearVelocity = new Vector2(directionX * currentSpeed, initialVerticalVelocity);
    }

    public void Split()
    {
        // 当球被击中时，如果它不是最小的球，则分裂成两个更小的球
        if (size > 1)
        {
            // 生成两个更小的球
            SpawnSplitBalls();
            
            // 计算得分：较大的球得分较少，较小的球得分更多
            int points = basePoints * (5 - size);
            //GameManager.Instance.AddScore(points);
        }
        else
        {
            // 如果是最小的球，给予更高的分数奖励
            //GameManager.Instance.AddScore(basePoints * 4);
        }
        
        // 销毁当前球体
        Destroy(gameObject);
    }

    private void SpawnSplitBalls()
    {
        // 获取当前球的垂直速度，以便在分裂时传递给新球
        float currentVerticalVelocity = rb.linearVelocity.y;
        
        // 生成两个更小的球
        for (int i = 0; i < 2; i++)
        {
            //创建新球的位置稍微偏离当前球的位置，以避免重叠
            Vector3 spawnPosition = transform.position;
            spawnPosition.x += (i == 0) ? -0.2f : 0.2f;
            
            // 生成新球
            //GameObject newBall = GameManager.Instance.SpawnBall(size - 1, spawnPosition);
            
            // 设置新球的速度和方向
            // Ball ballComponent = newBall.GetComponent<Ball>();
            // if (ballComponent != null)
            // {
            //     // 生成一个随机的水平方向：-1（向左）或 1（向右）
            //     float directionX = (i == 0) ? -1f : 1f;
                
            //     // 生成一个随机的垂直速度，确保它不会太低
            //     float verticalVelocity = currentVerticalVelocity + Random.Range(-1f, 1f);
            //     if (verticalVelocity < 0) verticalVelocity = 1f; // Assurer un minimum de vitesse verticale positive
                
            //     // 设置新球的速度
            //     ballComponent.SetCustomVelocity(directionX, verticalVelocity);
            // }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检查碰撞对象是否是墙壁或天花板
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ceiling"))
        {
            HandleBounce(collision);
        }
        
        // Effets sonores de rebond
        // AudioManager.Instance.PlayBounceSound();
    }

    private void HandleBounce(Collision2D collision)
    {
        // 判断是否是与地面的接触（即碰撞法线指向上方）
        bool isGroundContact = false;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 检查碰撞法线的y分量是否大于0.5，表示是地面接触
            if (contact.normal.y > 0.5f)
            {
                isGroundContact = true;
                break;
            }
        }
        
        // 如果是地面接触，应用跳跃力，否则应用水平反弹
        if (isGroundContact)
        {
            // 应用跳跃力
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        // 如果没有地面接触，检查是否是墙壁或天花板的碰撞
        else
        {
            // 反转水平速度以模拟墙壁碰撞
            float dirX = rb.linearVelocity.x > 0 ? -1 : 1;
            
            // 确保水平速度不低于最小水平速度
            rb.linearVelocity = new Vector2(dirX * Mathf.Max(currentSpeed, minHorizontalSpeed), rb.linearVelocity.y);
        }
    }

    private void FixedUpdate()
    {
        //不断检查球是否卡住在墙壁附近
        CheckIfStuck();
        
        // 确保水平速度不低于最小水平速度
        EnsureMinimumHorizontalSpeed();
        
        // 限制球的位置在屏幕范围内
        LimitBallPosition();
    }

    private void CheckIfStuck()
    {
        // 累积时间以检查球是否卡住
        stuckCheckTimer += Time.fixedDeltaTime;
        
        // 每隔一定时间检查一次 
        if (stuckCheckTimer >= STUCK_CHECK_INTERVAL)
        {
            stuckCheckTimer = 0f;
            
            // 检查水平速度是否低于最小水平速度
            if (Mathf.Abs(rb.linearVelocity.x) < minHorizontalSpeed)
            {
                //  获取屏幕宽度和球的半径
                float screenWidth = Camera.main.orthographicSize * Camera.main.aspect;
                float ballRadius = transform.localScale.x * 0.5f;
                
                // 检查球是否靠近左墙或右墙
                bool nearLeftWall = transform.position.x < -screenWidth + ballRadius + wallCheckDistance;
                bool nearRightWall = transform.position.x > screenWidth - ballRadius - wallCheckDistance;
                
                if (nearLeftWall || nearRightWall)
                {
                    // 如果球靠近墙壁且水平速度过低，强制设置一个最小的水平速度以防止卡住
                    float dirX = nearLeftWall ? 1f : -1f;
                    rb.linearVelocity = new Vector2(dirX * currentSpeed, rb.linearVelocity.y);
                }
            }
        }
    }

    private void EnsureMinimumHorizontalSpeed()
    {
        // S'assurer que la vitesse horizontale est toujours au moins égale à minHorizontalSpeed
        float currentXVelocity = rb.linearVelocity.x;
        
        if (Mathf.Abs(currentXVelocity) < minHorizontalSpeed)
        {
            // Garder le signe (direction) mais augmenter la magnitude
            float dirX = Mathf.Sign(currentXVelocity);
            // Si la vitesse est nulle, choisir une direction aléatoire
            if (dirX == 0f) dirX = Random.Range(0, 2) * 2 - 1;
            
            rb.linearVelocity = new Vector2(dirX * currentSpeed, rb.linearVelocity.y);
        }
    }

    private void LimitBallPosition()
    {
        // 获取屏幕的宽度和高度
        float screenHeight = Camera.main.orthographicSize;
        float screenWidth = screenHeight * Camera.main.aspect;
        
        // 获取球的半径
        float ballRadius = transform.localScale.x * 0.5f;
        
        Vector2 position = transform.position;
        Vector2 velocity = rb.linearVelocity;
        
        // 处理水平和垂直边界
        HandleHorizontalBoundaries(ref position, ref velocity, screenWidth, ballRadius);
        HandleVerticalBoundaries(ref position, ref velocity, screenHeight, ballRadius);
        
        // 应用修改后的位置和速度
        transform.position = position;
        rb.linearVelocity = velocity;
    }

    private void HandleHorizontalBoundaries(ref Vector2 position, ref Vector2 velocity, float screenWidth, float ballRadius)
    {
        const float OFFSET = 0.01f; // 定义一个小的偏移量，以防止球卡在边界上
        
        // 限制水平边界（确保球不会离开屏幕）
        if (position.x < -screenWidth + ballRadius)
        {
            position.x = -screenWidth + ballRadius + OFFSET;
            velocity.x = Mathf.Abs(velocity.x);
            
            // 触发反弹
            if (velocity.x < minHorizontalSpeed) velocity.x = currentSpeed;
        }
        else if (position.x > screenWidth - ballRadius)
        {
            position.x = screenWidth - ballRadius - OFFSET;
            velocity.x = -Mathf.Abs(velocity.x);
            
            //
            if (Mathf.Abs(velocity.x) < minHorizontalSpeed) velocity.x = -currentSpeed;
        }
    }
    
    private void HandleVerticalBoundaries(ref Vector2 position, ref Vector2 velocity, float screenHeight, float ballRadius)
    {
        const float OFFSET = 0.01f; //
        
        // 检查球不要飞出边界
        if (position.y < -screenHeight + ballRadius)
        {
            position.y = -screenHeight + ballRadius + OFFSET;
            velocity.y = jumpForce; // 
        }
        else if (position.y > screenHeight - ballRadius)
        {
            position.y = screenHeight - ballRadius - OFFSET;
            velocity.y = -Mathf.Abs(velocity.y);
        }
    }

}
