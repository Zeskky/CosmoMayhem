using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageSettings", menuName = "CosmoMayhem/Stage Settings")]
public class StageSettings : ScriptableObject
{
    [Tooltip("The enemy waves that will appear through the stage.")]
    [SerializeField] private List<Wave> waves;

    // Make sure the list is read-only
    public List<Wave> Waves { get => new(waves); } 

    [Tooltip("How much score does award for the player's ship integrity?")]
    [SerializeField] private int shipMaxBonus = 1000;
    [Tooltip("How much score does award for the player's clear time?")]
    [SerializeField] private int timeMaxBonus = 3000;
    [Tooltip("How much time has the player before the timeMaxBonus runs out? (Expressed as X times the stage's maximum length, in seconds)")]
    [SerializeField] private float timeBonusDecayRate = 1.6f;
    [SerializeField] private float stageMeanTimeScale = 1.2f;

    [SerializeField] private float bossSpawnDelay;

    public int ShipMaxBonus { get => shipMaxBonus; }
    public int TimeMaxBonus { get => timeMaxBonus; }
    public float TimeBonusDecayRate { get => timeBonusDecayRate; }
    public float StageMeanTimeScale { get => stageMeanTimeScale; }
    public float BossSpawnDelay { get => bossSpawnDelay; }

    public float MeanClearTime
    {
        get
        {
            float clearTime = 0;
            foreach (Wave wave in waves) clearTime += wave.maxDelay;
            return clearTime * stageMeanTimeScale;
        }
    }
}
