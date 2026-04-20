using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

[RequireComponent(typeof(RectTransform), typeof(Selectable))]
public class MenuItem : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [SerializeField] private Image dimFilter;
    [SerializeField] private UnityEvent confirmEvent;
    [SerializeField] private MenuItemCondition enableCondition;

    [SerializeField] private Transform cursorContainer;
    public Transform CursorContainer {  get { return cursorContainer; } }

    private void Start()
    {
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        InteractableMenu.UpdatePlayerCursor(cursorContainer);
        Launcher.Instance.PlaySelectionChangeSound();
    }

    public void InvokeConfirm()
    {
        confirmEvent.Invoke();
    }

    public void HideMenuItem(bool affectLayout = false)
    {
        if (affectLayout)
        {
            // Disable the GameObject
            gameObject.SetActive(false);
        }
        else
        {
            // Set its scale to 0 visually
            transform.localScale = Vector3.zero;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // TODO: get elegible condition working
        confirmEvent.Invoke();
    }
}