using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class LocalScoresManager : MonoBehaviour
{
    public static LocalScoresManager Instance { get; private set; }
    
    public List<ScoreEntry> LocalScores { get; private set; }
    [SerializeField] private int maxHighScores = 10;

    private void Awake()
    {
        Instance = this;
        LocalScores = new List<ScoreEntry>();
    }

    /// <summary>
    /// Submits a local score.
    /// </summary>
    /// <param name="entry">The ScoreEntry to submit to the local scores.</param>
    /// <returns>Returns TRUE if the score was added successfully to the score entry list. Otherwise, returns FALSE.</returns>
    public bool SubmitScoreEntry(ScoreEntry entry)
    {
        if (entry == null) return false;

        LocalScores.Add(entry);
        LocalScores = LocalScores
            .OrderByDescending(ls => ls.score)
            .ThenByDescending(ls => ls.submitDate)
            .Take(maxHighScores)
            .ToList();
    
        return LocalScores.Contains(entry);
    }

    public void WriteScoresToDisk()
    {
        string jsonDumpContent = JsonUtility.ToJson(LocalScores);
        string jsonPath = Path.Join(Application.persistentDataPath, "localScores.json");
        File.WriteAllText(jsonPath, jsonDumpContent);
        print($"Local scores written at '{jsonPath}' successfully!");
    }

    public void OnApplicationQuit()
    {
        WriteScoresToDisk();
    }
}