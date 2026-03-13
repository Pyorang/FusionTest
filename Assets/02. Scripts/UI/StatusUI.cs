using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("UI")]
    [SerializeField] private Image hpBar;
    [SerializeField] private Image staminaBar;

    private PlayerStat _stat;

    private void Start()
    {
        _stat = playerController.Stat;

        // 이벤트 구독
        _stat.OnHpChanged += UpdateHpBar;
        _stat.OnStaminaChanged += UpdateStaminaBar;

        // 초기값 반영
        UpdateHpBar(_stat.HP, _stat.MaxHp);
        UpdateStaminaBar(_stat.Stamina, _stat.MaxStamina);
    }

    private void OnDisable()
    {
        if (_stat == null) return;
        _stat.OnHpChanged -= UpdateHpBar;
        _stat.OnStaminaChanged -= UpdateStaminaBar;
    }

    private void UpdateHpBar(float current, float max)
    {
        hpBar.fillAmount = current / max;
    }

    private void UpdateStaminaBar(float current, float max)
    {
        staminaBar.fillAmount = current / max;
    }
}
