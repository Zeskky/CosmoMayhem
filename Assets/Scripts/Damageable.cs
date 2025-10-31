using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Damageable Object Properties")]
    public int Health { get; set; }
    [SerializeField] private GameObject deathEffect;

    public virtual void TakeDamage(int damage = 1)
    {
        Health -= damage;
    }
    public virtual void HealDamage(int damage = 1)
    {
        Health += damage;
    }

    public virtual void Die()
    {
        if (deathEffect)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}