using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class PlayerInfo : NetworkBehaviour
{
    [SerializeField] TMP_Text txtPlayerName;

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("Player Name"),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;

        txtPlayerName.SetText(playerName.Value.ToString());
        gameObject.name = "Player_" + playerName.Value.ToString();

        if(IsLocalPlayer)
        {
            GameManager.instance.SetLocalPlayer(NetworkObject);
        }

        GameManager.instance.OnPlayerJoined(NetworkObject);
    }

    public void SetName(string name)
    {
        playerName.Value = new FixedString64Bytes(name);
    }

    void OnNameChanged(FixedString64Bytes prevVal, FixedString64Bytes newVal)
    {
        if(newVal != prevVal)
        {
            txtPlayerName.SetText(newVal.Value);
            GameManager.instance.SetPlayerName(NetworkObject, newVal.Value);
        }
    }
}
