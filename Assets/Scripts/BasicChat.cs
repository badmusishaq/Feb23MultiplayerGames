using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class BasicChat : NetworkBehaviour
{
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] TMP_Text chatText;

    public void SendChat()
    {
        if(IsServer)
        {
            ChatClientRPC(NetworkManager.Singleton.LocalClientId + ". "+ chatInput.text);
        }
        else if(IsClient)
        {
            ChatServerRPC(NetworkManager.Singleton.LocalClientId + ". "+ chatInput.text);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC(string message)
    {
        if(!IsHost)
        {
            chatText.text += "\n" + message;
        }
        ChatClientRPC(message);
    }

    [ClientRpc]
    public void ChatClientRPC(string message)
    {
        chatText.text += "\n" + message;
    }
}
