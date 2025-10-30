using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public Vector2 startVelocity;
    [SerializeField] private float lifetime = 6f;
    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        (rb = GetComponent<Rigidbody2D>()).linearVelocity = startVelocity;
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Projectile>() && !collision.CompareTag(gameObject.tag))
        {
            // Destroy this projectile whenever it collides with another one,
            // unless both GameObjects have the same tag.
            Destroy(gameObject);
        }
    }
}
