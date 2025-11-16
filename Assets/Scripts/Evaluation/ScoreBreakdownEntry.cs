using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreBreakdownEntry : MonoBehaviour
{
    [SerializeField] private ScoreType scoreType;
    [SerializeField] private TMP_Text scoreDisplayBack, scoreDisplayFill;
    [SerializeField] private float charSpacing = 0.825f, scoreUpdateDuration = 3f;
    [SerializeField] private int digits = 8;
    private int displayedScore = 0, targetScore = 0;

    private void Start()
    {
        StageStats latestStageStats = Launcher.Instance.GameStageStats.LastOrDefault();
        if (latestStageStats != null)
        {
            targetScore = scoreType != ScoreType.None
                ? latestStageStats.ScoreBreakdown[scoreType]
                : latestStageStats.TotalScore;
        }
    }

    private void FixedUpdate()
    {
        if (displayedScore < targetScore)
        {
            string monospaceTag = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>";
            displayedScore = (int)Mathf.Min(displayedScore + Time.fixedDeltaTime * (1 / scoreUpdateDuration), GameManager.Instance.CurrentStageStats.TotalScore);
            scoreDisplayFill.text = $"{monospaceTag}{displayedScore}";
            scoreDisplayBack.text = $"{monospaceTag}{displayedScore.ToString().PadLeft(digits, '0')}";
        }
    }
}