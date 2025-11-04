using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public float startVelocity;
    public int damage = 1;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private GameObject explosionPrefab;
    // [SerializeField] private float gracePeriod = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        if (collision.GetComponent<Projectile>() && !collision.CompareTag(gameObject.tag))
        {
            // Destroy this projectile whenever it collides with another one,
            // unless both GameObjects have the same tag.
            StartCoroutine(DestroyProjectileCo());
        }
    }
}
