using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;
using System.Linq;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreLabel, scoreLabelBackground, multiplierLabel;
    [SerializeField] private List<Color> multiplierColors;
    [SerializeField] Transform multiplierBar;
    [SerializeField] private float charSpacing = 0.825f, scoreUpdateRate = 10f;
    private float displayedScore = 0;

    [SerializeField] private float healthBarBufferRate = .1f;
    /*
    [SerializeField] private Image healthBar, healthBarFill, healthBarBuffer;
    [SerializeField] private Image scChargeBar;
    [SerializeField] private Color scChargingColor, scReadyColor;
    [SerializeField] [Range(0f, 1f)] private float criticalHealthThreshold = .2f;
    [SerializeField] private float criticalHealthEffectSpeed = 1;
    */
    [SerializeField] private Gradient criticalHealthEffectGradient;
    public Gradient CriticalHealthGradient { get => criticalHealthEffectGradient; }
    [SerializeField] private Image criticalHealthBorder;

    [SerializeField] private float healthBorderFadeTime = 1f;
    private float healthBorderTimer = 0;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private GameObject playerStatusPrefab;
    [SerializeField] private Transform playerStatusContainer;

    [Header("Boss UI Elements")]
    [SerializeField] private GameObject bossAlertOverlay;
    [SerializeField] private GameObject bossHealthBar;
    [SerializeField] private Image bossHealthBarFill, bossHealthBarBuffer;
    [SerializeField] private Color bossNormalColor, bossAngryColor;
    [SerializeField] private TMP_Text bossDisplayNameLabel;

    private bool didBossAlert = false;
    // private PlayerController player;
    private List<PlayerController> players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        displayedScore = Launcher.Instance.GetCurrentGameScore();
        players = FindObjectsByType<PlayerController>(0).ToList();
        foreach (PlayerController pc in players)
        {
            GameObject pHUD = Instantiate(playerStatusPrefab, playerStatusContainer);
            pHUD.GetComponent<PlayerStatusHUD>().Player = pc;
        }
        /*
        if (player = FindAnyObjectByType<PlayerController>())
        {
            healthBar.gameObject.SetActive(player.MaxHealth > 1);
        }
        */
    }

    private void ProgressBarBufferUpdate(float targetFill, Image barFill, Image barBuffer, float rateScale = 1f)
    {
        float currentFill = barBuffer.fillAmount;

        if (targetFill < currentFill)
        {
            // Damage
            barBuffer.color = Color.red;
            barFill.fillAmount = targetFill;
            barBuffer.fillAmount = Mathf.Max(currentFill - Time.fixedDeltaTime * healthBarBufferRate * rateScale, targetFill);
        }
        else
        {
            // Heal
            currentFill = barFill.fillAmount;
            barBuffer.color = Color.green;
            barBuffer.fillAmount = targetFill;
            barFill.fillAmount = Mathf.Min(currentFill + Time.fixedDeltaTime * healthBarBufferRate * rateScale, targetFill);
        }
    }

    private void FixedUpdate()
    {
        /*
        ProgressBarBufferUpdate(
            targetFill: player ? player.NormalizedHealth : 0,
            barFill: healthBarFill,
            barBuffer: healthBarBuffer
        );
        */

        // Intro Overlay
        if (introAnimator && !GameManager.Instance.GameStarted)
        {
            AnimatorStateInfo asi = introAnimator.GetCurrentAnimatorStateInfo(0);
            GameManager.Instance.GameStarted = asi.IsTag("Out") && asi.normalizedTime >= 1f;
        }

        criticalHealthBorder.color = new(1, 1, 1, healthBorderTimer / healthBorderFadeTime);

        string monospaceTag = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>";
        displayedScore = Mathf.Min(
            displayedScore + Time.fixedDeltaTime * scoreUpdateRate,
            Launcher.Instance.GetCurrentGameScore()
            + GameManager.Instance.CurrentStageStats.TotalScore
            );
        scoreLabel.text = $"{monospaceTag}{(int)displayedScore,8}";
        scoreLabelBackground.text = $"{monospaceTag}{((int)displayedScore).ToString().PadLeft(8, '0')}";
        UpdateMultiplier();

        Boss currentBoss;
        bossHealthBar.SetActive(currentBoss = GameManager.Instance.CurrentBoss);

        if (currentBoss)
        {
            // Boss UI update
            bossHealthBarFill.color = currentBoss.IsAngry ? bossAngryColor : bossNormalColor;
            ProgressBarBufferUpdate(
                targetFill: currentBoss.NormalizedHealth,
                barFill: bossHealthBarFill,
                barBuffer: bossHealthBarBuffer,
                rateScale: 0.1f
            );
            bossDisplayNameLabel.text = currentBoss.DisplayName;
        }

        if (GameManager.Instance.CurrentStagePhase == StagePhase.Boss && !didBossAlert)
        {
            bossAlertOverlay.SetActive(didBossAlert = true);
            Animator bossAlertAnimator = bossAlertOverlay.GetComponent<Animator>();
            if (bossAlertAnimator)
            {
                bossAlertAnimator.SetTrigger("On");
            }

            StudioEventEmitter alertEmitter = bossAlertOverlay.GetComponent<StudioEventEmitter>();
            if (alertEmitter)
            {
                if (alertEmitter.EventPlayTrigger == EmitterGameEvent.None)
                {
                    alertEmitter.Play();
                }
            }
        }
    }

    private void UpdateMultiplier()
    {
        int currentMultiplier = GameManager.Instance.Multiplier;
        int nextMultiplier = currentMultiplier + 1;

        Color currentColor = Color.gray, nextColor = currentColor;
        if (currentMultiplier - 1 < multiplierColors.Count)
            nextColor = currentColor = multiplierColors[currentMultiplier - 1];

        if (nextMultiplier - 1 < multiplierColors.Count)
            nextColor = multiplierColors[nextMultiplier - 1];

        multiplierLabel.text = $"x{currentMultiplier}";
        multiplierLabel.color = currentColor;

        // Multiplier Segments
        for (int i = 0; i < multiplierBar.childCount; i++)
        {
            Image barSegment;
            if (barSegment = multiplierBar.GetChild(i).GetComponent<Image>())
            {
                barSegment.color = 
                    GameManager.Instance.MultiplierProgress > i 
                    ? nextColor
                    : currentColor;
            }
            
        }
    }
}
