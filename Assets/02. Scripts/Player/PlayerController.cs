using System.Collections;
using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private PlayerStat stat;
    [SerializeField] private float respawnDelay = 5f;

    public PlayerStat Stat => stat;
    public bool IsDead { get; private set; }

    [Networked] public float NetworkedHP { get; set; }
    [Networked] public float NetworkedStamina { get; set; }

    private PlayerAnimator _playerAnimator;
    private PlayerMove _playerMove;
    private NetworkCharacterController _ncc;

    public override void Spawned()
    {
        _playerAnimator = GetComponent<PlayerAnimator>();
        _playerMove = GetComponent<PlayerMove>();
        _ncc = GetComponent<NetworkCharacterController>();

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
        _playerAnimator.TriggerDie();
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
        _playerAnimator.TriggerIdle();
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
