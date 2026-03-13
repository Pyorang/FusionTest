using System.Collections;
using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private PlayerStat stat;
    [SerializeField] private float respawnDelay = 5f;

    public PlayerStat Stat => stat;

    [Networked] public NetworkBool IsDead { get; set; }
    [Networked] public float NetworkedHP { get; set; }
    [Networked] public float NetworkedStamina { get; set; }

    private PlayerAnimator _playerAnimator;
    private PlayerMove _playerMove;
    private NetworkCharacterController _ncc;
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerMove = GetComponent<PlayerMove>();
        _ncc = GetComponent<NetworkCharacterController>();
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            NetworkedHP = stat.MaxHp;
            NetworkedStamina = stat.MaxStamina;
        }
    }

    public void ModifyHP(float amount)
    {
        NetworkedHP = Mathf.Clamp(NetworkedHP + amount, 0f, stat.MaxHp);
    }

    public void ModifyStamina(float amount)
    {
        NetworkedStamina = Mathf.Clamp(NetworkedStamina + amount, 0f, stat.MaxStamina);
    }

    public override void Render()
    {
        stat.HP = NetworkedHP;
        stat.Stamina = NetworkedStamina;

        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(IsDead))
            {
                if (IsDead)
                    _playerAnimator.TriggerDie();
                else
                    _playerAnimator.TriggerIdle();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        ModifyHP(-damage);
        if (NetworkedHP <= 0f) Die();
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 respawnPos = GetRandomSpawnPosition();
        _ncc.Teleport(respawnPos);
        ResetStats();
        _playerMove.ResetFallTracking();

        IsDead = false;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Transform[] spawnPoints = ConnectManager.Instance.SpawnPoints;
        int randomIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex].position;
    }

    private void ResetStats()
    {
        NetworkedHP = stat.MaxHp;
        NetworkedStamina = stat.MaxStamina;
    }
}
