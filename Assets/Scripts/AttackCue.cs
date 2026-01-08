using UnityEngine;

public class AttackCue : MonoBehaviour
{
    [SerializeField] private GameObject attackObject;

    public void FinishCue()
    {
        if (attackObject)
        {
            _ = Instantiate(attackObject, transform.parent);
        }

        Destroy(gameObject);
    }
}
