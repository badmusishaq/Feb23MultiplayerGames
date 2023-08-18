using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class SimpleChat : NetworkBehaviour
{
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] TMP_Text chatText;

    public void SendChat()
    {
        if(IsServer)
        {
            ChatClientRPC();
        }
        else if(IsClient)
        {
            ChatServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC()
    {
        chatText.text = "A client says hi!";
    }

    [ClientRpc]
    public void ChatClientRPC()
    {
        chatText.text = "Server says hi";
    }
}
