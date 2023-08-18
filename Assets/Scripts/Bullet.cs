using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public ulong clientID;

    private void OnCollisionEnter(Collision collision)
    {
        if(IsServer)
        {
            PlayerDamage other = collision.gameObject.GetComponent<PlayerDamage>();

            if(other != null && clientID != other.OwnerClientId)
            {
                other.GetDamage();
                Debug.Log(clientID + " " + other.OwnerClientId);

                GameManager.instance.AddScore(clientID);
            }

            Destroy(gameObject);
        }
    }
}
