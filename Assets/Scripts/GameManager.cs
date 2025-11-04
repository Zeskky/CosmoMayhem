using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FMODUnity;

[System.Serializable]
public class Wave
{
    public float maxDelay;
    public GameObject wavePrefab;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private float scrollSpeed = 2f;
    public int score;

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
        set 
        { 
            combo = value;
            multiplier = Mathf.Clamp((combo / comboPerMultiplier) + 1, 1, maxMultiplier);
        }
    }

    public int MultiplierProgress
    {
        get { return combo % comboPerMultiplier; }
    }

    //[SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject gameOverMessage;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private Vector2 randomOffset;
    [SerializeField] private float /*spawnDelay = 1f,*/ timeFreezeTransitionTime = .75f;
    [SerializeField] private StudioEventEmitter bgmEmitter;

    [Header("Wave Settings")]
    [SerializeField] private List<Wave> waves;
    [SerializeField] private float currentWaveTimer;

    public List<Wave> Waves { get {  return waves; } }
    private int currentWave = 0;
    private List<GameObject> waveEnemies = new();

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameOverMessage.SetActive(false);
        // StartCoroutine(WaveSpawnCo());
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
    
    public void RemoveMissingWaveEnemies()
    {
        int c = waveEnemies.RemoveAll(enemy => enemy == null);
        /*
        if (c > 0)
        {
            print(c);
        }
        */
    }

    public void SpawnNextWave()
    {
        if (currentWave < 0 || currentWave > waves.Count - 1)
            return;

        Wave nextWave = waves[currentWave];
        if (currentWaveTimer >= nextWave.maxDelay || (IsCurrentWaveCleared() && currentWave > 0))
        {
            SpawnWave(nextWave);
            currentWave++;
        }

        RemoveMissingWaveEnemies();
    }

    private void SpawnWave(Wave wave)
    {
        currentWaveTimer -= wave.maxDelay;
        GameObject waveInstance = Instantiate(wave.wavePrefab, spawnArea.position, Quaternion.identity);
        while (waveInstance.transform.childCount > 0)
        {
            // Store the wave instance's children
            Transform t = waveInstance.transform.GetChild(0);
            waveEnemies.Add(t.gameObject);
            t.parent = transform;
        }

        Destroy(waveInstance);
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
