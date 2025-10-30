using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private float movementSpeed = 3f;
    public Vector2 MovementDirection { get; private set; }

    public int Health { get; set; }
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float damageImmunityTime = 2f, immunityBlinkDuration = 0.5f;
    private float immuneTimer;
    [SerializeField] private Gradient immunityBlinkPattern;

    private Rigidbody2D rb;
    [SerializeField] private Rect boundaryRect;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform cannonPoint;

    [SerializeField] private float shotDelay = 1 / 20f;
    [SerializeField] private bool autoShooting = false;
    private float shotTimer;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private StudioEventEmitter damageEmitter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = maxHealth;
        // Ensure rigidbody type is set to 'Kinematic'
        (rb = GetComponent<Rigidbody2D>()).bodyType = RigidbodyType2D.Kinematic;
    }
    
    // Update is called once per frame
    void Update()
    {
        shotTimer = Mathf.Max(shotTimer - Time.deltaTime, 0);

        rb.linearVelocity = MovementDirection * movementSpeed;

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
        sr.color = IsImmune()
            ? immunityBlinkPattern.Evaluate(
                (damageImmunityTime - immuneTimer) % immunityBlinkDuration / immunityBlinkDuration
            )
            : Color.white;
    }

    public void OnMove(InputValue value)
    {
        MovementDirection = value.Get<Vector2>();
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed && shotTimer <= 0)
        {
            // TODO: shoot projectiles
            Projectile newProjectile = Instantiate(bulletPrefab, cannonPoint.position, cannonPoint.rotation).GetComponent<Projectile>();
            newProjectile.startVelocity = new Vector2(10, 0);
            shotTimer = shotDelay;
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (IsImmune() || damage <= 0)
            return;

        immuneTimer = damageImmunityTime;
        Health -= damage;
        Debug.Log(Health);

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

    public void HealDamage(int damage = 1)
    {

    }

    public void Die()
    {
        // TODO: show explosion effect
        Destroy(gameObject);
        GameManager.Instance.DoGameOverSequence();
    }


    public bool IsImmune()
    {
        return immuneTimer > 0;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            TakeDamage();
        }
    }
}
