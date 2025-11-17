using UnityEngine;

public enum PickupType
{
    ScoreBonus,
    RepairKit,
    SupercoreBooster,
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Pickup : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private int pickupValue = 20;
    [SerializeField] private float movementSpeed = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /*
    void Start()
    {
        
    }
    */
    // Update is called once per frame
    void Update()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(-movementSpeed, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GetPickup(collision.GetComponent<PlayerController>());
    }

    private void GetPickup(PlayerController player)
    {
        if (!player) return;
        switch (pickupType)
        {
            case PickupType.ScoreBonus:
                GameManager.Instance.CurrentStageStats.ScoreBreakdown[ScoreType.Pickup] += pickupValue;
                break;
            case PickupType.RepairKit:
                // Calculate essential values
                float normalizedDamage = 1 - player.NormalizedHealth;
                float normalizedHealing = pickupValue / 100f;

                // Procede to repair
                player.HealDamage(normalizedHealing);
                
                // Grant an over-repair score bonus (if applicable)
                float normalizedOverRepair = Mathf.Clamp01(normalizedHealing - normalizedDamage);
                if (normalizedOverRepair > 0)
                {
                    int overRepairBonus = (int)(GameManager.Instance.OverRepairScore * normalizedOverRepair * 100);
                    GameManager.Instance.CurrentStageStats.ScoreBreakdown[ScoreType.Pickup] += overRepairBonus;
                }
                break;
            case PickupType.SupercoreBooster:
                player.CurrentSupercoreCharge += pickupValue;
                break;
        }

        Destroy(gameObject);
    }
}
