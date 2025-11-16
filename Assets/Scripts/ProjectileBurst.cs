using UnityEngine;

public enum BurstShape
{
    Circle,
    Axial,
    Diagonal
}

public class ProjectileBurst : MonoBehaviour
{
    [SerializeField] private BurstShape shape;
    public GameObject projectilePrefab;
    public int projectileCount = 8;
    public float projectileVelocity = 6f, burstMaxAngle = 360f, projectileOffset = .1f;
    [Range(0f, 1f)]
    public float burstAngleAnchor = 0.5f;
    public int projectileDamage = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GenerateBurst();
    }

    private void GenerateBurst()
    {
        float individualAngle = burstMaxAngle / projectileCount;
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            float angle = i * individualAngle;
            // print($"{i}: {angle}");
            newProjectile.transform.localScale = transform.localScale;
            newProjectile.transform.Rotate(0, 0, angle + (-burstMaxAngle * burstAngleAnchor));
            newProjectile.transform.position += newProjectile.transform.right * projectileOffset;
            Projectile projectile;
            if (projectile = newProjectile.GetComponent<Projectile>())
            {
                projectile.startVelocity = projectileVelocity;
                projectile.damage = projectileDamage;
            }
        }

        Destroy(gameObject);
    }
}
