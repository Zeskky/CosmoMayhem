using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Damageable
{
    [SerializeField] private float movementSpeed = 3f;
    public Vector2 MovementDirection { get; private set; }

    [SerializeField] private float immunityBlinkDuration = 0.5f;
    [SerializeField] private Gradient immunityBlinkPattern;

    private Rigidbody2D rb;
    [SerializeField] private Rect boundaryRect;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform cannonPoint;

    [SerializeField] private float shotDelay = 1 / 20f;
    [SerializeField] private int shotDamage = 1;
    [SerializeField] private float altShotDelay = 2f;
    [SerializeField] private GameObject altShotPrefab;
    [SerializeField] private bool autoShooting = false;
    private float shotTimer, altShotTimer;
    private bool fireState, altFireState;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private StudioEventEmitter damageEmitter, repairEmitter;

    [Header("Dash Mechanic Properties")]
    [SerializeField] private int maxDashCharges = 2;
    [SerializeField] private float dashChargeCooldown = 4f, dashDelay = 1f, dashDuration = 0.5f, dashForce = 4f;
    [SerializeField] private SpriteRenderer dashChargeBar;
    [SerializeField] private StudioEventEmitter dashEmitter;

    [Header("Super Core Properties")]
    private bool anyFireActionPerformed = false;
    [SerializeField] private int minSupercoreCharge = 100;
    private int currentSupercoreCharge = 0;
    public int CurrentSupercoreCharge
    {
        get {  return currentSupercoreCharge; }
        set
        {
            currentSupercoreCharge = Mathf.Clamp(value, 0, currentSupercoreCharge);
        }
    }

    public int DashCharges { get; set; }
    public bool IsDashReady { get { return dashChargeTimer <= 0; } }
    private float dashChargeTimer, dashDelayTimer;
    private Vector2 dashVelocityModifier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupHealth();
        DashCharges = maxDashCharges;
        // Ensure rigidbody type is set to 'Kinematic'
        (rb = GetComponent<Rigidbody2D>()).bodyType = RigidbodyType2D.Kinematic;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (shotTimer > 0)
        {
            if ((shotTimer -= Time.deltaTime) <= 0)
            {
                shotTimer = 0;
                // SuperCore input reset
                anyFireActionPerformed = false;
            }
        }

        if (altShotTimer > 0)
        {
            if ((altShotTimer -= Time.deltaTime) <= 0)
            {
                altShotTimer = 0;
                // SuperCore input reset
                anyFireActionPerformed = false;
            }
        }

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


        // Primary Fire
        if (fireState && shotTimer <= 0)
        {
            // Keep doing primary fire, if allowed
            if (!autoShooting)
            {
                fireState = false;
            }

            if (anyFireActionPerformed)
            {
                anyFireActionPerformed = false;
                ActivateSuperCore();
            }
            else
            {
                anyFireActionPerformed = true;
                if (bulletPrefab)
                {
                    Projectile newProjectile = Instantiate(bulletPrefab, cannonPoint.position, cannonPoint.rotation).GetComponent<Projectile>();
                    if (newProjectile)
                    {
                        newProjectile.startVelocity = 10;
                        newProjectile.damage = shotDamage;
                    }

                    shotTimer = shotDelay;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        ImmuneUpdate();

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
        fireState = value.isPressed;
    }

    public void OnAltFire(InputValue value)
    {
        altFireState = value.isPressed;
        // Bomb throw
        if (altFireState && altShotTimer <= 0)
        {
            if (anyFireActionPerformed)
            {
                anyFireActionPerformed = false;
                ActivateSuperCore();
            }
            else
            {
                anyFireActionPerformed = true;
                if (altShotPrefab)
                {
                    Projectile newAltProjectile = Instantiate(altShotPrefab, cannonPoint.position, cannonPoint.rotation).GetComponent<Projectile>();
                    if (newAltProjectile)
                    {
                        newAltProjectile.startVelocity = 10;
                    }

                    altShotTimer = altShotDelay;
                }
            }
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
                dashEmitter.Play();
            }
        }
    }

    public override bool TakeDamage(int damage = 1)
    {
        if (base.TakeDamage(damage))
        {
            // Reset combo to 0
            GameManager.Instance.Combo = 0;

            if (Health > 0)
            {
                // Still alive
                damageEmitter.Play();
            }
            else
            {
                Die();
            }

            return true;
        }

        return false;
    }

    public override void HealDamage(int damage = 1)
    {
        base.HealDamage(damage);
        repairEmitter.Play();
    }

    public override void Die()
    {
        // TODO: show explosion effect
        GameManager.Instance.DoGameOverSequence();
        base.Die();
    }

    private void ActivateSuperCore()
    {
        if (CurrentSupercoreCharge == minSupercoreCharge)
        {
            print("SuperCore has been activated!");
        }
        else
        {
            print("SuperCore is not ready yet.");
        }
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
