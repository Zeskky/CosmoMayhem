using System.Collections;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Damageable Object Properties")]
    [SerializeField] protected int maxHealth = 3;
    public int MaxHealth { get { return maxHealth; } }
    protected int health;
    public int Health 
    { 
        get { return health; }
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
        }
    }

    public float NormalizedHealth
    {
        get { return (float)health / maxHealth; }
    }

    [SerializeField] private GameObject deathEffect, preDeathEffect;
    protected float immuneTimer;
    [SerializeField] protected float damageImmunityTime = 2f;

    public bool IsImmune
    {
        get { return immuneTimer > 0; }
    }

    public bool WasDamagedThisFrame
    {
        get { return Mathf.Abs(immuneTimer - damageImmunityTime) <= 1 / 60f; }
    }

    public virtual bool TakeDamage(int damage = 1)
    {
        if (IsImmune || damage <= 0)
            return false;

        immuneTimer = damageImmunityTime;
        Health -= damage;
        return true;
    }

    public virtual void HealDamage(int damage = 1)
    {
        Health += damage;
    }

    public virtual void HealDamage(float ratio = 1f)
    {
        Health += Mathf.CeilToInt(maxHealth * ratio);
    }

    public virtual void Die()
    {
        if (deathEffect)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    protected void ImmuneUpdate()
    {
        immuneTimer = Mathf.Max(immuneTimer - Time.fixedDeltaTime, 0);
    }

    protected void SetupHealth()
    {
        health = maxHealth;
    }
}