using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Damageable
{
    [SerializeField] private float movementSpeed = 3f;
    public Vector2 MovementDirection { get; private set; }

    [SerializeField] private int maxHealth = 3;
    public int MaxHealth { get { return maxHealth; } }

    [SerializeField] private float damageImmunityTime = 2f, immunityBlinkDuration = 0.5f;
    private float immuneTimer;
    [SerializeField] private Gradient immunityBlinkPattern;

    private Rigidbody2D rb;
    [SerializeField] private Rect boundaryRect;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform cannonPoint;

    [SerializeField] private float shotDelay = 1 / 20f;
    [SerializeField] private int shotDamage = 1;
    [SerializeField] private float altShotDelay = 2f;
    [SerializeField] private GameObject altShotPrefab;
    // [SerializeField] private bool autoShooting = false;
    private float shotTimer, altShotTimer;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private StudioEventEmitter damageEmitter;

    [Header("Dash Mechanic Properties")]
    [SerializeField] private int maxDashCharges = 2;
    [SerializeField] private float dashChargeCooldown = 4f, dashDelay = 1f, dashDuration = 0.5f, dashForce = 4f;
    [SerializeField] private SpriteRenderer dashChargeBar;
    public int DashCharges { get; set; }
    public bool IsDashReady { get { return dashChargeTimer <= 0; } }
    private float dashChargeTimer, dashDelayTimer;
    private Vector2 dashVelocityModifier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = maxHealth;
        DashCharges = maxDashCharges;
        // Ensure rigidbody type is set to 'Kinematic'
        (rb = GetComponent<Rigidbody2D>()).bodyType = RigidbodyType2D.Kinematic;
    }
    
    // Update is called once per frame
    void Update()
    {
        shotTimer = Mathf.Max(shotTimer - Time.deltaTime, 0);
        altShotTimer = Mathf.Max(altShotTimer - Time.deltaTime, 0);

        rb.linearVelocity = (MovementDirection + dashVelocityModifier) * movementSpeed;
        dashVelocityModifier = Vector2.Lerp(dashVelocityModifier, Vector2.zero, Time.deltaTime / dashDuration);

        // Clamp ship to camera boundaries
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        rb.linearVelocityX = (
            (viewportPos.x <= boundaryRect.x && rb.linearVelocityX < 0)
            || (viewportPos.x >= boundaryRect.x + boundaryRect.width && rb.linearVelocityX > 0))
            ? 0
            : rb.linearVelocityX;
        rb.linearVelocityY = (
            (viewportPos.y <= boundaryRect.y && rb.linearVelocityY < 0)
            || (viewportPos.y >= boundaryRect.y + boundaryRect.height && rb.linearVelocityY > 0))
            ? 0
            : rb.linearVelocityY;
    }

    private void FixedUpdate()
    {
        immuneTimer = Mathf.Max(immuneTimer - Time.fixedDeltaTime, 0);
        sr.color = IsImmune
            ? immunityBlinkPattern.Evaluate(
                (damageImmunityTime - immuneTimer) % immunityBlinkDuration / Mathf.Max(immunityBlinkDuration, 1E-4f)
            )
            : Color.white;

        dashDelayTimer = Mathf.Max(dashDelayTimer - Time.fixedDeltaTime, 0);
        dashChargeTimer = Mathf.Max(dashChargeTimer - Time.fixedDeltaTime, 0);
        dashChargeBar.size = new(1 - dashChargeTimer / dashChargeCooldown, dashChargeBar.size.y);
        dashChargeBar.color = IsDashReady ? Color.white : Color.gray;
    }

    public void OnMove(InputValue value)
    {
        MovementDirection = value.Get<Vector2>();
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed && shotTimer <= 0)
        {
            Projectile newProjectile = Instantiate(bulletPrefab, cannonPoint.position, cannonPoint.rotation).GetComponent<Projectile>();
            if (newProjectile)
            {
                newProjectile.startVelocity = new Vector2(10, 0);
                newProjectile.damage = shotDamage;
            }

            shotTimer = shotDelay;
        }
    }

    public void OnAltFire(InputValue value)
    {
        if (value.isPressed && altShotTimer <= 0)
        {
            altShotTimer = altShotDelay;
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            if (dashDelayTimer <= 0 && dashChargeTimer <= 0)
            {
                // Can dash
                immuneTimer += dashDuration;
                dashVelocityModifier = MovementDirection * dashForce;
                dashDelayTimer = dashDelay;
                dashChargeTimer = dashChargeCooldown;
            }
        }
    }

    public override void TakeDamage(int damage = 1)
    {
        if (IsImmune || damage <= 0)
            return;

        immuneTimer = damageImmunityTime;
        base.TakeDamage(damage);
        // Debug.Log(Health);

        if (Health > 0)
        {
            // Still alive
            damageEmitter.Play();
        }
        else
        {
            Die();
        }
    }

    public override void HealDamage(int damage = 1)
    {
        base.HealDamage(damage);
    }

    public override void Die()
    {
        // TODO: show explosion effect
        GameManager.Instance.DoGameOverSequence();
        base.Die();
    }


    public bool IsImmune
    {
        get { return immuneTimer > 0; }
    }

    public bool WasDamagedThisFrame
    {
        get { return Mathf.Abs(immuneTimer - damageImmunityTime) <= 1 / 60f; }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag(gameObject.tag))
        {
            Projectile projectile = collision.GetComponent<Projectile>();
            Enemy enemy;
            if (projectile)
            {
                // Projectile damage
                TakeDamage(projectile.damage);
            }
            else if (enemy = collision.GetComponent<Enemy>())
            {
                // Contact damage
                TakeDamage(enemy.BaseDamage);
            }

            if (projectile && WasDamagedThisFrame)
            {
                // Destroy the colliding projectile, only if the player took damage
                Destroy(collision.gameObject);
            }
        }
    }
}
