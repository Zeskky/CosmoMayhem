using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{
    [SerializeField] private GameObject titleMessagePanel;
    private string currentScreen;
    public bool InTransition { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InTransition = false;
        currentScreen = "ScreenTitle";
    }

    public void OnConfirm(InputValue value)
    {
        if (value.isPressed)
        {
            GoToScreen("ScreenModeSelect");
        }
    }

    public void GoToScreen(string targetScreen)
    {
        GameObject targetPointGO = GameObject.FindGameObjectWithTag(targetScreen);
        if (targetPointGO)
        {
            currentScreen = targetScreen;
            Vector3 targetPosition = targetPointGO.transform.position;
            StartCoroutine(MoveCameraToPointCo(targetPosition));

            // UI update
            if (targetScreen != "ScreenTitle")
            {
                titleMessagePanel.SetActive(false);
            }
        }
    }

    private IEnumerator MoveCameraToPointCo(Vector3 targetPos, float duration = 1f)
    {
        Vector3 originPos = Camera.main.transform.position;
        float moveTimer = 0;
        InTransition = true;

        while (moveTimer < duration)
        {
            Camera.main.transform.position = Vector3.Lerp(originPos, targetPos, moveTimer / duration);
            yield return new WaitForEndOfFrame();
            moveTimer += Time.deltaTime;
        }

        // Snap camera position to target
        Camera.main.transform.position = targetPos;
        InTransition = false;
    }
}
