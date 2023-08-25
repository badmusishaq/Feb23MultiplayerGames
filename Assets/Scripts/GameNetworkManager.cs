using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;

using TMPro;

public class GameNetworkManager : MonoBehaviour
{
    [SerializeField] int maxConnections = 10;

    [Header("Connection UI")]
    [SerializeField] GameObject btnClient;
    [SerializeField] GameObject btnHost;
    [SerializeField] TMP_Text statusText;
    [SerializeField] TMP_Text playerIDText;
    [SerializeField] TMP_InputField joinCodeText;


    private string playerID, joinCode;
    bool clientAuthenticated = false;

    private async void Start()
    {
        await AuthenticatePlayer();
    }

    async Task AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            playerID = AuthenticationService.Instance.PlayerId;
            clientAuthenticated = true;
            playerIDText.text = $"Player ID : {playerID}";
            Debug.Log($"client aunthentication success - {playerID}");
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    public async Task<RelayServerData> AllocateRelayServerAndGetCode(int maxConnections, string region = null)
    {
        Allocation allocation;

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch(Exception e)
        {
            Debug.Log($"Relay allocation request failed - {e}");
            throw;
        }

        Debug.Log($"Server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"Server: {allocation.AllocationId}");

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch(Exception ex)
        {
            Debug.Log($"Unable to create a join code - {ex}");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    IEnumerator ConfigureGetCodeAndJoinHost()
    {
        //Run the task and get the code
        var allocateAndGetCode = AllocateRelayServerAndGetCode(maxConnections);

        while(!allocateAndGetCode.IsCompleted)
        {
            yield return null;
        }

        if(allocateAndGetCode.IsFaulted)
        {
            Debug.LogError($"Cannot start the server due to an exception - {allocateAndGetCode.Exception.Message}");
            yield break;
        }

        var relayServerData = allocateAndGetCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartHost();

        joinCodeText.gameObject.SetActive(true);
        joinCodeText.text = joinCode;
        statusText.text = "Joined as host!";
    }

    public async Task<RelayServerData> JoinRelayServerWithCode(string _joinCode)
    {
        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);
        }
        catch(Exception ex)
        {
            Debug.Log($"Relay allocation join request failed - {ex}");
            throw;
        }

        Debug.Log($"Client : {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"Host : {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"Client : {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }

    IEnumerator ConfigureUseCodeJoinClient(string _joinCode)
    {
        var joinAllocationFromCode = JoinRelayServerWithCode(_joinCode);

        while(!joinAllocationFromCode.IsCompleted)
        {
            yield return null;
        }

        if(joinAllocationFromCode.IsFaulted)
        {
            Debug.LogError($"Cannot join the relay server due to an exception {joinAllocationFromCode.Exception.Message}");
            yield break;
        }

        var relayServerData = joinAllocationFromCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();

        statusText.text = "Joined as Client!";
    }
    public void JoinHost()
    {
        if(!clientAuthenticated)
        {
            Debug.Log("Client is not authenticated. Please try again");
            return;
        }

        StartCoroutine(ConfigureGetCodeAndJoinHost());

        btnClient.gameObject.SetActive(false);
        btnHost.gameObject.SetActive(false);
        joinCodeText.gameObject.SetActive(false);
    }

    public void JoinClient()
    {
        if(!clientAuthenticated)
        {
            Debug.Log("Client is not authenticatd. Please try again");
            return;
        }

        if(joinCodeText.text.Length <= 0)
        {
            Debug.Log("Enter a proper code");
            statusText.text = "Enter a proper code";
        }

        StartCoroutine(ConfigureUseCodeJoinClient(joinCodeText.text));

        btnClient.gameObject.SetActive(false);
        btnHost.gameObject.SetActive(false);
        joinCodeText.gameObject.SetActive(false);
    }

    public void JoinServer()
    {
        if(!clientAuthenticated)
        {
            Debug.Log("Client is not authneticated. Please try again");
            return;
        }

        NetworkManager.Singleton.StartServer();
        statusText.text = "Joined as Server!";
    }
}
