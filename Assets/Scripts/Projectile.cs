using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float startVelocity;
    public int damage = 1;
    [Range(1, 10)]
    public int priority = 0;
    [SerializeField] private bool scaledPriority = true;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private GameObject explosionPrefab;
    // [SerializeField] private float gracePeriod = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (scaledPriority)
            priority = (int)(priority * transform.localScale.magnitude);
        GetComponent<Rigidbody2D>().linearVelocity = transform.right * startVelocity;
        StartCoroutine(DestroyProjectileCo(lifetime, false));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DestroyProjectile()
    {
        StartCoroutine(DestroyProjectileCo());
    }

    private IEnumerator DestroyProjectileCo(float delay = 0f, bool explode = true)
    {
        yield return delay > 0 ? new WaitForSeconds(delay) : null;
        if (explosionPrefab && explode)
        {
            GameObject newExplosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Projectile other = collision.GetComponent<Projectile>();
        if (other && !collision.CompareTag(gameObject.tag))
        {
            print($"{gameObject.name}: {priority}");
            if (priority <= other.priority)
            {
                // Destroy this projectile unless it has higher priority
                StartCoroutine(DestroyProjectileCo());
            }
        }
    }
}
