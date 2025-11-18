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
    [Header("Basic Enemy Properties")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private MovementBehaviour movementBehaviour;
    [SerializeField] private bool doWarp = true, doHalt = false, inactiveUntilHalted = false;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float minSpeedModifier, maxSpeedModifier/*, maxVelocity = 5f*/;
    [SerializeField] private int baseDamage = 2;
    public int BaseDamage { get { return baseDamage; } }
    [SerializeField] private SpriteRenderer enemyRenderer;

    [Header("Shooting Behaviour Properties")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private float minShotDelay = 2f, maxShotDelay = 3f, shotSpeed = 5f;
    [SerializeField] private Transform cannonPoint;
    public float ShotRate { get; protected set; }

    [Header("Ram Behaviour Properties")]
    [SerializeField] private bool doesRam = false;
    [SerializeField] private float ramDelay = 4f, ramChargeTime = 1f, ramSpeedModifier = 4f;
    [SerializeField] private GameObject ramTrail;
    [SerializeField] private StudioEventEmitter ramChargeEmitter;
    [SerializeField] private Gradient ramChargeTimerGradient;
    private float ramDelayTimer, ramChargeTimer;

    private bool isChargingToRam = false, isRamming = false;

    [Header("Prefab References")]
    [SerializeField] private GameObject floatingHealthBarPrefab;
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private GameObject damageEmitterPrefab;

    // Private flags
    private float shotTimer, nextShotDelay;
    private float movementTimer, movementSpeed;
    private Vector3 startPosition, ramStartPos;
    private Rigidbody2D rb;
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
        ShotRate = 1;
        nextShotDelay = Random.Range(minShotDelay, maxShotDelay) / ShotRate;

        startPosition = transform.position;
        movementSpeed = Random.Range(minSpeedModifier, maxSpeedModifier);
    }

    // Update is called once per frame
    void Update()
    {
        movementTimer += Time.deltaTime;
        PlayerController player = FindAnyObjectByType<PlayerController>();

        Vector2 moveDir = rb.linearVelocity.normalized;
        if (moveDir == Vector2.zero)
            moveDir = Vector2.left;

        if (doesRam)
        {
            if (ramTrail)
                ramTrail.SetActive(isRamming);
            if (!isRamming && !isChargingToRam)
            {
                if ((ramDelayTimer += Time.deltaTime) >= ramDelay)
                {
                    isChargingToRam = true;
                    ramDelayTimer = 0;

                    ramChargeEmitter.Play();
                }
            }
        }

        // print(rb.linearVelocity);

        if (isRamming)
        {
            // Ram towards the screen's left border
            moveDir.x = -ramSpeedModifier;
            Vector2 currentVpPos = Camera.main.WorldToViewportPoint(transform.position);
            if (currentVpPos.x < -1) isChargingToRam = isRamming = false;
        }
        else if (isChargingToRam)
        {
            // Preparing to ram towards the left border of the screen
            moveDir = Vector2.zero;
            ramChargeTimer += Time.deltaTime;
            
            float normalizedCharge = ramChargeTimer / ramChargeTime;
            Vector2 vpPos = Camera.main.ViewportToWorldPoint(targetPosition);
            enemyRenderer.color = ramChargeTimerGradient.Evaluate(normalizedCharge);

            transform.position = new Vector3(
                vpPos.x + normalizedCharge * (transform.localScale.magnitude / 4),
                transform.position.y
            );

            if (ramChargeTimer >= ramChargeTime)
            {
                ramChargeTimer -= ramChargeTime;
                isRamming = true;
            }
        }
        else
        {
            // Standard behaviours
            enemyRenderer.color = Color.white;

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
                        ? t - (s / 2) // Odd
                        : -t + (s / 2); // Even
                    break;
                case MovementBehaviour.FollowPlayerY:
                    // Enemy will follow the player ship vertically (y)
                    if (player)
                        moveDir.y = (player.transform.position - transform.position).normalized.y;
                    break;
            }

            if (doWarp)
            {
                // Enemy will warp to the initial spawn position
                Vector3 warpPosition = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f)) + Vector3.left;
                if (transform.position.x < warpPosition.x)
                {
                    transform.position = new(GameManager.Instance.GetSpawnAreaPosition().x + transform.localScale.x / 2f, transform.position.y);
                }
            }

            if (canShoot)
            {
                if ((shotTimer += Time.deltaTime) >= nextShotDelay)
                {
                    shotTimer -= nextShotDelay;
                    nextShotDelay = Random.Range(minShotDelay, maxShotDelay) / ShotRate;
                    GenerateShot();
                }
            }

            if (doHalt)
            {
                // Enemy will halt upon reaching target horizontal position (x)
                Vector3 actualTargetPosition = Camera.main.ViewportToWorldPoint(targetPosition);

                if (transform.position.x < actualTargetPosition.x)
                {
                    transform.parent = Camera.main.transform;
                    moveDir.x = 0;
                }
                else
                {
                    if (inactiveUntilHalted)
                    {
                        // Enemy will be inactive until has reached the halt point
                        ramDelayTimer = shotTimer = 0;
                        immuneTimer = damageImmunityTime * 2f;
                    }
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
        GameObject newShotInstance = Instantiate(shotPrefab, cannonPoint.position, Quaternion.identity);
        newShotInstance.transform.right = cannonPoint.right;

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
            /*
            rb.AddForce(
                projectile.GetComponent<Rigidbody2D>().linearVelocity * (projectile.damage * .02f),
                ForceMode2D.Impulse
            );
            */
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
                if (damageEmitterPrefab)
                {
                    GameObject damageEmitterInstance = Instantiate(damageEmitterPrefab, transform);
                    Destroy(damageEmitterInstance, .5f);
                }

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

    public void SetRamBehaviourEnabled(bool enabled)
    {
        doesRam = enabled;
    }
}
