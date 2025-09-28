using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float specialEnergyCost = 100f;
    [SerializeField] private float AttackSlowMultiplier = 0.5f;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Transform weapon;

    [Header("Resource Settings")]
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float maxEnergy = 100f;
    [SerializeField] private float healthRegenPerSecond = 0f;
    [SerializeField] private float energyRegenPerSecond = 0f;
    
    [Header("Auto Combat")]
    [SerializeField] private float targetSearchRadius = 30f;   // how far to look for enemies
    [SerializeField] private float retargetInterval = 0.15f;   // how often to refresh target
    private Transform _currentTarget;
    private float _nextRetargetTime = 0f;
    public float MaxHealth => maxHealth; 
    public float MaxEnergy => maxEnergy; 
    private Rigidbody2D _rb;
    private PlayerAnimationController _animController;
    private Vector2 _moveInput;
    private Vector2 _dodgeDirection;
    private bool _isDodging = false;
    private float _dodgeTimer = 0f;
    private float _lastDodgeTime = -999f;
    private bool _isAttacking = false;
    private float _lastAttackTime = -999f;
    private int _comboIndex = 0;
    private bool _isSpecial = false;
    private float _currentHealth;
    private float _currentEnergy;
    private Direction _currentFacing = Direction.Down;

    private enum Direction { Up, Down, Left, Right }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            Debug.LogWarning("Added Rigidbody2D to Player - Check Inspector settings!");
        }
        else
        {
            Debug.Log("Player Rigidbody2D found - BodyType: " + _rb.bodyType + ", Constraints: " + _rb.constraints);
        }

        _animController = GetComponent<PlayerAnimationController>();
        if (_animController == null)
        {
            Debug.LogError("PlayerAnimationController not found on Player!");
        }

        _currentHealth = maxHealth;
        _currentEnergy = 0f;

        if (weaponPivot == null)
        {
            Debug.LogError("WeaponPivot not assigned in PlayerController!");
        }
        if (weapon == null)
        {
            Debug.LogError("Weapon not assigned in PlayerController!");
        }

        Debug.Log("PlayerController Start - Rigidbody: " + (_rb != null) + ", Anim: " + (_animController != null) + ", Position: " + transform.position);
    }

    void Update()
    {
        HandleInput();
        UpdateAimAndFacing(); 
        if (_animController != null) _animController.UpdateMovement(_moveInput);
        
        // regen
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + healthRegenPerSecond * Time.deltaTime);
        _currentEnergy = Mathf.Min(maxEnergy, _currentEnergy + energyRegenPerSecond * Time.deltaTime);
        AutoCombatUpdate(); 
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleInput()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        _moveInput = _moveInput.normalized;

        // dodge (保留)
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _lastDodgeTime + dodgeCooldown && !_isAttacking)
        {
            StartDodge();
        }
    }
    

    void HandleMovement()
    {
        if (_isDodging)
        {
            _dodgeTimer -= Time.deltaTime;
            _rb.linearVelocity = _dodgeDirection * dodgeSpeed;
            if (weapon != null) weapon.position = transform.position + weapon.localPosition;
            if (_dodgeTimer <= 0) EndDodge();
        }
        else
        {
            _rb.linearVelocity = _moveInput * moveSpeed;
        }
    }

    void StartDodge()
    {
        _isDodging = true;
        _dodgeTimer = dodgeDuration;
        _lastDodgeTime = Time.time;

        if (_moveInput.magnitude > 0.1f)
        {
            _dodgeDirection = _moveInput.normalized;
        }
        else
        {
            _dodgeDirection = GetVectorFromDirection(_currentFacing).normalized;
        }

        if (_animController != null) _animController.TriggerDodge();
    }

    void EndDodge()
    {
        _isDodging = false;
        _rb.linearVelocity = Vector2.zero;
        if (_animController != null) _animController.EndDodge();

    }
    void AutoCombatUpdate()
    {
        if (_isDodging) return;

        // 自动必杀：能量满优先
        if (!_isAttacking && !_isSpecial && _currentEnergy >= specialEnergyCost)
        {
            var t = GetOrFindTarget();
            if (t != null)
            {
                AimAt(t.position);
                StartSpecial();     // 会清空能量
                return;
            }
        }

        // 自动普攻：按冷却循环
        if (!_isAttacking && !_isSpecial && Time.time > _lastAttackTime + attackCooldown)
        {
            var t = GetOrFindTarget();
            if (t != null)
            {
                AimAt(t.position);
                StartAttack();
            }
        }
    }
    
    void UpdateAimAndFacing()
    {
        // periodic retarget
        if (Time.time >= _nextRetargetTime)
        {
            _nextRetargetTime = Time.time + retargetInterval;
            _currentTarget = FindNearestEnemy(targetSearchRadius);
        }

        // aim only when we have a target; otherwise keep current rotation
        if (_currentTarget != null)
            AimAt(_currentTarget.position);
    }

    void AimAt(Vector3 worldPos)
    {
        if (!weaponPivot) return;
        Vector2 dir = ((Vector2)worldPos - (Vector2)weaponPivot.position).normalized;
        if (dir.sqrMagnitude < 1e-4f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        weaponPivot.rotation = Quaternion.Euler(0, 0, angle);

        // update facing for animations
        _currentFacing = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
            ? (dir.x > 0 ? Direction.Right : Direction.Left)
            : (dir.y > 0 ? Direction.Up    : Direction.Down);
    }
    
    void StartAttack()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _comboIndex = (_comboIndex + 1) % 3;
        if (_animController != null) _animController.TriggerAttack(_comboIndex);

        if (weaponPivot != null)
        {
            StartCoroutine(SwingWeapon());
        }
    }

    System.Collections.IEnumerator SwingWeapon()
    {
        float elapsed = 0f;
        Quaternion startRotation = weaponPivot.rotation;
        float swingAngle = _comboIndex == 0 ? 90f : _comboIndex == 1 ? -90f : 0f; // Larger swing for melee effect
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, swingAngle);

        while (elapsed < attackCooldown)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attackCooldown;
            if (t < 0.5f) 
                weaponPivot.rotation = Quaternion.Lerp(startRotation, targetRotation, t * 2f);
            else 
                weaponPivot.rotation = Quaternion.Lerp(targetRotation, startRotation, (t - 0.5f) * 2f);
            yield return null;
        }

        weaponPivot.rotation = startRotation; 
        _isAttacking = false;
        if (_animController != null) _animController.EndAttack();
    }

    void StartSpecial()
    {
        _isSpecial = true;
        _lastAttackTime = Time.time;
        _currentEnergy = 0f;
        if (_animController != null) _animController.TriggerSpecial();
        if (weaponPivot != null) StartCoroutine(SpinWeapon());
    }

    System.Collections.IEnumerator SpinWeapon()
    {
        float elapsed = 0f;
        Quaternion startRotation = weaponPivot.rotation;

        while (elapsed < attackCooldown)
        {
            elapsed += Time.deltaTime;
            weaponPivot.rotation = startRotation * Quaternion.Euler(0, 0, 360f * (elapsed / attackCooldown));
            yield return null;
        }

        weaponPivot.rotation = startRotation;
        _isSpecial = false;
        if (_animController != null) _animController.EndSpecial();
    }

    Transform FindNearestEnemy(float radius)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform best = null;
        float bestDist = Mathf.Infinity;
        Vector2 from = weaponPivot ? (Vector2)weaponPivot.position : (Vector2)transform.position;

        foreach (var e in enemies)
        {
            if (!e.activeInHierarchy) continue;
            float d = Vector2.Distance(from, e.transform.position);
            if (d < bestDist && d <= radius)
            {
                bestDist = d;
                best = e.transform;
            }
        }
        return best;
    }

    Transform GetOrFindTarget()
    {
        if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy ||
            Vector2.Distance(weaponPivot.position, _currentTarget.position) > targetSearchRadius)
        {
            _currentTarget = FindNearestEnemy(targetSearchRadius);
        }
        return _currentTarget;
    }
    
    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage called - Damage: " + damage + ", Current Health: " + _currentHealth);
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
        else if (_animController != null)
        {
            _animController.TriggerHit();
            Invoke("EndHit", 0.1f);
        }
    }

    void EndHit()
    {
        if (_animController != null) _animController.EndHit();
    }

    void Die()
    {
        _currentHealth = 0;
        if (_animController != null) _animController.TriggerHit();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
        else
        {
            Debug.LogWarning("Die - GameManager.Instance is null!");
        }
    }

    public void OnHitEnemy()
    {
        _currentEnergy = Mathf.Min(_currentEnergy + 10f, maxEnergy);
    }

    Vector2 GetVectorFromDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2.up;
            case Direction.Down: return Vector2.down;
            case Direction.Left: return Vector2.left;
            case Direction.Right: return Vector2.right;
            default: return Vector2.down;
        }
    }

    public bool IsAttacking()
    {
        return _isAttacking || _isSpecial;
    }

    public float GetHealth() { return _currentHealth; }
    public void SetHealth(float health) { _currentHealth = Mathf.Clamp(health, 0, maxHealth); }
    public float GetEnergy() { return _currentEnergy; }
    public void SetEnergy(float energy) { _currentEnergy = Mathf.Clamp(energy, 0, maxEnergy); }
    
    public void AddMoveSpeedPercent(float pct)          { moveSpeed *= (1f + pct); }
    public void AddAttackSpeedPercent(float pct)        { attackCooldown = Mathf.Max(0.05f, attackCooldown * (1f - pct)); }
    public void AddMaxHealth(float delta)               { maxHealth += delta; _currentHealth = Mathf.Min(maxHealth, _currentHealth + delta); }
    public void AddHealthRegen(float deltaPerSecond)    { healthRegenPerSecond += deltaPerSecond; }
    public void AddEnergyRegen(float deltaPerSecond)    { energyRegenPerSecond += deltaPerSecond; }
}