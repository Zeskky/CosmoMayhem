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
    [Tooltip("How long the projectile will stay in scene. Set this value below 0 if you want to destroy this object manually.")]
    [SerializeField] private float lifetime = 6f;
    [Tooltip("The trigger to fire from the attached Animator Controller at the end of lifetime. Leave it empty to destroy this projectile immediately.")]
    [SerializeField] private string lifetimeOutAnimatorTrigger;
    [SerializeField] private bool destroyOnContact = true;
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

    public void ForceProjectileDestruction()
    {
        Destroy(gameObject);
    }

    public void DestroyProjectile()
    {
        if (destroyOnContact)
        {
            StartCoroutine(DestroyProjectileCo());
        }
    }

    private IEnumerator DestroyProjectileCo(float delay = 0f, bool explode = true)
    {
        if (delay >= 0)
        {
            yield return delay == 0 ? null : new WaitForSeconds(delay);
            if (explosionPrefab && explode)
                _ = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            Animator anim = GetComponent<Animator>();
            if (!anim)
            {
                Destroy(gameObject);
            }
            else if (string.IsNullOrEmpty(lifetimeOutAnimatorTrigger))
            {
                Destroy(gameObject);
            }
            else
            {
                anim.SetTrigger(lifetimeOutAnimatorTrigger);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Projectile other = collision.GetComponent<Projectile>();
        if (other && !collision.CompareTag(gameObject.tag))
        {
            // print($"{gameObject.name}: {priority}");
            if (priority <= other.priority && destroyOnContact)
            {
                // Destroy this projectile unless it has higher priority
                StartCoroutine(DestroyProjectileCo());
            }
        }
    }
}
