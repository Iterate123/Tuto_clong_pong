using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject harpoonPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private float cooldownTime = 0.5f;

    
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Player state
    private Vector2 moveDirection;
    private Vector2 currentVelocity;
    private bool canShoot = true;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private float invulnerabilityDuration = 2f;

    // Trigger shot
    private bool shoot;

    // Movement limit
    private float screenBoundaryX = 8f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (firingPoint == null)
            firingPoint = transform;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameRunning()) return;

        HandleInput();

        if (isInvulnerable)
            HandleInvulnerability();
    }

    private void HandleInput()
    {
        // Get horizontal input
        moveDirection.x = Input.GetAxisRaw("Horizontal");
        
        // Shot
        if ((Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space)) && canShoot)
            shoot = true;
    }




}
