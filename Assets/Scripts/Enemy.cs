using FMODUnity;
using UnityEngine;

public enum MovementBehaviour
{
    GoForward,
    FacePlayer,
    Wavy,
    FollowPlayerY,
}

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : Damageable
{
    [SerializeField] private GameObject floatingHealthBarPrefab;

    [SerializeField] private int scoreValue = 10;
    [SerializeField] private MovementBehaviour movementBehaviour;
    [SerializeField] private bool doWarp = true, doHalt = false, inactiveUntilHalted = false;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float minSpeedModifier, maxSpeedModifier/*, maxVelocity = 5f*/;
    [SerializeField] private int baseDamage = 2;

    public int BaseDamage { get { return baseDamage; } }

    [Header("Shooting Behaviour")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private float minShotDelay = 2f, maxShotDelay = 3f, shotSpeed = 5f;
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private Transform cannonPoint;
    private float shotTimer, nextShotDelay;

    private float movementTimer, movementSpeed;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private StudioEventEmitter damageTakenEmitter;
    private GameObject currentFloatingHealthBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemySetup();
    }

    public virtual void EnemySetup()
    {
        SetupHealth();
        rb = GetComponent<Rigidbody2D>();
        nextShotDelay = Random.Range(minShotDelay, maxShotDelay);

        startPosition = transform.position;
        movementSpeed = Random.Range(minSpeedModifier, maxSpeedModifier);
        damageTakenEmitter = GetComponent<StudioEventEmitter>();
    }

    // Update is called once per frame
    void Update()
    {
        movementTimer += Time.deltaTime;
        PlayerController player = FindAnyObjectByType<PlayerController>();

        Vector2 moveDir = rb.linearVelocity.normalized;
        if (moveDir == Vector2.zero)
            moveDir = Vector2.left;

        // print(rb.linearVelocity);

        switch (movementBehaviour)
        {
            case MovementBehaviour.GoForward:
                // Enemy will keep going forward
                // Avoid using this value with 'doWarp' set to false, unless it's an optional enemy
                break;
            case MovementBehaviour.FacePlayer:
                // Enemy will face towards the player ship
                if (player)
                    if (transform.position.x > player.transform.position.x)
                    {
                        moveDir = (player.transform.position - transform.position).normalized;
                        transform.right = moveDir;
                    }
                break;
            case MovementBehaviour.Wavy:
                // Enemy will make a sine-like movement
                float s = 4 * transform.localScale.magnitude;
                float t = s * movementTimer % s;
                moveDir.y = movementTimer % 2 >= 1
                    ?  t - (s / 2) // Odd
                    : -t + (s / 2); // Even
                break;
            case MovementBehaviour.FollowPlayerY:
                // Enemy will follow the player ship vertically (y)
                if (player)
                {
                    float yDistance = (transform.position - transform.position).normalized.y;
                    moveDir.y = -yDistance;
                }
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

        if (doHalt)
        {
            // Enemy will halt upon reaching target horizontal position (x)
            Vector3 actualTargetPosition = Camera.main.ViewportToWorldPoint(targetPosition);
            
            if (transform.position.x < actualTargetPosition.x)
            {
                transform.parent = Camera.main.transform;
                movementSpeed = 0;

                if (canShoot)
                {
                    if ((shotTimer += Time.deltaTime) >= nextShotDelay)
                    {
                        shotTimer -= nextShotDelay;
                        nextShotDelay = Random.Range(minShotDelay, maxShotDelay);
                        GenerateShot();
                    }
                }
            }
            else
            {
                if (inactiveUntilHalted)
                {
                    // Enemy will be inactive until has reached the halt point
                    shotTimer = 0;
                    immuneTimer = damageImmunityTime * 2f;
                }
            }
        }

        // Standard linear movement
        rb.linearVelocity = moveDir * movementSpeed;

    }

    private void FixedUpdate()
    {
        ImmuneUpdate();
    }

    private void GenerateShot()
    {
        GameObject newShotInstance = Instantiate(shotPrefab, cannonPoint.position, cannonPoint.rotation);

        if (newShotInstance.GetComponent<Projectile>())
        {
            Projectile projectile = newShotInstance.GetComponent<Projectile>();
            projectile.damage = baseDamage;
            projectile.startVelocity = shotSpeed;
        }
        else if (newShotInstance.GetComponent<ProjectileBurst>())
        {
            ProjectileBurst burst = newShotInstance.GetComponent<ProjectileBurst>();
            burst.projectileDamage = baseDamage;
        }
    }

    public override void Die()
    {
        // TODO: show explosion effect
        GameManager.Instance.CurrentStageStats.ScoreBreakdown[ScoreType.Enemy] += scoreValue * GameManager.Instance.Multiplier;
        GameManager.Instance.Combo++;
        GameManager.Instance.MultiplierProgress++;
        base.Die();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Projectile projectile = collision.GetComponent<Projectile>();
        if (!collision.CompareTag(gameObject.tag) && projectile)
        {
            rb.AddForce(
                projectile.GetComponent<Rigidbody2D>().linearVelocity * (projectile.damage * .02f),
                ForceMode2D.Impulse
            );
            // Take damage from any non-enemy projectile
            if (TakeDamage(projectile.damage))
            {
                projectile.DestroyProjectile();
            }
        }
    }

    public override bool TakeDamage(int damage = 1)
    {
        if (base.TakeDamage(damage))
        {
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

            return true;
        }

        return false;
    }

    public override void HealDamage(int damage = 1)
    {
        base.HealDamage(damage);
    }

    private void ShowFloatingHealthBar(float lifetime = 1f)
    {
        if (floatingHealthBarPrefab)
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

}
