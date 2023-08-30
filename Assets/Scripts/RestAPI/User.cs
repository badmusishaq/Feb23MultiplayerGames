[System.Serializable]
public class User
{
    public string username;
    public string password;
    public int highScore;
}

[System.Serializable]
public class HighScores
{
    public User[] highScores;
}
