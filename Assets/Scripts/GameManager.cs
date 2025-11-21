using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum StagePhase
{
    Regular,
    Boss,
    Bonus,
}

public enum StageResult
{
    Unfinished,
    Cleared,
    Failed,
}

[System.Serializable]
public class Wave
{
    public float maxDelay;
    public GameObject wavePrefab;
    public bool hasBoss;
}

public enum ScoreType
{
    None,
    Enemy,
    Pickup,
    Ship
}

[System.Serializable]
public class StageStats
{
    // public bool Cleared { get; set; }
    public StageResult Result { get; set; }
    public Dictionary<ScoreType, int> scoreBreakdown = new();
    public Dictionary<ScoreType, int> ScoreBreakdown { get { return scoreBreakdown; } }

    public int TotalScore
    {
        get { return ScoreBreakdown.Values.Sum(); }
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private float scrollSpeed = 2f;
    [Tooltip("Score awarded for every 1% of over-repairing done on the player's ship.")]
    [SerializeField] private int overRepairScore = 10;
    [Tooltip("How much score does award for the player's ship integrity?")]
    [SerializeField] private int shipMaxBonus = 1000;
    [Tooltip("How much health must have the player's ship to award by its integrity?")]
    [Range(0f, 1f)]
    [SerializeField] private float normalizedHealthForBonus = 0.2f;
    [SerializeField] private Vector2 currentShakeValue;

    public int OverRepairScore { get { return overRepairScore; } }

    [Header("Stage Statistics")]
    [SerializeField] private StageStats currentStageStats = new();
    public StageStats CurrentStageStats { get { return currentStageStats; } }

    // public int score;

    [SerializeField] private int maxMultiplier = 4, comboPerMultiplier = 5;
    private int multiplier = 1;
    public int Multiplier
    {
        get {  return multiplier; }
    }

    private int combo = 0;
    public int Combo
    {
        get { return combo; }
        set { combo = value; }
    }

    private int multiplierProgress = 0;
    public int MultiplierProgress
    {
        get { return multiplierProgress; }
        set 
        {
            if (value >= multiplierProgress)
            {
                if (multiplier < maxMultiplier)
                {
                    // Multiplier should only increase if not maxed out
                    multiplierProgress = value;
                    if (multiplierProgress >= comboPerMultiplier)
                    {
                        // Increase multiplier
                        multiplier += multiplierProgress / comboPerMultiplier;
                        multiplierProgress = comboPerMultiplier - multiplierProgress + 1;
                    }
                }
            }
            else
            {
                if (multiplierProgress > 0)
                {
                    // Reset multiplier progress
                    multiplierProgress = 0;
                }
                else
                {
                    // Decrease multiplier
                    multiplier--;
                }
            }

            multiplier = Mathf.Clamp(multiplier, 1, maxMultiplier);
        }
    }

    //[SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject gameOverMessage;
    [SerializeField] private Transform spawnArea, mainCameraGroup;
    [SerializeField] private Vector2 randomOffset;
    [SerializeField] private float /*spawnDelay = 1f,*/ timeFreezeTransitionTime = .75f;
    [SerializeField] private StudioEventEmitter bgmEmitter;

    [Header("Wave Settings")]
    [SerializeField] private List<Wave> waves;
    [SerializeField] private float bossSpawnDelay;
    private float currentWaveTimer;

    public List<Wave> Waves { get {  return waves; } }
    private int currentWave = 0;
    private List<GameObject> waveEnemies = new();
    
    public Boss CurrentBoss { get; private set; }
    public bool BossDefeated { get; set; }

    public StagePhase CurrentStagePhase { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private GameObject missionCompletePrefab;
    [SerializeField] private GameObject missionFailedPrefab;

    private void Awake()
    {
        Instance = this;
        gameOverMessage.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentStagePhase = StagePhase.Regular;
        SetupStageStats();
        // StartCoroutine(WaveSpawnCo());
    }
    
    private void SetupStageStats()
    {
        currentStageStats.ScoreBreakdown[ScoreType.Enemy] = 
        currentStageStats.ScoreBreakdown[ScoreType.Pickup] = 
        currentStageStats.ScoreBreakdown[ScoreType.Ship] = 0;
    }

    // Update is called once per frame
    void Update()
    {
        currentWaveTimer += Time.deltaTime;
        transform.position += new Vector3(scrollSpeed * Time.deltaTime, 0);

        SpawnNextWave();
    }

    private void FixedUpdate()
    {
        // print(currentStageStats.Result);
        if (currentStageStats.Result != StageResult.Failed)
        {
            Time.timeScale = Mathf.Clamp01(Time.timeScale + Time.fixedDeltaTime / (timeFreezeTransitionTime * 1.5f));
        }

        if (IsLastWave() && waveEnemies.Count == 0 && BossDefeated)
        {
            StartCoroutine(EndMission(true));
        }

        if (bgmEmitter.IsPlaying())
        {
            RuntimeManager.StudioSystem.setParameterByName("Music Pitch", Time.timeScale);

            bgmEmitter.SetParameter("Stage State", (int)CurrentStagePhase);
        }
    }

    public void DoGameOverSequence()
    {
        StartCoroutine(GameOverSequenceCo());
    }

    private IEnumerator GameOverSequenceCo()
    {
        currentStageStats.Result = StageResult.Failed;
        while (Time.timeScale > 0.05f)
        {
            yield return new WaitForFixedUpdate();
            Time.timeScale -= Time.fixedDeltaTime / timeFreezeTransitionTime;
            // print(Time.timeScale);
        }

        Time.timeScale = 0;
        StopMusic();
        gameOverMessage.SetActive(true);
        // Debug.Log("OBLITERATED");
    }

    public bool IsCurrentWaveCleared()
    {
        return waveEnemies.Count == 0;
    }
    
    public bool IsLastWave()
    {
        return currentWave >= waves.Count;
    }

    public void RemoveMissingWaveEnemies()
    {
        // Deletes all missing or non-enemy references
        int c = waveEnemies.RemoveAll(enemy => enemy == null || !enemy.CompareTag("Enemy"));
    }

    public void SpawnNextWave()
    {
        RemoveMissingWaveEnemies();
        if (currentWave < 0 || IsLastWave())
        {
            return;
        }

        Wave nextWave = waves[currentWave];
        // print(nextWave.maxDelay - currentWaveTimer);
        if ((currentWaveTimer >= nextWave.maxDelay && !nextWave.hasBoss) || (IsCurrentWaveCleared() && currentWave > 0))
        {
            if (nextWave.hasBoss)
            {
                if (CurrentStagePhase != StagePhase.Boss)
                {
                    StartCoroutine(DoBossSequenceCo(nextWave));
                }
            }
            else
            {
                SpawnWave(nextWave);
            }

            currentWave++;
        }
    }

    public IEnumerator EndMission(bool success)
    {
        if (currentStageStats.Result != StageResult.Unfinished) yield break;
        currentStageStats.Result = success ? StageResult.Cleared : StageResult.Failed;

        if (success)
        {
            foreach (GameObject enemyGO in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                Projectile p = enemyGO.GetComponent<Projectile>();
                if (p) Destroy(enemyGO);
            }

            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player)
            {
                float n = normalizedHealthForBonus;
                currentStageStats.ScoreBreakdown[ScoreType.Ship] = (int)(shipMaxBonus 
                    * Mathf.Max(0, (player.NormalizedHealth - n) / (1 - n)));
            }
        }

        StopMusic();
        yield return new WaitForSecondsRealtime(timeFreezeTransitionTime * 3f);

        // Disable all PlayerInput instances
        foreach (PlayerInput pi in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            pi.enabled = false;

        Launcher.Instance.SendEndStage(currentStageStats);

        /*
        Animator transitionAnim = null;
        if (missionCompletePrefab)
        {
            if (success)
            {
                GameObject transitionInstance = Instantiate(missionCompletePrefab);
                transitionAnim = transitionInstance.GetComponent<Animator>();
                DontDestroyOnLoad(transitionInstance);
            }
        }

        yield return new WaitForSecondsRealtime(transitionStayTime);
        if (transitionAnim)
        {
            transitionAnim.SetTrigger("End");
            yield return SceneManager.LoadSceneAsync("Evaluation");
        }
        */
    }

    private void SpawnWave(Wave wave)
    {
        if (!wave.wavePrefab)
            return;
        // Standard wave
        currentWaveTimer -= wave.maxDelay;
        GameObject waveInstance = Instantiate(wave.wavePrefab, spawnArea.position, Quaternion.identity);
        while (waveInstance.transform.childCount > 0)
        {
            // Store the wave instance's children
            Transform t = waveInstance.transform.GetChild(0);
            waveEnemies.Add(t.gameObject);
            t.parent = transform;
        }

        waveEnemies.ForEach(e => e.transform.parent = null);

        if (!CurrentBoss && wave.hasBoss)
        {
            CurrentBoss = FindFirstObjectByType<Boss>();
        }

        Destroy(waveInstance);
    }

    private IEnumerator DoBossSequenceCo(Wave wave)
    {
        CurrentStagePhase = StagePhase.Boss;
        yield return new WaitForSecondsRealtime(bossSpawnDelay);
        SpawnWave(wave);
    }

    public IEnumerator WaveSpawnCo()
    {
        /*
        while (true)
        {
            Vector3 pos = new(
                spawnArea.position.x + Random.Range(-randomOffset.x, randomOffset.x),
                spawnArea.position.y + Random.Range(-randomOffset.y, randomOffset.y)
            );
            GameObject _ = Instantiate(enemyPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }
        */
        yield return null;
    }

    private void LateUpdate()
    {
        Vector2 shakeOffset = new(
            Random.Range(-1f, 1f) * currentShakeValue.x, 
            Random.Range(-1f, 1f) * currentShakeValue.y
        );

        (mainCameraGroup.parent = Camera.main.transform).position = new Vector3(
            transform.position.x + shakeOffset.x, 
            transform.position.y + shakeOffset.y, 
            -10
        );
    }

    public Vector2 GetSpawnAreaPosition()
    {
        return spawnArea.position;
    }

    public void ShakeScreen(float magnitude)
    {
        StartCoroutine(ShakeScreen(new Vector2(magnitude, magnitude)));
    }

    public IEnumerator ShakeScreen(Vector2 shakeMagnitude, float duration = 1f, bool decay = true)
    {
        if (shakeMagnitude.magnitude > 0f && duration > 0f)
        {
            float timer = 0;
            while (timer < duration)
            {
                currentShakeValue = decay ? shakeMagnitude * (1 - timer / duration) : shakeMagnitude;

                yield return new WaitForEndOfFrame();
                timer += Time.deltaTime;
            }

            currentShakeValue = Vector2.zero;
        }
    }

    public void StopMusic()
    {
        bgmEmitter.Stop();
    }
}