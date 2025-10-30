using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FMODUnity;
public class Wave
{
    public float minDelay;
    public List<Enemy> enemies;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private float scrollSpeed = 2f;
    public int score;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private Vector2 randomOffset;
    [SerializeField] private float spawnDelay = 1f, timeFreezeTransitionTime = .75f;
    [SerializeField] private StudioEventEmitter bgmEmitter;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaveSpawnCo());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(scrollSpeed * Time.deltaTime, 0);
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
        }

        Time.timeScale = 0;
        bgmEmitter.Stop();
        yield return new WaitForSecondsRealtime(1f);
        Debug.Log("OBLITERATED");
    }

    public IEnumerator WaveSpawnCo()
    {
        while (true)
        {
            Vector3 pos = new(
                spawnArea.position.x + Random.Range(-randomOffset.x, randomOffset.x),
                spawnArea.position.y + Random.Range(-randomOffset.y, randomOffset.y)
            );
            GameObject _ = Instantiate(enemyPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
