using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    Animator _anim;
    PlayerController _controller;
    Rigidbody2D _rb;

    static readonly int IsRunning = Animator.StringToHash("isWalking");
    static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    static readonly int IsDashing = Animator.StringToHash("isDashing");
    //static readonly int IsBackstep = Animator.StringToHash("isBackstep");
    static readonly int IsFalling = Animator.StringToHash("isFalling");
    static readonly int JumpTrig = Animator.StringToHash("jumpTrigger");
    static readonly int DoubleJumpTrig = Animator.StringToHash("doubleJumpTrigger");
    static readonly int IsDropping = Animator.StringToHash("isDropping");
    int _lastRemainingJumps;

    bool _wasGroundedLastFrame;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<PlayerController>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        bool grounded = _controller.IsGrounded;

        if (!grounded && _controller.remainingJumps < _lastRemainingJumps)
            _anim.SetTrigger(DoubleJumpTrig);

        _lastRemainingJumps = _controller.remainingJumps;

        if (_wasGroundedLastFrame && !grounded && !_controller._isDroppingDown)
            _anim.SetTrigger(JumpTrig);
        if (_controller._isDroppingDown)
        {
            _anim.SetBool(IsDropping, true);
            return; 
        }

        _anim.SetBool(IsDropping, false);

        _wasGroundedLastFrame = grounded;

        _anim.SetBool(IsGrounded, grounded);
        _anim.SetBool(IsFalling, !grounded && _rb.linearVelocityY < -0.1f);
        _anim.SetBool(IsDashing, _controller._isDashing);
        //_anim.SetBool(IsBackstep, _controller._wasBackstep);

        float vx = Mathf.Abs(_rb.linearVelocityX);
        _anim.SetBool(IsRunning, vx > 0.1f && grounded);
    }
}