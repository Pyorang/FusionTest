using UnityEngine;

[RequireComponent(typeof(PlayerAnimator), typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("콤보 설정")]
    [Tooltip("공격 순서 (Animator의 ComboIndex 값)")]
    [SerializeField] private int[] comboOrder = { 0, 1, 2 };

    private PlayerController _playerController;
    private PlayerStat _stat;
    private PlayerAnimator _playerAnimator;
    private int _currentCombo;
    private bool _isAttacking;
    private bool _canNextAttack;

    public bool IsAttacking => _isAttacking;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _stat = _playerController.Stat;
        _playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _playerController.NetworkedHP > 0)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!_isAttacking)
        {
            if (_playerController.NetworkedStamina < _stat.AttackStaminaRequired) return;

            _playerController.ModifyStamina(-_stat.AttackStaminaCost);
            _isAttacking = true;
            _currentCombo = 0;
            _playerAnimator.SetComboIndex(comboOrder[_currentCombo]);
            _playerAnimator.TriggerAttack();
        }
        else if (_canNextAttack)
        {
            if (_playerController.NetworkedStamina < _stat.AttackStaminaRequired) return;

            _playerController.ModifyStamina(-_stat.AttackStaminaCost);
            _canNextAttack = false;
            _currentCombo++;

            if (_currentCombo >= comboOrder.Length)
                _currentCombo = 0;

            _playerAnimator.SetComboIndex(comboOrder[_currentCombo]);
            _playerAnimator.TriggerAttack();
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
        _isAttacking = false;
        _canNextAttack = false;
        _currentCombo = 0;
    }
}
