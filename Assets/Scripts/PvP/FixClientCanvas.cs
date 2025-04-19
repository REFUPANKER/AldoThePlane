using Unity.Netcode;

public class FixClientCanvas : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(IsOwner);
    }
}
