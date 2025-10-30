using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreLabel, scoreLabelBackground, multiplierLabel;
    [SerializeField] private float charSpacing = 0.825f, scoreUpdateRate = 10f;
    private float displayedScore = 0;

    [SerializeField] private GameObject healthBarObject;
    [SerializeField] private Image healthBarFill, healthBarBuffer;
    [SerializeField] private float healthBarBufferRate = .1f;

    private PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player = FindAnyObjectByType<PlayerController>())
        {
            healthBarObject.SetActive(player.MaxHealth > 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (player)
        {
            float targetFill = (float)player.Health / player.MaxHealth;
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
        }

        string monospaceTag = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>";
        displayedScore = Mathf.Min(displayedScore + Time.fixedDeltaTime * scoreUpdateRate, GameManager.Instance.score);
        scoreLabel.text = $"{monospaceTag}{(int)displayedScore,8}";
        scoreLabelBackground.text = $"{monospaceTag}{((int)displayedScore).ToString().PadLeft(8, '0')}";
        multiplierLabel.text = $"x{GameManager.Instance.Multiplier}";
    }
}
