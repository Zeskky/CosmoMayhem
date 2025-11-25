using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum MenuItemCondition
{
    /// <summary>
    /// No menu item enable condition applied, making it always elegible.
    /// </summary>
    None,
    /// <summary>
    /// Menu item enabled on single player games.
    /// </summary>
    SingleOnly,
    /// <summary>
    /// Item enabled on multiplayer games.
    /// </summary>
    NoSingle,
}

public class MenuItem : MonoBehaviour
{
    [SerializeField] private Image dimFilter;
    [SerializeField] private UnityEvent confirmEvent;
    [SerializeField] private MenuItemCondition enableCondition;

    [SerializeField] private Transform cursorContainer;
    public Transform CursorContainer {  get { return cursorContainer; } }

    private void Start()
    {
        
    }

    public void OnConfirm()
    {
        // TODO: get elegible condition working
        confirmEvent.Invoke();
    }
}