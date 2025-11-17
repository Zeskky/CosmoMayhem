using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Launcher : MonoBehaviour
{
    public static Launcher Instance;

    [SerializeField] private GameObject attractStartPanel;
    [SerializeField] private StudioEventEmitter confirmEmitter;
    [SerializeField] private Animator uiAnimator;

    [SerializeField] private List<string> attractScenesSequence;

    /// <summary>
    /// The stats from all the stages played on this game so far.
    /// </summary>
    public List<StageStats> GameStageStats { get; }

    public bool InTransition { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            // Already existing instance: destroy this one
            Destroy(gameObject);
        }
        else
        {
            // Store this instance's reference, making it persistent between scenes
            DontDestroyOnLoad((Instance = this).gameObject);
            NextAttractScene();
        }


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InTransition = false;
    }

    public void OnConfirm(InputValue value)
    {
        if (value.isPressed)
        {
            if (confirmEmitter) confirmEmitter.Play();
        }
    }

    private void LateUpdate()
    {
        GameObject animatorGO;
        Animator outAnim;
        if (animatorGO = GameObject.FindGameObjectWithTag("Out-able"))
        {
            if (outAnim = animatorGO.GetComponent<Animator>())
            {
                AnimatorStateInfo asi = outAnim.GetCurrentAnimatorStateInfo(0);
                print(asi.normalizedTime);
                if (asi.IsTag("Out") && asi.normalizedTime >= 1)
                {
                    NextAttractScene();
                }
            }
        }
    }

    public void NextAttractScene()
    {
        int nextSceneIndex = attractScenesSequence.IndexOf(SceneManager.GetActiveScene().name) + 1;
        if (nextSceneIndex >= attractScenesSequence.Count)
        {
            nextSceneIndex = 0;
        }

        string nextScene = attractScenesSequence[nextSceneIndex];
        SceneManager.LoadScene(nextScene);
    }

    /*
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
        yield return new WaitForSeconds(1f);
        uiAnimator.SetTrigger("RunTransition");
        // currentScreen.ScreenObjects.ForEach(screen => screen.SetActive(true));
    }
    */
}
