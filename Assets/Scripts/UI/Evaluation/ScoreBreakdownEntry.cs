using FMODUnity;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreBreakdownEntry : MonoBehaviour
{
    [SerializeField] private ScoreType scoreType;
    [SerializeField] private TMP_Text scoreDisplayBack, scoreDisplayFill;
    [SerializeField] private StudioEventEmitter tickEmitter, bgmEmitter;
    [SerializeField] private float charSpacing = 0.825f, scoreUpdateDuration = 3f;
    [SerializeField] private int digits = 8;
    private float displayedScore = 0f;
    private int targetScore = 0;

    private void Start()
    {
        StageStats latestStageStats = Launcher.Instance.GameStageStats.LastOrDefault();
        if (latestStageStats != null)
        {
            targetScore = scoreType != ScoreType.None
                ? latestStageStats.ScoreBreakdown[scoreType]
                : latestStageStats.TotalScore;

            if (scoreType == ScoreType.None)
                Launcher.Instance.SetupMenuTimer(15, false);
        }
    }

    private void FixedUpdate()
    {
        bool finishedCounting = displayedScore >= targetScore;
        if (scoreType == ScoreType.None)
        {
            Launcher.Instance.SetMusicStatus(true);
            if (tickEmitter) tickEmitter.gameObject.SetActive(!finishedCounting);
            if (bgmEmitter) bgmEmitter.gameObject.SetActive(finishedCounting);
            Launcher.Instance.TimerEnabled = finishedCounting;
        }

        if (!finishedCounting)
        {
            string monospaceTag = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>";
            displayedScore = Mathf.Min(displayedScore + Time.fixedDeltaTime * (targetScore / scoreUpdateDuration), targetScore);
            scoreDisplayFill.text = $"{monospaceTag}{(int)displayedScore}";
            scoreDisplayBack.text = $"{monospaceTag}{((int)displayedScore).ToString().PadLeft(digits, '0')}";
        }
    }
}