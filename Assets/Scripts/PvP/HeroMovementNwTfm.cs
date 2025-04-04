using Unity.Netcode;
using UnityEngine;

public class HeroMovementNwTfm : NetworkBehaviour
{
    [SerializeField] CharacterController ctrl;
    [SerializeField] Animator anims;
    [SerializeField] float animDamping = 0.1f;

    private NetworkVariable<StructPlayer> netPlayerData = new NetworkVariable<StructPlayer>(writePerm: NetworkVariableWritePermission.Server);

    void Update()
    {
        if (IsOwner)
        {
            StructPlayer data = new StructPlayer
            {
                pos = transform.position,
                rot = transform.rotation,
                vel = ctrl.velocity,
            };
            AnimationVelocity(ctrl.velocity.magnitude);
            UpdatePlayerServerRpc(data);
        }
        else
        {
            transform.position = netPlayerData.Value.pos;
            transform.rotation = netPlayerData.Value.rot;
            ctrl.Move(netPlayerData.Value.vel * Time.deltaTime);
            AnimationVelocity(netPlayerData.Value.vel.magnitude);
        }
    }

    void AnimationVelocity(float v)
    {
        anims.SetFloat("velocity", v, animDamping, Time.deltaTime);
    }

    [ServerRpc]
    void UpdatePlayerServerRpc(StructPlayer data)
    {
        netPlayerData.Value = data;
        UpdatePlayerClientRpc(data);
    }

    [ClientRpc]
    void UpdatePlayerClientRpc(StructPlayer data)
    {
        if (!IsOwner)
        {
            transform.position = data.pos;
            transform.rotation = data.rot;
        }
    }
}
