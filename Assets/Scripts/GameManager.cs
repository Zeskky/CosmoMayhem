using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FMODUnity;
using UnityEngine.SceneManagement;
using System.Linq;

public enum StagePhase
{
    Regular,
    Boss,
    Bonus,
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
    public bool Cleared { get; set; }
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
                        multiplier = Mathf.Clamp(multiplier + 1, 1, maxMultiplier);
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
        }
    }

    //[SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject gameOverMessage;
    [SerializeField] private Transform spawnArea;
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

    public StagePhase CurrentStagePhase { get; private set; }

    [Header("Transition Settings")]
    [SerializeField] private GameObject missionCompletePrefab;
    [SerializeField] private GameObject missionFailedPrefab;
    [SerializeField] private float transitionStayTime = 5f;

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
        while (Time.timeScale > 0.05f)
        {
            yield return new WaitForFixedUpdate();
            Time.timeScale -= Time.fixedDeltaTime / timeFreezeTransitionTime;
            // print(Time.timeScale);
        }

        Time.timeScale = 0;
        bgmEmitter.Stop();
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
        if (currentWave < 0 || IsLastWave())
        {
            if (IsLastWave() && waveEnemies.Count == 0)
            {
                StartCoroutine(EndMission(true));
            }

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
                currentWave++;
            }
        }

        RemoveMissingWaveEnemies();
    }

    public IEnumerator EndMission(bool success)
    {
        bgmEmitter.Stop();
        Animator transitionAnim = null;

        currentStageStats.Cleared = success;
        Launcher.Instance.GameStageStats.Add(currentStageStats);

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
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    public Vector2 GetSpawnAreaPosition()
    {
        return spawnArea.position;
    }
}
