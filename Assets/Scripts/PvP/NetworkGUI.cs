using UnityEngine;
using Unity.Netcode;

public class NetworkGUI : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        // Ensure buttons appear only if not already a client or server
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            
            if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
            }
            
            if (GUILayout.Button("Client"))
            {
                NetworkManager.Singleton.StartClient();
            }
        }
        
        GUILayout.EndArea();
    }
}
