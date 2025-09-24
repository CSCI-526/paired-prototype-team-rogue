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
    
    // Movement
    private Vector2 _moveInput;
    private Vector2 _mousePosition;
    private Rigidbody2D _rb;
    private Camera _mainCamera;
    
    // Dodge
    private bool _isDodging = false;
    private float _dodgeTimer = 0f;
    private float _lastDodgeTime = -999f;
    private Vector2 _dodgeDirection;
    
    // Combat
    private int _currentCombo = 0;
    private float _lastAttackTime = -999f;
    private float _comboResetTime = 1f; // 连招重置时间
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
        
        // initialize Health
        _currentHealth = maxHealth;
        
        // Initialize Weapon Location
        if (weapon != null)
        {
            weapon.localPosition = new Vector3(0.8f, 0, 0);
        }
    }
    
    void Update()
    {
        HandleInput();
        HandleMouseLook();
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
            PerformNormalAttack();
        }
        
        // Special Attack
        if (Input.GetMouseButtonDown(1) && !_isDodging && !_isAttacking && _currentEnergy >= maxEnergy)
        {
            PerformSpecialAttack();
        }
    }
    
    void HandleMouseLook()
    {
        // Rotate Weapon towards Mouse
        if (weaponPivot != null && !_isAttacking)
        {
            Vector2 lookDir = _mousePosition - (Vector2)weaponPivot.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
    
    void HandleMovement()
    {
        if (_isDodging)
        {
            // Dodge
            _rb.linearVelocity = _dodgeDirection * dodgeSpeed;
        }
        else if (!_isAttacking)
        {
            // normal movement
            _rb.linearVelocity = _moveInput * moveSpeed;
        }
        else
        {
            // slow down when attack
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
            // dodge towards mouse direction
            _dodgeDirection = ((Vector2)_mousePosition - (Vector2)transform.position).normalized;
        }
        
        #if UNITY_EDITOR
        Debug.Log("Doding！");
        #endif
    }
    
    void PerformNormalAttack()
    {
        // Check Combo CD
        if (Time.time > _lastAttackTime + _comboResetTime)
        {
            _currentCombo = 0;
        }
        
        _isAttacking = true;
        _attackAnimTimer = attackCooldown;
        _lastAttackTime = Time.time;
        
        // Attack Combos
        switch (_currentCombo)
        {
            case 0:
                Attack1(); // Combo1-1
                break;
            case 1:
                Attack2(); // Combo1-2
                break;
            case 2:
                Attack3(); // Combo1-3
                break;
        }
        
        _currentCombo = (_currentCombo + 1) % 3;
    }
    
    void Attack1()
    {
        #if UNITY_EDITOR
        Debug.Log("Combo1-1");
        #endif
        
        if (weaponPivot != null)
        {
            // Temp Attack
            StartCoroutine(SwingWeapon(-45f, 45f, 0.2f));
        }
    }
    
    void Attack2()
    {
        #if UNITY_EDITOR
        Debug.Log("Combo1-2");
        #endif
        
        if (weaponPivot != null)
        {
            // Temp Attack
            StartCoroutine(SwingWeapon(45f, -45f, 0.2f));
        }
    }
    
    void Attack3()
    {
        #if UNITY_EDITOR
        Debug.Log("Combo1-3");
        #endif
        
        if (weaponPivot != null)
        {
            // Temp Attack
            StartCoroutine(SwingWeapon(-60f, 60f, 0.3f));
        }
    }
    
    void PerformSpecialAttack()
    {
        #if UNITY_EDITOR
        Debug.Log("Special Attack");
        #endif
        
        _isAttacking = true;
        _attackAnimTimer = 0.5f;
        _currentEnergy = 0;
        
        if (weaponPivot != null)
        {
            StartCoroutine(SpinAttack(360f, 0.5f));
        }
    }
    
    System.Collections.IEnumerator SwingWeapon(float startAngle, float endAngle, float duration)
    {
        float elapsed = 0f;
        Vector2 lookDir = _mousePosition - (Vector2)weaponPivot.position;
        float baseAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            weaponPivot.rotation = Quaternion.AngleAxis(baseAngle + angle, Vector3.forward);
            yield return null;
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
    }
    
    void UpdateTimers()
    {
        // Update Dodge CD
        if (_isDodging)
        {
            _dodgeTimer -= Time.deltaTime;
            if (_dodgeTimer <= 0)
            {
                _isDodging = false;
            }
        }
        
        // Update Attack CD
        if (_isAttacking)
        {
            _attackAnimTimer -= Time.deltaTime;
            if (_attackAnimTimer <= 0)
            {
                _isAttacking = false;
            }
        }
    }
    
    // General Events
    public void AddEnergy(int amount)
    {
        _currentEnergy = Mathf.Min(_currentEnergy + amount, maxEnergy);
        
        #if UNITY_EDITOR
        Debug.Log($"Get Energy {amount}, Current Energy: {_currentEnergy}/{maxEnergy}");
        #endif
    }
    
    public void OnHitEnemy()
    {
        // Gain Energy when Hit an enemy
        AddEnergy(energyPerHit);
    }
    
    public void TakeDamage(int damage)
    {
        _currentHealth = Mathf.Max(_currentHealth - damage, 0);
        
        #if UNITY_EDITOR
        Debug.Log($"Taking Damage: {damage}, Current HP: {_currentHealth}/{maxHealth}");
        #endif
    }
    
    public int GetCurrentHealth() => _currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentEnergy() => _currentEnergy;
    public int GetMaxEnergy() => maxEnergy;
    public bool IsDodging() => _isDodging;
    public bool IsAttacking() => _isAttacking;
}