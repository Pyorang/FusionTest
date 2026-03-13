using Fusion;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerRotate : NetworkBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 60f;

    private Transform _target;
    private PlayerController _playerController;

    [Networked] private float Yaw { get; set; }
    [Networked] private float Pitch { get; set; }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public override void Spawned()
    {
        _target = transform.Find("Target");

        if (HasInputAuthority)
        {
            var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

            if (cinemachineCamera != null && _target != null)
            {
                cinemachineCamera.Follow = _target;
                cinemachineCamera.LookAt = _target;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input)) return;

        Yaw += input.mouseX * mouseSensitivity;
        Pitch -= input.mouseY * mouseSensitivity;
        Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);

        if(_playerController.NetworkedHP > 0)
        {
            // 모든 클라이언트에서 동기화된 Yaw/Pitch로 회전 적용
            transform.rotation = Quaternion.Euler(0f, Yaw, 0f);

            if (_target != null)
                _target.localRotation = Quaternion.Euler(Pitch, 0f, 0f);
        }
    }

    public Quaternion GetYawRotation()
    {
        return Quaternion.Euler(0f, Yaw, 0f);
    }
}
