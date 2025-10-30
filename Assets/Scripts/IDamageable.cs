
public interface IDamageable
{
    public int Health { get; set; }

    public void TakeDamage(int damage = 1);
    public void HealDamage(int damage = 1);
    public void Die();
}