using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable]
public class MenuScreen
{
    [SerializeField] private string id;
    [SerializeField] private Transform screenCenterPoint;
    [SerializeField] private List<GameObject> screenObjects;

    public Transform ScreenCenterPoint { get { return screenCenterPoint; } }
    public string Id { get { return id; } }
    public List<GameObject> ScreenObjects { get { return screenObjects; } }
}

public class Launcher : MonoBehaviour
{
    [SerializeField] private GameObject titleMessagePanel;
    [SerializeField] private List<MenuScreen> menuScreens;
    [SerializeField] private StudioEventEmitter confirmEmitter;
    private MenuScreen currentScreen;
    public bool InTransition { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InTransition = false;
        currentScreen = menuScreens.FirstOrDefault();
        if (currentScreen == null)
        {
            Debug.LogError("No valid Menu Screens were found! Consider adding one in the Inspector.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    public void OnConfirm(InputValue value)
    {
        if (value.isPressed)
        {
            if (confirmEmitter) confirmEmitter.Play();
            GoToScreen("ModeSelect");
        }
    }

    public void GoToScreen(string screenId)
    {
        MenuScreen targetScreen = menuScreens.FirstOrDefault(s => s.Id == screenId);
        if (targetScreen != null)
        {
            // Make sure it's a different screen
            if (currentScreen != targetScreen)
            {
                currentScreen.ScreenObjects.ForEach(screen => screen.SetActive(false));
                (currentScreen = targetScreen).ScreenObjects.ForEach(screen => screen.SetActive(false));
                Vector3 targetPosition = (currentScreen = targetScreen).ScreenCenterPoint.position;
                StartCoroutine(MoveCameraToPointCo(targetPosition));
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
