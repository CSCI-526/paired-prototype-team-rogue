using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // Animation parameter names
    private const string PARAM_MOVE_X = "MoveX";
    private const string PARAM_MOVE_Y = "MoveY";
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_ATTACKING = "IsAttacking";
    private const string PARAM_COMBO_INDEX = "ComboIndex";
    private const string TRIGGER_ATTACK = "Attack";
    private const string TRIGGER_SPECIAL = "SpecialAttack";
    private const string TRIGGER_DODGE = "Dodge";
    private const string TRIGGER_HIT = "Hit";
    
    private PlayerController _playerController;
    private Vector2 _lastMoveDirection;
    
    void Start()
    {
        // Get required components
        _playerController = GetComponent<PlayerController>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
        if (_playerController == null)
        {
            Debug.LogError("PlayerAnimationController requires PlayerController component!");
        }
    }
    
    void Update()
    {
        if (_playerController == null || animator == null) return;
        
        UpdateAnimationParameters();
    }
    
    void UpdateAnimationParameters()
    {
        // Get movement input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveY) > 0.1f;
        
        // Update movement parameters
        animator.SetBool(PARAM_IS_MOVING, isMoving);
        
        // Keep last move direction when stopping
        if (isMoving)
        {
            _lastMoveDirection = new Vector2(moveX, moveY).normalized;
        }
        
        // Set direction for blend tree
        animator.SetFloat(PARAM_MOVE_X, _lastMoveDirection.x);
        animator.SetFloat(PARAM_MOVE_Y, _lastMoveDirection.y);
        
        // Update attack state
        animator.SetBool(PARAM_IS_ATTACKING, _playerController.IsAttacking());
    }
    
    // Called by PlayerController when attacking
    public void TriggerAttack(int comboIndex)
    {
        if (animator == null) return;
        
        animator.SetInteger(PARAM_COMBO_INDEX, comboIndex);
        animator.SetTrigger(TRIGGER_ATTACK);
    }
    
    // Called by PlayerController for special attack
    public void TriggerSpecialAttack()
    {
        if (animator == null) return;
        
        animator.SetTrigger(TRIGGER_SPECIAL);
    }
    
    // Called by PlayerController when dodging
    public void TriggerDodge()
    {
        if (animator == null) return;
        
        animator.SetTrigger(TRIGGER_DODGE);
    }
    
    // Called when player takes damage
    public void TriggerHit()
    {
        if (animator == null) return;
        
        animator.SetTrigger(TRIGGER_HIT);
    }
}