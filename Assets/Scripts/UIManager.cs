using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FMODUnity;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreLabel, scoreLabelBackground, multiplierLabel;
    [SerializeField] List<Color> multiplierColors;
    [SerializeField] Transform multiplierBar;
    [SerializeField] private float charSpacing = 0.825f, scoreUpdateRate = 10f;
    private float displayedScore = 0;

    [SerializeField] private Image healthBar, healthBarFill, healthBarBuffer;
    [SerializeField] private float healthBarBufferRate = .1f;
    [SerializeField] [Range(0f, 1f)] private float criticalHealthThreshold = .2f;
    [SerializeField] private float criticalHealthEffectSpeed = 1;
    [SerializeField] private Gradient criticalHealthEffectGradient;
    [SerializeField] private GameObject bossAlertOverlay;

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

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        float targetFill = player ? (float)player.Health / player.MaxHealth : 0;
        float currentFill = healthBarBuffer.fillAmount;

        if (targetFill < currentFill)
        {
            // Damage
            healthBarBuffer.color = Color.red;
            healthBarFill.fillAmount = targetFill;
            healthBarBuffer.fillAmount = Mathf.Max(currentFill - Time.fixedDeltaTime * healthBarBufferRate, targetFill);
        }
        else
        {
            // Heal
            healthBarBuffer.color = Color.green;
            healthBarBuffer.fillAmount = targetFill;
            healthBarFill.fillAmount = Mathf.Min(currentFill + Time.fixedDeltaTime * healthBarBufferRate, targetFill);
        }

        if (!player)
        {
            // Obliterated
            healthBar.color = Color.red;
        }
        else if (targetFill <= criticalHealthThreshold)
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
        displayedScore = Mathf.Min(displayedScore + Time.fixedDeltaTime * scoreUpdateRate, GameManager.Instance.score);
        scoreLabel.text = $"{monospaceTag}{(int)displayedScore,8}";
        scoreLabelBackground.text = $"{monospaceTag}{((int)displayedScore).ToString().PadLeft(8, '0')}";
        UpdateMultiplier();

        if (GameManager.Instance.CurrentStageState == StageState.Boss)
        {
            didBossAlert = true;
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
