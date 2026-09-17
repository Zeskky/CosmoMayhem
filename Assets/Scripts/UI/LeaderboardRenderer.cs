using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardRenderer : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardEntryGO;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RenderLeaderboard();
    }

    public void RenderLeaderboard()
    {
        List<LeaderboardEntry> entries = GetComponentsInChildren<LeaderboardEntry>().ToList();
        foreach (LeaderboardEntry entry in entries)
        {
            entry.transform.SetParent(null);
            Destroy(entry.gameObject);
        }

        foreach (ScoreEntry se in LocalScoresManager.Instance.LocalScores.soloScores)
        {
            GameObject newEntry = Instantiate(leaderboardEntryGO, leaderboardEntryGO.transform.parent);
            newEntry.GetComponent<LeaderboardEntry>().UpdateEntryData(se.PlayerName, se.Score);
            newEntry.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
