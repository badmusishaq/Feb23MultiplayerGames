using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    float horizontal, vertical;

    [SerializeField] float tankMoveSpeed = 10, tankTurnSpeed = 10;

    Rigidbody tankBody;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        tankBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if(IsServer && IsLocalPlayer)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        else if(IsClient && IsLocalPlayer)
        {
            MovementServerRPC(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
        
    }

    private void FixedUpdate()
    {
        tankBody.velocity = tankBody.transform.forward * tankMoveSpeed * vertical;
        tankBody.rotation = Quaternion.Euler(transform.eulerAngles + transform.up * horizontal * tankTurnSpeed);
    }

    [ServerRpc]
    public void MovementServerRPC(float _horizontal, float _vertical)
    {
        horizontal = _horizontal;
        vertical = _vertical;
    }
}
