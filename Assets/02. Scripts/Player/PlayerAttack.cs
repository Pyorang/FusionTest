using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimator), typeof(PlayerController))]
public class PlayerAttack : NetworkBehaviour
{
    [Header("콤보 설정")]
    [Tooltip("공격 순서 (Animator의 ComboIndex 값)")]
    [SerializeField] private int[] comboOrder = { 0, 1, 2 };

    private PlayerController _playerController;
    private PlayerStat _stat;
    private PlayerAnimator _playerAnimator;

    [Networked] public NetworkBool IsAttacking { get; set; }
    [Networked] private int CurrentComboStep { get; set; }
    [Networked] private int AttackTriggerCount { get; set; }

    private bool _canNextAttack;
    private bool _pendingAttackEnd;

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _playerController = GetComponent<PlayerController>();
        _stat = _playerController.Stat;
        _playerAnimator = GetComponent<PlayerAnimator>();
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input)) return;
        if (_playerController.NetworkedHP <= 0) return;

        if (_pendingAttackEnd)
        {
            IsAttacking = false;
            _canNextAttack = false;
            CurrentComboStep = 0;
            _pendingAttackEnd = false;
        }

        if (input.attack)
            TryAttack();
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(AttackTriggerCount))
            {
                _playerAnimator.SetComboIndex(comboOrder[CurrentComboStep]);
                _playerAnimator.TriggerAttack();
            }
        }
    }

    private void TryAttack()
    {
        if (!IsAttacking)
        {
            if (_playerController.NetworkedStamina < _stat.AttackStaminaRequired) return;

            _playerController.ModifyStamina(-_stat.AttackStaminaCost);
            IsAttacking = true;
            CurrentComboStep = 0;
            AttackTriggerCount++;
            _canNextAttack = false;
        }
        else if (_canNextAttack)
        {
            if (_playerController.NetworkedStamina < _stat.AttackStaminaRequired) return;

            _playerController.ModifyStamina(-_stat.AttackStaminaCost);
            _canNextAttack = false;
            CurrentComboStep = (CurrentComboStep + 1) % comboOrder.Length;
            AttackTriggerCount++;
        }
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출 — 다음 콤보 입력 허용 시점
    /// </summary>
    public void OnCanNextAttack()
    {
        _canNextAttack = true;
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출 — 공격 모션 종료 시점
    /// </summary>
    public void OnAttackEnd()
    {
        _pendingAttackEnd = true;
        _canNextAttack = false;
    }
}
