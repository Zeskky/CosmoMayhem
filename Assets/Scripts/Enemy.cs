using FMODUnity;
using UnityEngine;

public enum MovementBehaviour
{
    GoForward,
    Halt,
    ChasePlayer,
}

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Damageable
{
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private GameObject floatingHealthBarPrefab;

    [SerializeField] private int scoreValue = 10;
    [SerializeField] private MovementBehaviour movementBehaviour;
    [SerializeField] private bool doWarp = true;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float minSpeedModifier, maxSpeedModifier, chaseSpeed = 2f;
    [SerializeField] private int baseDamage = 2;
    public int BaseDamage { get { return baseDamage; } }

    [Header("Shooting Behaviour")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private float minShotDelay = 2f, maxShotDelay = 3f, shotSpeed = 5f;
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private Transform cannonPoint;
    private float shotTimer, nextShotDelay;

    private float movementTimer;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private StudioEventEmitter damageTakenEmitter;
    private GameObject currentFloatingHealthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        nextShotDelay = Random.Range(minShotDelay, maxShotDelay);
        
        startPosition = transform.position;
        rb.linearVelocityX = -Random.Range(minSpeedModifier, maxSpeedModifier);
        damageTakenEmitter = GetComponent<StudioEventEmitter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canShoot)
        {
            if ((shotTimer += Time.deltaTime) >= nextShotDelay)
            {
                shotTimer -= nextShotDelay;
                nextShotDelay = Random.Range(minShotDelay, maxShotDelay);
                GenerateShot();
            }
        }

        switch (movementBehaviour)
        {
            case MovementBehaviour.GoForward:
                // Enemy will keep going forward
                // Avoid using this value with 'doWarp' set to false, unless it's an optional enemy
                break;
            case MovementBehaviour.Halt:
                // Enemy will halt upon reaching target horizontal position (x)
                Vector3 actualTargetPosition = Camera.main.ViewportToWorldPoint(targetPosition);
                if (transform.position.x < actualTargetPosition.x)
                {
                    transform.parent = Camera.main.transform;
                }
                break;
            case MovementBehaviour.ChasePlayer:
                // Enemy will chase player's ship
                PlayerController player;
                if (player = FindAnyObjectByType<PlayerController>())
                    rb.linearVelocityY = (player.transform.position.y - transform.position.y) * chaseSpeed;
                break;
        }

        if (doWarp)
        {
            // Enemy will warp to the initial spawn position
            Vector3 warpPosition = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f)) + Vector3.left;
            if (transform.position.x < warpPosition.x)
            {
                transform.position = new(GameManager.Instance.GetSpawnAreaPosition().x, transform.position.y);
            }
        }
    }

    private void GenerateShot()
    {
        Projectile newShot = Instantiate(shotPrefab, cannonPoint.position, Quaternion.identity).GetComponent<Projectile>();
        if (newShot)
        {
            newShot.damage = baseDamage;
            newShot.startVelocity = new(-shotSpeed, 0);
        }
    }

    public override void Die()
    {
        // TODO: show explosion effect
        GameManager.Instance.score += scoreValue * GameManager.Instance.Multiplier;
        base.Die();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Projectile projectile = collision.GetComponent<Projectile>();
        if (!collision.CompareTag(gameObject.tag) && projectile)
        {
            // Take damage from any non-enemy projectile
            TakeDamage(projectile.damage);
            Destroy(collision.gameObject);
        }
    }

    public override void TakeDamage(int damage = 1)
    {
        base.TakeDamage(damage);
        if (Health <= 0)
        {
            Die();
        }
        else
        {
            // Still alive
            damageTakenEmitter.Play();
            ShowFloatingHealthBar();
        }
    }

    public override void HealDamage(int damage = 1)
    {
        base.HealDamage(damage);
    }

    private void ShowFloatingHealthBar(float lifetime = 1f)
    {
        if (currentFloatingHealthBar)
        {
            // Destroy any existing floating bars
            Destroy(currentFloatingHealthBar);
        }

        currentFloatingHealthBar = Instantiate(floatingHealthBarPrefab, transform);
        currentFloatingHealthBar.GetComponent<FloatingHealthBar>().HealthBarFill = (float)Health / maxHealth;
        Destroy(currentFloatingHealthBar, lifetime);
    }

}
