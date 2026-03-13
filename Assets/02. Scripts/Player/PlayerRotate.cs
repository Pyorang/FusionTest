using UnityEngine;
using Unity.Cinemachine;

public class PlayerRotate : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 60f;

    private CinemachineCamera _cinemachineCamera;
    private Transform _target;
    private float _yaw;
    private float _pitch;

    private void Awake()
    {
        _target = transform.Find("Target");
        _cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        if (_cinemachineCamera != null && _target != null)
        {
            _cinemachineCamera.Follow = _target;
            _cinemachineCamera.LookAt = _target;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        // 플레이어 본체는 좌우(yaw)만 회전
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

        // Target은 로컬 기준 상하(pitch)만 → Cinemachine이 추적
        _target.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public Quaternion GetYawRotation()
    {
        return Quaternion.Euler(0f, _yaw, 0f);
    }
}
