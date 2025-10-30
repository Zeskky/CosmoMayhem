using UnityEngine;

public enum MovementBehaviour
{
    GoForward,
    Halt,
    Warp,
}

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private AnimationCurve movementPath;
    [SerializeField] private MovementBehaviour movementBehaviour;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private float speedModifier;

    [Header("Shooting Behaviour")]
    [SerializeField] private bool canShoot = false;
    [SerializeField] private float shotDelay = 2f, shotSpeed = 5f;
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private Transform cannonPoint;
    private float shotTimer;

    public int Health { get; set; }

    private float movementTimer;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canShoot)
        {
            if ((shotTimer += Time.deltaTime) >= shotDelay)
            {
                shotTimer -= shotDelay;
                GenerateShot();
            }
        }

        switch (movementBehaviour)
        {
            case MovementBehaviour.GoForward:
                // Enemy will keep going forward
                // Avoid using this value, unless it's an optional enemy
                break;
            case MovementBehaviour.Halt:
                // Enemy will halt upon reaching target horizontal position (x)
                Vector3 actualTargetPosition = Camera.main.ViewportToWorldPoint(targetPosition);
                if (transform.position.x < actualTargetPosition.x)
                {
                    transform.parent = Camera.main.transform;
                }
                break;
            case MovementBehaviour.Warp:
                // Enemy will warp to the initial spawn position
                Vector3 warpPosition = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f));
                if (transform.position.x < warpPosition.x)
                {
                    transform.position = new(GameManager.Instance.GetSpawnAreaPosition().x, transform.position.y);
                }
                break;
        }

        rb.linearVelocity = new Vector2(-speedModifier, movementPath.Evaluate(movementTimer += Time.deltaTime));
    }

    private void GenerateShot()
    {
        GameObject newShot = Instantiate(shotPrefab, cannonPoint.position, Quaternion.identity);
        newShot.GetComponent<Projectile>().startVelocity = new(-shotSpeed, 0);
    }

    public void Die()
    {
        // TODO: show explosion effect
        GameManager.Instance.score += scoreValue * GameManager.Instance.Multiplier;
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag(gameObject.tag) && collision.GetComponent<Projectile>())
        {
            // Take damage only from non-enemy bullets
            TakeDamage();
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    public void HealDamage(int damage = 1)
    {

    }
}
