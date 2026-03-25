using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed, jumpForce, moveForce;
    public Transform groundCheck;
    private Rigidbody2D _myRigidBody;
    private InputAction _jump, _move, _dash, _down;
    private bool _grounded, _jumpInitiated;
    public bool IsGrounded => _grounded;
    public Vector2 LastGroundedPosition { get; private set; }
    private bool _orientation;
    private bool _isFacingRight = true;
    private bool _inAir = false;
    public int maxJump = 2;
    public int remainingJumps;
    public int maxDash = 1;
    public int remainingDash;
    public float dashForce = 80f;
    public float dashCooldownTimer = 3f;
    public float dashCooldown = 3f;
    private bool _isOnPlatform;
    public bool _wasBackstep = false;
    private Collider2D _platformCollider;
    public bool _isDashing = false;
    private float _dashDuration = 0.15f;
    private Renderer _renderer;
    private Collider2D _playerCollider;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;

    public bool _isDroppingDown = false;
    private float _dropDownBuffer = 0.2f;
    private float _dropDownTimer = 0f;
    private bool _ignorePlatformCollision = false;

    private void Start()
    {
        _playerCollider = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
    }

    void Awake()
    {
        _myRigidBody = GetComponent<Rigidbody2D>();
        _jump = InputSystem.actions.FindAction("Jump");
        _move = InputSystem.actions.FindAction("Move");
        _dash = InputSystem.actions.FindAction("Sprint");
        _down = InputSystem.actions.FindAction("Down");
        _jumpInitiated = false;
        remainingJumps = maxJump;
        _orientation = _isFacingRight;
        remainingDash = maxDash;
        dashCooldownTimer = 0f;
    }

    void Update()
    {
        if (_dropDownTimer > 0)
        {
            _dropDownTimer -= Time.deltaTime;
        }

        if (!_ignorePlatformCollision)
        {
            _grounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

            Collider2D platformHit = Physics2D.OverlapCircle(groundCheck.position, 0.1f, platformLayer);
            _isOnPlatform = platformHit != null;

            if (_isOnPlatform && !_down.IsPressed())
            {
                _platformCollider = platformHit;
                _grounded = true;
                remainingJumps = maxJump;
            }
        }
        else
        {
            _grounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
            _isOnPlatform = false;
        }

        InAir();

        if (_grounded)
            LastGroundedPosition = transform.position;

        if (_isOnPlatform && _down.IsPressed() && _jump.WasPressedThisFrame() && !_isDroppingDown)
        {
            StartCoroutine(DropThroughPlatform());
            _jumpInitiated = false; 
            return; 
        }
        if (_jump.WasPressedThisFrame() && _grounded && !_isDroppingDown)
        {
            _jumpInitiated = true;
        }

        if (_inAir && _jump.WasPressedThisFrame() && remainingJumps > 0 && !_isDroppingDown)
        {
            _myRigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            Debug.Log("double jump");
            remainingJumps--;
        }

        if (_grounded)
        {
            remainingJumps = maxJump;
        }

        CheckOrientation();

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
            _renderer.material.color = Color.cyan;
        }
        else if (remainingDash < maxDash)
        {
            _renderer.material.color = Color.white;
            remainingDash = maxDash;
        }

        if (_dash.WasPressedThisFrame() && remainingDash > 0 && dashCooldownTimer <= 0f && !_isDroppingDown)
        {
            PerformDash();
            _renderer.material.color = Color.blue;
            remainingDash--;
            dashCooldownTimer = dashCooldown;
        }
    }

    void FixedUpdate()
    {
        if (_isDashing || _isDroppingDown)
            return;

        float horizontalMovement = _move.ReadValue<Vector2>().x;

        if (horizontalMovement * _myRigidBody.linearVelocityX < maxSpeed)
        {
            _myRigidBody.AddForce(Vector2.right * horizontalMovement * moveForce);
        }

        if (Mathf.Abs(_myRigidBody.linearVelocityX) > maxSpeed)
        {
            _myRigidBody.linearVelocityX = Mathf.Sign(_myRigidBody.linearVelocityX) * maxSpeed;
        }

        if (_jumpInitiated)
        {
            _myRigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            Debug.Log("jumping");
            _jumpInitiated = false;
            remainingJumps--;
        }
    }

    void CheckOrientation()
    {
        float horizontalMovement = _move.ReadValue<Vector2>().x;
        if (Mathf.Abs(horizontalMovement) > 0.1f)
        {
            _orientation = _isFacingRight;
            _isFacingRight = horizontalMovement > 0;
            if (_isFacingRight != _orientation)
            {
                FlipSprite();
            }
        }
    }

    void FlipSprite()
    {
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    void InAir()
    {
        _inAir = !_grounded;
    }

    void PerformDash()
    {
        float horizontalInput = _move.ReadValue<Vector2>().x;
        Vector2 dashDirection;

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            dashDirection = horizontalInput > 0 ? Vector2.right : Vector2.left;
            Debug.Log("Dashing");
        }
        else
        {
            dashDirection = _isFacingRight ? Vector2.left : Vector2.right;
            Debug.Log("Backstep");
            _wasBackstep = true;
        }

        _isDashing = true;
        _myRigidBody.linearVelocity = new Vector2(dashDirection.x * dashForce, _myRigidBody.linearVelocity.y);
        StartCoroutine(EndDashAfterDelay(_dashDuration));
    }

    IEnumerator EndDashAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isDashing = false;
        _wasBackstep = false;
    }

    IEnumerator DropThroughPlatform()
    {
        if (_platformCollider != null)
        {
            _isDroppingDown = true;
            _ignorePlatformCollision = true;

            Physics2D.IgnoreCollision(_playerCollider, _platformCollider, true);

            _myRigidBody.linearVelocity = new Vector2(_myRigidBody.linearVelocity.x, -5f);

            Debug.Log("Dropping through platform");

            yield return new WaitForSeconds(0.3f);

            Physics2D.IgnoreCollision(_playerCollider, _platformCollider, false);

            _ignorePlatformCollision = false;
            _isDroppingDown = false;
            _platformCollider = null;
        }
    }
}