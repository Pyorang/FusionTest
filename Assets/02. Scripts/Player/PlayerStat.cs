using System;
using UnityEngine;

[Serializable]
public class PlayerStat
{
    public float MoveSpeed = 7f;
    public float JumpPower = 2.5f;
    public float RotationSpeed = 100f;
    public float AttackPower = 25f;

    private float _hp = 100f;
    private float _maxHp = 100f;

    public float MaxHp => _maxHp;
    public float HP
    {
        get => _hp;
        set
        {
            _hp = Mathf.Clamp(value, 0f, _maxHp);
            OnHpChanged?.Invoke(_hp, _maxHp);
        }
    }
    public event Action<float, float> OnHpChanged;

    private float _stamina = 100f;
    private float _maxStamina = 100f;

    public float MaxStamina => _maxStamina;
    public float Stamina
    {
        get => _stamina;
        set
        {
            _stamina = Mathf.Clamp(value, 0f, _maxStamina);
            OnStaminaChanged?.Invoke(_stamina, _maxStamina);
        }
    }
    public event Action<float, float> OnStaminaChanged;

    public float SprintSpeedMultiplier = 1.8f;
    public float StaminaDrainRate = 20f;
    public float StaminaRegenRate = 15f;
    public float StaminaRegenDelay = 1f;

    public float AttackStaminaCost = 15f;
    public float AttackStaminaRequired = 15f;
    public float JumpStaminaCost = 10f;
    public float JumpStaminaRequired = 10f;
}
