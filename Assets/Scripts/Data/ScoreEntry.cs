using System;

[Serializable]
public class ScoreEntry
{
    public string PlayerName = "Player";
    public int Score = 0;
    public string SubmitTimestamp = DateTime.Now.ToString("yyyymmddhhmmssfff");
}