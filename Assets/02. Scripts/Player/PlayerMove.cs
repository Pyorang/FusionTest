using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkCharacterController), typeof(PlayerController))]
public class PlayerMove : NetworkBehaviour
{
    [Header("낙하 사망")]
    [SerializeField] private float _lethalFallDistance = 10f;

    // Networked 상태
    [Networked] private NetworkBool IsSprinting { get; set; }
    [Networked] private float StaminaRegenTimer { get; set; }
    [Networked] private float HighestY { get; set; }
    [Networked] private NetworkBool WasGrounded { get; set; }

    // 로컬 참조
    private PlayerStat _stat;
    private PlayerController _playerController;
    private PlayerRotate _playerRotate;
    private PlayerAnimator _playerAnimator;
    private PlayerAttack _playerAttack;
    private NetworkCharacterController _ncc;

    public override void Spawned()
    {
        _playerController = GetComponent<PlayerController>();
        _stat = _playerController.Stat;
        _playerRotate = GetComponent<PlayerRotate>();
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerAttack = GetComponent<PlayerAttack>();
        _ncc = GetComponent<NetworkCharacterController>();

        // NCC 회전 비활성화 — PlayerRotate에서 회전을 직접 처리
        _ncc.rotationSpeed = 0f;

        // 낙하 추적 초기화
        HighestY = transform.position.y;
        WasGrounded = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input)) return;
        if (_playerController.IsDead)
        {
            _ncc.Move(Vector3.zero);
            return;
        }

        if (_playerAttack != null && _playerAttack.IsAttacking)
        {
            // 공격 중에는 이동/점프 불가, 중력 + 스태미나 회복만 처리
            _ncc.Move(Vector3.zero);
            HandleStamina(input);
            CheckFallDamage();
            return;
        }

        HandleMovement(input);
        HandleJump(input);
        HandleStamina(input);
        CheckFallDamage();
    }

    public override void Render()
    {
        // 애니메이션은 매 렌더 프레임에서
        Vector3 horizontalVel = new Vector3(_ncc.Velocity.x, 0f, _ncc.Velocity.z);
        _playerAnimator.SetMoveSpeed(horizontalVel.magnitude);
    }

    private void HandleMovement(NetworkInputData input)
    {
        Vector3 inputDir = new Vector3(input.horizontal, 0f, input.vertical).normalized;

        // 카메라 yaw 기준으로 이동 방향 변환
        Vector3 moveDir = _playerRotate != null
            ? _playerRotate.GetYawRotation() * inputDir
            : inputDir;

        bool wantSprint = input.sprint && moveDir.magnitude > 0f;
        IsSprinting = wantSprint && _playerController.NetworkedStamina > 0f;

        // NCC의 maxSpeed를 달리기 여부에 따라 동적 변경
        _ncc.maxSpeed = IsSprinting
            ? _stat.MoveSpeed * _stat.SprintSpeedMultiplier
            : _stat.MoveSpeed;

        // NCC.Move가 중력, 가감속, 지면체크를 모두 처리
        _ncc.Move(moveDir);
    }

    private void HandleJump(NetworkInputData input)
    {
        if (!input.jump) return;
        if (!_ncc.Grounded) return;
        if (_playerController.NetworkedStamina < _stat.JumpStaminaRequired) return;

        _ncc.Jump(overrideImpulse: _stat.JumpPower);
        _playerController.ModifyStamina(-_stat.JumpStaminaCost);
        StaminaRegenTimer = _stat.StaminaRegenDelay;
    }

    private void HandleStamina(NetworkInputData input)
    {
        if (IsSprinting)
        {
            _playerController.ModifyStamina(-_stat.StaminaDrainRate * Runner.DeltaTime);
            StaminaRegenTimer = _stat.StaminaRegenDelay;
        }
        else
        {
            StaminaRegenTimer -= Runner.DeltaTime;

            if (StaminaRegenTimer <= 0f)
            {
                _playerController.ModifyStamina(_stat.StaminaRegenRate * Runner.DeltaTime);
            }
        }
    }

    private void CheckFallDamage()
    {
        bool isGrounded = _ncc.Grounded;

        if (isGrounded)
        {
            if (!WasGrounded)
            {
                float fallDistance = HighestY - transform.position.y;
                if (fallDistance >= _lethalFallDistance)
                {
                    _playerController.TakeDamage(_playerController.NetworkedHP);
                }
            }
            HighestY = transform.position.y;
        }
        else
        {
            if (transform.position.y > HighestY)
                HighestY = transform.position.y;
        }

        WasGrounded = isGrounded;
    }

    public void ResetFallTracking()
    {
        HighestY = transform.position.y;
        WasGrounded = true;
    }
}
