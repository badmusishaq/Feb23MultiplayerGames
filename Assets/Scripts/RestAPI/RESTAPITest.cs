using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class RESTAPITest : MonoBehaviour
{
    [SerializeField] TMP_InputField inputUsername, inputPassword, inputScore;

    [SerializeField] TMP_Text txtUsername;
    [SerializeField] GameObject highScoreElement, registerPanel;
    [SerializeField] Transform scoreBoard;

    [SerializeField] GameObject btnRegister, btnLogin;

    
    IEnumerator RegisterUser(string username, string password)
    {
        User user = new User();
        user.username = username;
        user.password = password;
        string dataToUpload = JsonUtility.ToJson(user);

        UnityWebRequest registerUserRequest = UnityWebRequest.Post(
            "https://bootcamp-restapi-practice.xrcourse.com/register", dataToUpload, "application/json");

        yield return registerUserRequest.SendWebRequest();

        Debug.Log($"Response code: {registerUserRequest.responseCode}");
        Debug.Log($"Response Error: {registerUserRequest.error}");
        Debug.Log(registerUserRequest.downloadHandler.text);

        //Login the user in by default if the reg is seuccessful.
        StartCoroutine(LoginUser(username, password));
    }

    IEnumerator LoginUser(string username, string password)
    {
        User user = new User();
        user.username = username;
        user.password = password;
        string dataToUpload = JsonUtility.ToJson(user);

        UnityWebRequest loginUserRequest = UnityWebRequest.Post(
            "https://bootcamp-restapi-practice.xrcourse.com/login", dataToUpload, "application/json");

        yield return loginUserRequest.SendWebRequest();

        Debug.Log($"Response code: {loginUserRequest.responseCode}");
        Debug.Log($"Response Error: {loginUserRequest.error}");
        Debug.Log(loginUserRequest.downloadHandler.text);

        Login loginData = JsonUtility.FromJson<Login>(loginUserRequest.downloadHandler.text);

        PlayerPrefs.SetString("token", loginData.token);
        txtUsername.text = username;

        registerPanel.SetActive(false);
    }

    IEnumerator SubmitScore(int score)
    {
        Score scoreData = new Score();
        scoreData.score = score;
        string dataToUpload = JsonUtility.ToJson(scoreData);

        UnityWebRequest submitScoreRequest = UnityWebRequest.Post(
            "https://bootcamp-restapi-practice.xrcourse.com/submit-score", dataToUpload, "application/json");

        submitScoreRequest.SetRequestHeader("Authorization", PlayerPrefs.GetString("token"));

        yield return submitScoreRequest.SendWebRequest();

        Debug.Log($"Response code: {submitScoreRequest.responseCode}");
        Debug.Log($"Response Error: {submitScoreRequest.error}");
        Debug.Log(submitScoreRequest.downloadHandler.text);

        //Update the scoreboard
        StartCoroutine(UpdateScoreboard());
    }

    IEnumerator UpdateScoreboard()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(
            "https://bootcamp-restapi-practice.xrcourse.com/top-scores");

        yield return webRequest.SendWebRequest();

        Debug.Log($"Response code: {webRequest.responseCode}");
        Debug.Log($"Response Error: {webRequest.error}");
        Debug.Log(webRequest.downloadHandler.text);

        string highScores = $"{{\"highScores\":{webRequest.downloadHandler.text}}}";

        HighScores topScores = JsonUtility.FromJson<HighScores>(highScores);

        for(int i = 0; i < topScores.highScores.Length; i++)
        {
            Transform _highScoreElement;

            if(i < scoreBoard.childCount)
            {
                _highScoreElement = scoreBoard.GetChild(i);
            }
            else
            {
                _highScoreElement = Instantiate(highScoreElement, scoreBoard).transform;
            }

            //Update the element data
            _highScoreElement.GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString();
            _highScoreElement.GetChild(1).GetComponent<TMP_Text>().text = topScores.highScores[i].username;
            _highScoreElement.GetChild(2).GetComponent<TMP_Text>().text = topScores.highScores[i].highScore.ToString();
        }
    }

    public void RegisterUserCall()
    {
        StartCoroutine(RegisterUser(inputUsername.text, inputPassword.text));
        //Disable the register button
    }

    public void LoginUserCall()
    {
        StartCoroutine(LoginUser(inputUsername.text, inputPassword.text));
        //Disable login and register buttons.
    }

    public void SubmitScoreCall()
    {
        StartCoroutine(SubmitScore(int.Parse(inputScore.text)));
    }

    public void UpdateScoreboardCall()
    {
        StartCoroutine(UpdateScoreboard());
    }

    public void LogOut()
    {
        //delete token
        PlayerPrefs.DeleteKey("token");

        registerPanel.SetActive(true);
    }
}
