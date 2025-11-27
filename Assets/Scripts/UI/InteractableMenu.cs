using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class InteractableMenu : MonoBehaviour
{
    [SerializeField] private List<MenuItem> menuItems;
    [SerializeField] private bool allowOptionWarp = false;
    [SerializeField] private List<GameObject> playerCursors;
    private int menuItemIndex = 0;

    public int MenuItemIndex
    {
        get { return menuItemIndex; }
        set
        {
            if (menuItems.Count == 0) menuItemIndex = 0;
            else
            {
                int newIndex = allowOptionWarp ? value % menuItems.Count
                    : Mathf.Clamp(value, 0, menuItems.Count - 1);

                if (newIndex != menuItemIndex)
                {
                    menuItemIndex = newIndex;

                    // Play sound and focus the new menu item if it has actually changed
                    Launcher.Instance.PlaySelectionChangeSound();
                    playerCursors.ForEach(cur => cur.transform.SetParent(CurrentMenuItem.CursorContainer));
                }
            }
        }
    }

    public MenuItem CurrentMenuItem
    {
        get { return menuItems.Count > 0 ? menuItems[menuItemIndex] : null; }
    }

    public void GoToScene(string sceneName)
    {
        Launcher.Instance.GoToScene(sceneName);
    }

    private void Start()
    {

    }
}
