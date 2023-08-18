using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerShoot : NetworkBehaviour
{
    Rigidbody tankBody;

    [SerializeField] GameObject bulletObject;
    [SerializeField] Transform shootPoint;
    [SerializeField] float shootSpeed;

    public override void OnNetworkSpawn()
    {
        tankBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;


        if(Input.GetButtonDown("Jump"))
        {
            if(IsServer && IsLocalPlayer)
            {
                //Shoot
                Shoot(OwnerClientId);
            }
            else if(IsClient && IsLocalPlayer)
            {
                RequestShootServerRPC();
            }
        }
    }

    void Shoot(ulong ownerID)
    {
        GameObject bullet = Instantiate(bulletObject, shootPoint.position, shootPoint.rotation);
        bullet.GetComponent<NetworkObject>().Spawn();
        bullet.GetComponent<Bullet>().clientID = ownerID;

        bullet.GetComponent<Rigidbody>().AddForce(tankBody.velocity + bullet.transform.forward * shootSpeed, ForceMode.VelocityChange);
        Destroy(bullet, 5.0f);
    }

    [ServerRpc]
    public void RequestShootServerRPC(ServerRpcParams serverRpcParams = default)
    {
        Shoot(serverRpcParams.Receive.SenderClientId);
    }
}
