using UnityEngine;
using System.Collections.Generic;
public class Wave
{
    public float minDelay;
    public List<Enemy> enemies;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // [SerializeField] private float scrollSpeed = 2f;
    public int score;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
