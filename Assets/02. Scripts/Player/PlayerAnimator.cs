using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator _animator;

    // 파라미터 해시
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int ComboIndex = Animator.StringToHash("ComboIndex");
    private static readonly int DieTrigger = Animator.StringToHash("Die");
    private static readonly int IdleTrigger = Animator.StringToHash("Idle");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat(MoveSpeed, speed, 0.1f, Time.deltaTime);
    }

    public void SetComboIndex(int index)
    {
        _animator.SetInteger(ComboIndex, index);
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(AttackTrigger);
    }

    public void TriggerDie()
    {
        _animator.SetTrigger(DieTrigger);
    }

    public void TriggerIdle()
    {
        _animator.SetTrigger(IdleTrigger);
    }
}
