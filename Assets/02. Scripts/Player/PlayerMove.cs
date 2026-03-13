using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CharacterController), typeof(PlayerController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    // 내부 상태
    private PlayerStat _stat;
    private PlayerRotate _playerRotate;
    private PlayerAnimator _playerAnimator;
    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isSprinting;
    private float _staminaRegenTimer;
    private const float Gravity = -9.81f;

    private void Awake()
    {
        _stat = GetComponent<PlayerController>().Stat;
        _playerRotate = GetComponent<PlayerRotate>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleStamina();
        ApplyGravity();
    }

    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        // 카메라 yaw 기준으로 이동 방향 변환
        Vector3 moveDir = _playerRotate != null
            ? _playerRotate.GetYawRotation() * inputDir
            : inputDir;

        bool wantSprint = Input.GetKey(KeyCode.LeftShift) && moveDir.magnitude > 0f;
        _isSprinting = wantSprint && _stat.Stamina > 0f;

        float speed = _isSprinting
            ? _stat.MoveSpeed * _stat.SprintSpeedMultiplier
            : _stat.MoveSpeed;

        _controller.Move(moveDir * speed * Time.deltaTime);

        // 애니메이션: 수평 이동 속도 전달
        Vector3 horizontalVel = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
        _playerAnimator.SetMoveSpeed(horizontalVel.magnitude);
    }

    private void HandleJump()
    {
        if (!Input.GetButtonDown("Jump")) return;
        if (!_isGrounded) return;
        if (_stat.Stamina < _stat.JumpStaminaRequired) return;

        _velocity.y = Mathf.Sqrt(_stat.JumpPower * -2f * Gravity);
        _stat.Stamina -= _stat.JumpStaminaCost;
        _staminaRegenTimer = _stat.StaminaRegenDelay;
    }

    private void HandleStamina()
    {
        if (_isSprinting)
        {
            _stat.Stamina -= _stat.StaminaDrainRate * Time.deltaTime;
            _staminaRegenTimer = _stat.StaminaRegenDelay;
        }
        else
        {
            _staminaRegenTimer -= Time.deltaTime;

            if (_staminaRegenTimer <= 0f)
            {
                _stat.Stamina += _stat.StaminaRegenRate * Time.deltaTime;
            }
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += Gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}
