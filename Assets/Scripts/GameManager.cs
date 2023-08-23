using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [SerializeField] TMP_InputField playerNameField;

    private NetworkObject localPlayer;

    public NetworkVariable<short> state = new NetworkVariable<short>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    [SerializeField] Transform[] startPositions;

    [Header("UI Elements")]
    [SerializeField] GameObject endGameScreen;
    [SerializeField] TMP_Text endGameMessage;

    [SerializeField] TMP_Text scoreUI;


    void Singleton()
    {
        if(instance != null && instance != this)
        {
            Destroy(instance);
        }

        instance = this;
    }

    void Awake()
    {
        Singleton();

        if(IsServer)
        {
            state.Value = 0;
        }
    }

    public void SetLocalPlayer(NetworkObject _localPlayer)
    {
        localPlayer = _localPlayer;

        if(playerNameField.text.Length > 0)
        {
            localPlayer.GetComponent<PlayerInfo>().SetName(playerNameField.text);
        }
        else
        {
            localPlayer.GetComponent<PlayerInfo>().SetName($"Player-{localPlayer.OwnerClientId}");
        }

        playerNameField.gameObject.SetActive(false);
    }

    public void OnPlayerJoined(NetworkObject playerObj)
    {
        //Assign position to the player when the player joins
        playerObj.transform.position = startPositions[(int)playerObj.OwnerClientId].position;
        playerScores.Add(playerObj.OwnerClientId, 0);
    }

    public void StartGame()
    {
        state.Value = 1;
        ShowScoreUI();
    }

    public void SetPlayerName(NetworkObject playerObj, string name)
    {
        if(playerNames.ContainsKey(playerObj.OwnerClientId))
        {
            playerNames[playerObj.OwnerClientId] = name;
        }
        else
        {
            playerNames.Add(playerObj.OwnerClientId, name);
        }
    }

    public void AddScore(ulong playerID)
    {
        if(IsServer)
        {
            playerScores[playerID]++;
            ShowScoreUI();
            CheckWinner(playerID);
        }
    }

    public void ShowScoreUI()
    {
        scoreUI.text = "";

        PlayerScores scores = new PlayerScores();
        scores.scores = new List<ScoreInfo>();

        foreach(var item in playerScores)
        {
            ScoreInfo temp = new ScoreInfo();
            temp.score = item.Value;
            temp.id = item.Key;
            temp.name = playerNames[item.Key];
            scores.scores.Add(temp);

            scoreUI.text = $"[{item.Key}] {playerNames[item.Key]} : {item.Value}/10\n";
        }

        //Update the client side
        UpdateClientScoreClientRPC(JsonUtility.ToJson(scores));
    }


    [ClientRpc]
    public void UpdateClientScoreClientRPC(string scoreInfo)
    {
        PlayerScores scores = JsonUtility.FromJson<PlayerScores>(scoreInfo);

        scoreUI.text = "";

        foreach(var item in scores.scores)
        {
            scoreUI.text += $"[{item.id}] {item.name} : {item.score}/10\n";
        }
    }

    void CheckWinner(ulong playerID)
    {
        if(playerScores[playerID] >= 10)
        {
            //End the game
            EndGame(playerID);
        }
    }

    public void EndGame(ulong winnerID)
    {
        if(IsServer)
        {
            endGameScreen.SetActive(true);

            if(winnerID == NetworkManager.LocalClientId)
            {
                endGameMessage.text = "YOU WIN!";
            }
            else
            {
                endGameMessage.text = $"YOU LOSE! \n The winner is {playerNames[winnerID]}";
            }

            ScoreInfo temp = new ScoreInfo();
            temp.score = playerScores[winnerID];
            temp.id = winnerID;
            temp.name = playerNames[winnerID];

            ShowGameEndUIClientRPC(JsonUtility.ToJson(temp));
        }
    }


    [ClientRpc]
    public void ShowGameEndUIClientRPC(string winnerInfo)
    {
        endGameScreen.SetActive(true);
        ScoreInfo info = JsonUtility.FromJson<ScoreInfo>(winnerInfo);

        if(info.id == NetworkManager.LocalClientId)
        {
            endGameMessage.text = "YOU WIN!";
        }
        else
        {
            endGameMessage.text = $"YOU LOSE! \n The winner is {info.name}";
        }
    }

    public void ResetPlayerPosition(NetworkObject playerObj, ulong playerID)
    {
        playerObj.transform.position = startPositions[(int)playerID].position;
    }
}

[System.Serializable]
public class PlayerScores
{
    public List<ScoreInfo> scores;
}


[System.Serializable]
public class ScoreInfo
{
    public ulong id;
    public string name;
    public int score;
}
