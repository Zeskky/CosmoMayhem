using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class BossEvent
{
    [Range(0f, 1f)]
    [SerializeField] private float targetHealth = 0.5f;
    [SerializeField] private UnityEvent eventAction;

    public float TargetHealth { get { return targetHealth; } }
    public UnityEvent Action { get { return eventAction; } }

    public bool Triggered { get; set; }
}

public class Boss : Enemy
{
    [Header("Boss Properties")]
    [SerializeField] private string displayName;
    [SerializeField] private List<BossEvent> events;

    public string DisplayName { get { return displayName; } }
    public List<BossEvent> Events { get { return events; } }

    public bool IsAngry { get; private set; }

    public void GetAngry()
    {
        IsAngry = true;
        // GameManager.Instance.ShakeScreen(2.5f);
    }

    public override void Die()
    {
        GameManager.Instance.StopMusic();
        base.Die();
        GameManager.Instance.BossDefeated = true;
        Time.timeScale = .1f;
    }

    public override bool TakeDamage(int damage = 1)
    {
        Events.ForEach(
            ev => {
                if (!ev.Triggered && NormalizedHealth <= ev.TargetHealth)
                {
                    ev.Triggered = true;
                    ev.Action.Invoke();
                }
            }
        );
        return base.TakeDamage(damage);
    }

    public void SetShotRate(float rate = 1)
    {
        ShotRate = rate;
    }
}
