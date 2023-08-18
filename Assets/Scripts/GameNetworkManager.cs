using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameNetworkManager : MonoBehaviour
{
    [SerializeField] TMP_Text statusText;


    public void JoinHost()
    {
        NetworkManager.Singleton.StartHost();
        statusText.text = "Joined as Host!";
    }

    public void JoinClient()
    {
        NetworkManager.Singleton.StartClient();
        statusText.text = "Joined as Client!";
    }

    public void JoinServer()
    {
        NetworkManager.Singleton.StartServer();
        statusText.text = "Joined as Server!";
    }
}
