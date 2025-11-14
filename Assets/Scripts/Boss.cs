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
}

public class Boss : Enemy
{
    [Header("Boss Properties")]
    [SerializeField] private string displayName;
    [SerializeField] private List<BossEvent> events;

    public string DisplayName { get { return displayName; } }
    public List<BossEvent> Events { get { return events; } }
}
