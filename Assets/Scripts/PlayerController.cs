using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 3f;
    public Vector2 MovementDirection { get; private set; }

    private Rigidbody2D rb;
    [SerializeField] private Rect boundaryRect;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform cannonPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure rigidbody type is set to 'Kinematic'
        (rb = GetComponent<Rigidbody2D>()).bodyType = RigidbodyType2D.Kinematic;
    }
    
    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = MovementDirection * movementSpeed;

        // Clamp ship to camera boundaries
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        rb.linearVelocityX = (
            (viewportPos.x <= boundaryRect.x && rb.linearVelocityX < 0)
            || (viewportPos.x >= boundaryRect.x + boundaryRect.width && rb.linearVelocityX > 0))
            ? 0
            : rb.linearVelocityX;
        rb.linearVelocityY = (
            (viewportPos.y <= boundaryRect.y && rb.linearVelocityY < 0)
            || (viewportPos.y >= boundaryRect.y + boundaryRect.height && rb.linearVelocityY > 0))
            ? 0
            : rb.linearVelocityY;
    }

    public void OnMove(InputValue value)
    {
        MovementDirection = value.Get<Vector2>();
    }

    public void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            // TODO: shoot projectiles
            Projectile newProjectile = Instantiate(bulletPrefab, cannonPoint.position, cannonPoint.rotation).GetComponent<Projectile>();
            newProjectile.startVelocity = new Vector2(6, 0);
        }
    }
}
