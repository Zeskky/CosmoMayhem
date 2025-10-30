using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private AnimationCurve movementPath;
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
        rb.linearVelocityY = movementPath.Evaluate(movementTimer += Time.deltaTime);
    }

    public void Die()
    {
        // TODO: show explosion effect
        GameManager.Instance.score += scoreValue;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            // Take damage only from player bullets
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
