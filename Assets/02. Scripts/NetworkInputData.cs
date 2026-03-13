using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public float horizontal;
    public float vertical;
    public float mouseX;
    public float mouseY;
    public NetworkBool jump;
    public NetworkBool sprint;
}
