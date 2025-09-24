using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dodgeSpeed = 15f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int energyPerHit = 20;
    [SerializeField] private int maxHealth = 100;
    
    [Header("Component References")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Transform weapon;
    
    // Direction enum for cleaner code
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    // Movement
    private Vector2 _moveInput;
    private Vector2 _mousePosition;
    private Rigidbody2D _rb;
    private Camera _mainCamera;
    
    // Animation controller
    private PlayerAnimationController _animController;
    
    // Direction
    private Direction _currentFacing = Direction.Right;
    private Direction _attackDirection = Direction.Right;
    
    // Dodge
    private bool _isDodging = false;
    private float _dodgeTimer = 0f;
    private float _lastDodgeTime = -999f;
    private Vector2 _dodgeDirection;
    
    // Combat
    private int _currentCombo = 0;
    private float _lastAttackTime = -999f;
    private float _comboResetTime = 1f;
    private bool _isAttacking = false;
    private float _attackAnimTimer = 0f;
    
    // Resource
    private int _currentHealth;
    private int _currentEnergy = 0;
    
    private const float AttackSlowMultiplier = 0.3f;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }
        
        _mainCamera = Camera.main;
        
        // Get animation controller if exists
        _animController = GetComponent<PlayerAnimationController>();
        
        // Initialize Health
        _currentHealth = maxHealth;
        
        // Initialize Weapon Location
        if (weapon != null)
        {
            weapon.localPosition = new Vector3(0.8f, 0, 0);
        }
        
        // Set initial facing direction
        UpdateFacingDirection(_currentFacing);
    }
    
    void Update()
    {
        HandleInput();
        UpdateTimers();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
    }
    
    void HandleInput()
    {
        // Movement Input
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        _moveInput = _moveInput.normalized;
        
        // Update facing direction based on movement (only when not attacking)
        if (!_isAttacking && _moveInput.magnitude > 0.1f)
        {
            Direction newDirection = GetDirectionFromVector(_moveInput);
            if (newDirection != _currentFacing)
            {
                UpdateFacingDirection(newDirection);
            }
        }
        
        // Mouse Position
        _mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // Dodge Input
        if (Input.GetKeyDown(KeyCode.Space) && !_isDodging && Time.time > _lastDodgeTime + dodgeCooldown)
        {
            StartDodge();
        }
        
        // Normal Attack
        if (Input.GetMouseButtonDown(0) && !_isDodging && !_isAttacking)
        {
            // Determine attack direction based on mouse position
            _attackDirection = GetAttackDirection();
            UpdateFacingDirection(_attackDirection);
            PerformNormalAttack();
        }
        
        // Special Attack
        if (Input.GetMouseButtonDown(1) && !_isDodging && !_isAttacking && _currentEnergy >= maxEnergy)
        {
            _attackDirection = GetAttackDirection();
            UpdateFacingDirection(_attackDirection);
            PerformSpecialAttack();
        }
    }
    
    Direction GetDirectionFromVector(Vector2 vector)
    {
        // Convert vector to 4-directional
        float absX = Mathf.Abs(vector.x);
        float absY = Mathf.Abs(vector.y);
        
        if (absX > absY)
        {
            return vector.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return vector.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    
    Direction GetAttackDirection()
    {
        // Get mouse position relative to player
        Vector2 mouseRelative = _mousePosition - (Vector2)transform.position;
        return GetDirectionFromVector(mouseRelative);
    }
    
    void UpdateFacingDirection(Direction direction)
    {
        _currentFacing = direction;
        
        // Update weapon pivot rotation based on direction
        if (weaponPivot != null)
        {
            float angle = 0f;
            switch (direction)
            {
                case Direction.Right:
                    angle = 0f;
                    break;
                case Direction.Up:
                    angle = 90f;
                    break;
                case Direction.Left:
                    angle = 180f;
                    break;
                case Direction.Down:
                    angle = -90f;
                    break;
            }
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    void HandleMovement()
    {
        if (_isDodging)
        {
            // Dodge movement
            _rb.linearVelocity = _dodgeDirection * dodgeSpeed;
        }
        else if (!_isAttacking)
        {
            // Normal movement
            _rb.linearVelocity = _moveInput * moveSpeed;
        }
        else
        {
            // Slow down when attacking
            _rb.linearVelocity = _moveInput * (moveSpeed * AttackSlowMultiplier);
        }
    }
    
    void StartDodge()
    {
        _isDodging = true;
        _dodgeTimer = dodgeDuration;
        _lastDodgeTime = Time.time;
        
        // Dodge Direction
        if (_moveInput.magnitude > 0.1f)
        {
            _dodgeDirection = _moveInput;
        }
        else
        {
            // Dodge in facing direction if no movement input
            _dodgeDirection = GetVectorFromDirection(_currentFacing);
        }
        
        // Trigger dodge animation if animation controller exists
        if (_animController != null)
        {
            _animController.TriggerDodge();
        }
        
        #if UNITY_EDITOR
        Debug.Log("Dodging!");
        #endif
    }
    
    Vector2 GetVectorFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector2.up;
            case Direction.Down:
                return Vector2.down;
            case Direction.Left:
                return Vector2.left;
            case Direction.Right:
                return Vector2.right;
            default:
                return Vector2.right;
        }
    }
    
    void PerformNormalAttack()
    {
        // Check Combo timing
        if (Time.time > _lastAttackTime + _comboResetTime)
        {
            _currentCombo = 0;
        }
        
        _isAttacking = true;
        _attackAnimTimer = attackCooldown;
        _lastAttackTime = Time.time;
        
        // Execute different attack animations based on direction and combo
        switch (_currentCombo)
        {
            case 0:
                Attack1();
                break;
            case 1:
                Attack2();
                break;
            case 2:
                Attack3();
                break;
        }
        
        _currentCombo = (_currentCombo + 1) % 3;
    }
    
    void Attack1()
    {
        #if UNITY_EDITOR
        Debug.Log($"Attack 1 - Direction: {_attackDirection}");
        #endif
        
        // Trigger animation if animation controller exists
        if (_animController != null)
        {
            _animController.TriggerAttack(0);
        }
        
        if (weaponPivot != null)
        {
            // Swing based on current attack direction
            StartCoroutine(DirectionalSwing(-30f, 30f, 0.2f));
        }
    }
    
    void Attack2()
    {
        #if UNITY_EDITOR
        Debug.Log($"Attack 2 - Direction: {_attackDirection}");
        #endif
        
        // Trigger animation if animation controller exists
        if (_animController != null)
        {
            _animController.TriggerAttack(1);
        }
        
        if (weaponPivot != null)
        {
            // Opposite swing
            StartCoroutine(DirectionalSwing(30f, -30f, 0.2f));
        }
    }
    
    void Attack3()
    {
        #if UNITY_EDITOR
        Debug.Log($"Attack 3 - Direction: {_attackDirection}");
        #endif
        
        // Trigger animation if animation controller exists
        if (_animController != null)
        {
            _animController.TriggerAttack(2);
        }
        
        if (weaponPivot != null)
        {
            // Wide swing for combo finisher
            StartCoroutine(DirectionalSwing(-45f, 45f, 0.3f));
        }
    }
    
    void PerformSpecialAttack()
    {
        #if UNITY_EDITOR
        Debug.Log($"Special Attack - Direction: {_attackDirection}");
        #endif
        
        _isAttacking = true;
        _attackAnimTimer = 0.5f;
        _currentEnergy = 0;
        
        // Trigger special attack animation if animation controller exists
        if (_animController != null)
        {
            _animController.TriggerSpecialAttack();
        }
        
        if (weaponPivot != null)
        {
            StartCoroutine(SpinAttack(360f, 0.5f));
        }
    }
    
    System.Collections.IEnumerator DirectionalSwing(float startAngle, float endAngle, float duration)
    {
        float elapsed = 0f;
        float baseAngle = GetAngleFromDirection(_attackDirection);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            weaponPivot.rotation = Quaternion.Euler(0, 0, baseAngle + angle);
            yield return null;
        }
        
        // Return to base direction
        weaponPivot.rotation = Quaternion.Euler(0, 0, baseAngle);
    }
    
    float GetAngleFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                return 0f;
            case Direction.Up:
                return 90f;
            case Direction.Left:
                return 180f;
            case Direction.Down:
                return -90f;
            default:
                return 0f;
        }
    }
    
    System.Collections.IEnumerator SpinAttack(float totalRotation, float duration)
    {
        float elapsed = 0f;
        float startRotation = weaponPivot.eulerAngles.z;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float angle = Mathf.Lerp(0, totalRotation, t);
            weaponPivot.rotation = Quaternion.Euler(0, 0, startRotation + angle);
            yield return null;
        }
        
        // Return to current facing direction
        UpdateFacingDirection(_currentFacing);
    }
    
    void UpdateTimers()
    {
        // Update Dodge timer
        if (_isDodging)
        {
            _dodgeTimer -= Time.deltaTime;
            if (_dodgeTimer <= 0)
            {
                _isDodging = false;
            }
        }
        
        // Update Attack timer
        if (_isAttacking)
        {
            _attackAnimTimer -= Time.deltaTime;
            if (_attackAnimTimer <= 0)
            {
                _isAttacking = false;
            }
        }
    }
    
    // Public methods for other systems
    public void AddEnergy(int amount)
    {
        _currentEnergy = Mathf.Min(_currentEnergy + amount, maxEnergy);
        
        #if UNITY_EDITOR
        Debug.Log($"Gained Energy: {amount}, Current Energy: {_currentEnergy}/{maxEnergy}");
        #endif
    }
    
    public void OnHitEnemy()
    {
        // Gain energy when hitting an enemy
        AddEnergy(energyPerHit);
    }
    
    public void TakeDamage(int damage)
    {
        _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        
        // Trigger hit animation if animation controller exists
        if (_animController != null)
        {
            _animController.TriggerHit();
        }
        
        #if UNITY_EDITOR
        Debug.Log($"Took Damage: {damage}, Current HP: {_currentHealth}/{maxHealth}");
        #endif
        
        // Check if dead
        if (_currentHealth <= 0)
        {
            OnDeath();
        }
    }
    
    void OnDeath()
    {
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
        
        // Disable controls
        enabled = false;
    }
    
    // Save/Load methods for level transitions
    public void SetHealth(int current, int max)
    {
        _currentHealth = current;
        maxHealth = max;
    }
    
    public void SetEnergy(int current, int max)
    {
        _currentEnergy = current;
        maxEnergy = max;
    }
    
    // Getters
    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentEnergy() => _currentEnergy;
    public int GetMaxEnergy() => maxEnergy;
    public bool IsDodging() => _isDodging;
    public bool IsAttacking() => _isAttacking;
    public Direction GetCurrentFacing() => _currentFacing;
}