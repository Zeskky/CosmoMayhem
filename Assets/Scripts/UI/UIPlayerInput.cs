using UnityEngine;
using UnityEngine.InputSystem;

public class UIPlayerInput : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make this GameObject a child of Launcher
        transform.parent = Launcher.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnConfirm(InputValue iv)
    {
        Launcher.Instance.OnConfirm(iv);
    }

    public void OnMoveCursor(InputValue iv)
    {
        Launcher.Instance.OnMoveCursor(iv);
    }

    public void OnFastForward(InputValue iv)
    {
        Launcher.Instance.OnFastForward(iv);
    }
}
