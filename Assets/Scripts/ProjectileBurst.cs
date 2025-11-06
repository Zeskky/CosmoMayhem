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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        GenerateBurst();
    }

    private void GenerateBurst()
    {
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            float angle = i * burstMaxAngle / projectileCount;
            // print($"{i}: {angle}");
            newProjectile.transform.Rotate(0, 0, angle);
            newProjectile.transform.position += newProjectile.transform.right * projectileOffset;
            if (newProjectile.GetComponent<Projectile>())
                newProjectile.GetComponent<Projectile>().startVelocity = projectileVelocity;
        }

        Destroy(gameObject);
    }
}
