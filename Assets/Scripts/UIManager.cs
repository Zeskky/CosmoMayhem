using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreLabel, scoreLabelBackground, multiplierLabel;
    [SerializeField] private List<Color> multiplierColors;
    [SerializeField] Transform multiplierBar;
    [SerializeField] private float charSpacing = 0.825f, scoreUpdateRate = 10f;
    private float displayedScore = 0;

    [SerializeField] private Image healthBar, healthBarFill, healthBarBuffer;
    [SerializeField] private float healthBarBufferRate = .1f;
    [SerializeField] [Range(0f, 1f)] private float criticalHealthThreshold = .2f;
    [SerializeField] private float criticalHealthEffectSpeed = 1;
    [SerializeField] private Gradient criticalHealthEffectGradient;
    [Header("Boss UI Elements")]
    [SerializeField] private GameObject bossAlertOverlay, bossHealthBar;
    [SerializeField] private Image bossHealthBarFill, bossHealthBarBuffer;
    [SerializeField] private TMP_Text bossDisplayNameLabel;

    private bool didBossAlert = false;
    private PlayerController player;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player = FindAnyObjectByType<PlayerController>())
        {
            healthBar.gameObject.SetActive(player.MaxHealth > 1);
        }
    }

    private void ProgressBarBufferUpdate(float targetFill, Image barFill, Image barBuffer)
    {
        float currentFill = barBuffer.fillAmount;

        if (targetFill < currentFill)
        {
            // Damage
            barBuffer.color = Color.red;
            barFill.fillAmount = targetFill;
            barBuffer.fillAmount = Mathf.Max(currentFill - Time.fixedDeltaTime * healthBarBufferRate, targetFill);
        }
        else
        {
            // Heal
            currentFill = barFill.fillAmount;
            barBuffer.color = Color.green;
            barBuffer.fillAmount = targetFill;
            barFill.fillAmount = Mathf.Min(currentFill + Time.fixedDeltaTime * healthBarBufferRate, targetFill);
        }
    }

    private void FixedUpdate()
    {
        ProgressBarBufferUpdate(
            targetFill: player ? player.NormalizedHealth : 0,
            barFill: healthBarFill,
            barBuffer: healthBarBuffer
        );

        if (!player)
        {
            // Obliterated
            healthBar.color = Color.red;
        }
        else if ((float)player.NormalizedHealth <= criticalHealthThreshold)
        {
            // Critical damage
            healthBar.color = criticalHealthEffectGradient.Evaluate((Time.time * criticalHealthEffectSpeed) % 1);
        }
        else
        {
            // Systems OK
            healthBar.color = Color.black;
        }

        string monospaceTag = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>";
        displayedScore = Mathf.Min(displayedScore + Time.fixedDeltaTime * scoreUpdateRate, GameManager.Instance.CurrentStageStats.TotalScore);
        scoreLabel.text = $"{monospaceTag}{(int)displayedScore,8}";
        scoreLabelBackground.text = $"{monospaceTag}{((int)displayedScore).ToString().PadLeft(8, '0')}";
        UpdateMultiplier();

        Boss currentBoss;
        bossHealthBar.SetActive(currentBoss = GameManager.Instance.CurrentBoss);

        if (currentBoss)
        {
            // Boss UI update
            ProgressBarBufferUpdate(
                targetFill: currentBoss.NormalizedHealth,
                barFill: bossHealthBarFill,
                barBuffer: bossHealthBarBuffer
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
