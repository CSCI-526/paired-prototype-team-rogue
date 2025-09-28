using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator _animator;
    private int _directionXHash;
    private int _directionYHash;
    private int _isMovingHash;
    private int _isAttackingHash;
    private int _comboIndexHash;
    private int _isSpecialHash;
    private int _isDodgingHash;
    private int _isHitHash;

    private Vector2 _lastMoveDirection = Vector2.zero;

    void Start()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            _animator = gameObject.AddComponent<Animator>();
        }

        _directionXHash = Animator.StringToHash("DirectionX");
        _directionYHash = Animator.StringToHash("DirectionY");
        _isMovingHash = Animator.StringToHash("IsMoving");
        _isAttackingHash = Animator.StringToHash("IsAttacking");
        _comboIndexHash = Animator.StringToHash("ComboIndex");
        _isSpecialHash = Animator.StringToHash("IsSpecial");
        _isDodgingHash = Animator.StringToHash("IsDodging");
        _isHitHash = Animator.StringToHash("IsHit");
    }

    public void UpdateMovement(Vector2 moveDirection)
    {
        if (moveDirection.magnitude > 0.1f)
        {
            _lastMoveDirection = moveDirection.normalized;
        }

        if (_animator != null)
        {
            _animator.SetFloat(_directionXHash, _lastMoveDirection.x);
            _animator.SetFloat(_directionYHash, _lastMoveDirection.y);
            _animator.SetBool(_isMovingHash, moveDirection.magnitude > 0.1f);
        }
    }

    public void TriggerAttack(int comboIndex = 0)
    {
        if (_animator != null)
        {
            _animator.SetBool(_isAttackingHash, true);
            _animator.SetInteger(_comboIndexHash, comboIndex);
        }
    }

    public void EndAttack()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isAttackingHash, false);
            _animator.SetInteger(_comboIndexHash, 0);
        }
    }

    public void TriggerSpecial()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isSpecialHash, true);
        }
    }

    public void EndSpecial()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isSpecialHash, false);
        }
    }

    public void TriggerDodge()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isDodgingHash, true);
        }
    }

    public void EndDodge()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isDodgingHash, false);
        }
    }

    public void TriggerHit()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isHitHash, true);
        }
    }

    public void EndHit()
    {
        if (_animator != null)
        {
            _animator.SetBool(_isHitHash, false);
        }
    }
}