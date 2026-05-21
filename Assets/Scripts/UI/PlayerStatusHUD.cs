using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusHUD : MonoBehaviour
{
    public PlayerController Player { get; set; }
    [SerializeField] private Image healthBar, healthBarFill, healthBarBuffer;
    [SerializeField] private Image scChargeBar;
    [SerializeField] private Color scChargingColor, scReadyColor;
    [SerializeField] private float healthBarBufferRate = .1f;
    [SerializeField][Range(0f, 1f)] private float criticalHealthThreshold = .2f;
    [SerializeField] private float criticalHealthEffectSpeed = 1;
    [SerializeField] private Gradient criticalHealthEffectGradient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBarFill.fillAmount = healthBarBuffer.fillAmount = 1f;
        scChargeBar.fillAmount = 0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        ProgressBarBufferUpdate();
        
        if (!Player)
        {
            // Obliterated
            healthBar.color = Color.red;
        }
        else if ((float)Player.NormalizedHealth <= criticalHealthThreshold)
        {
            // Critical damage
            healthBar.color = criticalHealthEffectGradient.Evaluate(Time.time * criticalHealthEffectSpeed % 1);
        }
        else
        {
            // Systems OK
            healthBar.color = Color.black;
        }

        // What I actually see
        if (Player)
        {
            // Supercore Charge Bar
            scChargeBar.color =
                Player.CurrentSupercoreChargePercent >= 1f
                ? scReadyColor
                : scChargingColor;


            scChargeBar.fillAmount = Player.CurrentSupercoreChargePercent;
        }


        // What mortal programmers see
        /*
        if (player)
        {
            // Supercore Charge Bar
            scChargeBar.fillAmount = player.CurrentSupercoreChargePercent;
            if (player.CurrentSupercoreChargePercent >= 1f)
            {
                scChargeBar.color = scReadyColor;
            }
            else
            {
                scChargeBar.color = scChargingColor;
            }
        }
        */
    }

    private void ProgressBarBufferUpdate()
    {
        float currentFill = healthBarBuffer.fillAmount;
        float targetFill = Player ? Player.NormalizedHealth : 0;

        if (targetFill < currentFill)
        {
            // Damage
            healthBarBuffer.color = Color.red;
            healthBarFill.fillAmount = Player.NormalizedHealth;
            healthBarBuffer.fillAmount = Mathf.Max(currentFill - Time.fixedUnscaledDeltaTime * healthBarBufferRate, targetFill);
        }
        else
        {
            // Heal
            currentFill = healthBarBuffer.fillAmount;
            healthBarBuffer.color = Color.green;
            healthBarBuffer.fillAmount = Player.NormalizedHealth;
            healthBarFill.fillAmount = Mathf.Min(currentFill + Time.fixedUnscaledDeltaTime * healthBarBufferRate, targetFill);
        }
    }
}
