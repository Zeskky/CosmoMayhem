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
}
