using UnityEngine;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer barFillRenderer;
    public float HealthBarFill
    {
        get { return barFillRenderer.size.x; }
        set { barFillRenderer.size = new Vector2(value, barFillRenderer.size.y); }
    }
}
