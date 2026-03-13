using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStat stat;

    public PlayerStat Stat => stat;
}
