using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ScoreArchive
{
    public List<ScoreEntry> soloScores = new();
}

public class LocalScoresManager : MonoBehaviour
{
    public static LocalScoresManager Instance { get; private set; }

    [SerializeField] private List<ScoreEntry> initialScores;
    public ScoreArchive LocalScores { get; private set; }
    [SerializeField] private int maxHighScores = 10;
    [SerializeField] private string localScoresKey = "LocalScores";

    private void Awake()
    {
        Instance = this;
        InitializeLocalScores();
    }

    /// <summary>
    /// Submits a local score.
    /// </summary>
    /// <param name="entry">The ScoreEntry to submit to the local scores.</param>
    /// <returns>Returns TRUE if the score was added successfully to the score entry list. Otherwise, returns FALSE.</returns>
    public bool SubmitScoreEntry(ScoreEntry entry)
    {
        if (entry == null) return false;

        LocalScores.soloScores.Add(entry);
        LocalScores.soloScores = LocalScores.soloScores
            .OrderByDescending(ls => ls.Score)
            .ThenByDescending(ls => ls.SubmitTimestamp)
            .Take(maxHighScores)
            .ToList();
    
        return LocalScores.soloScores.Contains(entry);
    }

    public void WriteScoresToDisk()
    {
        string jsonDump = JsonUtility.ToJson(LocalScores);
        print(jsonDump);
        PlayerPrefs.SetString(localScoresKey, jsonDump);
        PlayerPrefs.Save();
    }

    [ContextMenu("Clear Local Scores")]
    public void ClearLocalScores()
    {
        PlayerPrefs.DeleteKey(localScoresKey);
        InitializeLocalScores();
        print("Local scores have been reset successfully.");
    }

    public void InitializeLocalScores()
    {
        if (PlayerPrefs.HasKey(localScoresKey))
        {
            string localScoresData = PlayerPrefs.GetString(localScoresKey);
            LocalScores = JsonUtility.FromJson<ScoreArchive>(localScoresData);
        }
        else
        {
            LocalScores = new()
            {
                soloScores = initialScores
            };
        }
    }

    public void OnApplicationQuit()
    {
        WriteScoresToDisk();
    }
}