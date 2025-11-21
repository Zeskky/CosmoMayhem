using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : MonoBehaviour
{
    public static Launcher Instance;

    // [SerializeField] private GameObject attractStartPanel;
    [SerializeField] private StudioEventEmitter confirmEmitter;
    // [SerializeField] private Animator systemAnimator;
    [SerializeField] private Image fadeTransitionOverlay;
    
    [SerializeField] private GameObject stageClearedTransition, stageFailedTransition;
    // [SerializeField] private float transitionStayTime = 5f;

    [SerializeField] private List<string> attractScenesSequence;

    [Header("Menu Timer Properties")]
    [SerializeField] private bool enableMenuTimer = true;
    [SerializeField] private int menuTime = 30;
    [SerializeField] private GameObject menuTimerGO;
    [SerializeField] private TMP_Text menuTimerCounterLabel;
    [SerializeField] private Image menuTimerBackground;
    [SerializeField] private Color timerNormalColor, timerDangerColor;
    [SerializeField] private int timerTickThreshold = 5;
    [SerializeField] private StudioEventEmitter timerTickEmitter;
    private int menuTimer;
    private float clockTimer;
    private readonly int timePerTick = 1;
    private string nextSceneName;

    public bool TimerEnabled { get; set; }
    private bool canConfirm = true;

    public int MenuTimer
    {
        get
        {
            return menuTimer;
        }
        set
        {
            menuTimer = value;
            menuTimerCounterLabel.text = menuTimer.ToString().PadLeft(2, '0');
            if (menuTimer <= 0)
            {
                menuTimer = 0;
                StartCoroutine(ScreenOutCo());
            }
            else if (menuTimer <= timerTickThreshold)
            {
                menuTimerCounterLabel.color = timerDangerColor;
                timerTickEmitter.Play();
            }
            else
            {
                menuTimerCounterLabel.color = timerNormalColor;
            }
        }
    }

    /// <summary>
    /// The stats from all the stages played on this game so far.
    /// </summary>
    public List<StageStats> GameStageStats { get; private set; }

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

            menuTimerGO.SetActive(false);
            GameStageStats = new List<StageStats>();
            canConfirm = true;
            InTransition = false;
            fadeTransitionOverlay.color = new Color(0f, 0f, 0f, 0f);

            NextAttractScene();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void OnConfirm(InputValue value)
    {
        if (value.isPressed && canConfirm)
        {
            canConfirm = false;
            switch (SceneManager.GetActiveScene().name)
            {
                case "CompanyLogo":
                    break;
                case "Title":
                    GameObject menu = GameObject.FindGameObjectWithTag("Menu");
                    if (menu ? menu.GetComponent<Animator>() : false)
                        menu.GetComponent<Animator>().SetTrigger("Confirm");
                    break;
                case "Menu":
                case "Evaluation":
                    break;
                default:
                    return;
            }

            // Next scene
            MenuTimer = 0;

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

    private void Update()
    {
        if (enableMenuTimer)
        {
            // Menu timer logic
            menuTimerGO.SetActive(MenuTimer > 0);
            if (TimerEnabled)
            {
                if ((clockTimer += Time.deltaTime) >= timePerTick)
                {
                    clockTimer -= timePerTick;
                    MenuTimer--;
                }
            }
        }
    }

    public void SetupMenuTimer(int menuTime, bool enabled = true)
    {
        if (enableMenuTimer)
        {
            clockTimer = 0;
            MenuTimer = menuTime;
            TimerEnabled = enabled;
        }
    }

    public string GetNextSceneName()
    {
        string nextScene = "";

        switch (SceneManager.GetActiveScene().name)
        {
            case "CompanyLogo":
            case "Title":
                nextScene = "Menu";
                break;
            case "Menu":
                nextScene = "Gameplay";
                break;
            case "Gameplay":
                nextScene = "Evaluation";
                break;
            case "Evaluation":
                nextScene = "GameOver";
                break;
        }

        return nextScene;
    }

    public IEnumerator ScreenOutCo(float duration = 1f)
    {
        string targetScene = GetNextSceneName();
        TimerEnabled = false;
        float t = 0;
        while (t < 1)
        {
            fadeTransitionOverlay.color = new Color(0f, 0f, 0f, t);
            yield return new WaitForEndOfFrame();
            t += Time.unscaledDeltaTime / duration;
        }

        fadeTransitionOverlay.color = Color.black;
        if (string.IsNullOrEmpty(targetScene))
        {
            // Next attract scene
            NextAttractScene();
        }
        else
        {
            // Disable UI input in-game
            GetComponent<PlayerInput>().enabled = !targetScene.Contains("Gameplay");

            // Specified scene
            yield return SceneManager.LoadSceneAsync(targetScene);
            fadeTransitionOverlay.color = new Color(0f, 0f, 0f, 0f);
            
            if (!targetScene.Contains("Gameplay"))
            {
                SetupMenuTimer(menuTime);
            }
        }

        canConfirm = true;
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

    public void SendEndStage(StageStats stats)
    {
        StartCoroutine(SendEndStageCo(stats));
    }

    private IEnumerator SendEndStageCo(StageStats stats)
    {
        bool success = stats.Result == StageResult.Cleared;
        GameStageStats.Add(stats);

        Animator anim = null;
        if (success)
        {
            GameObject newTransition = Instantiate(stageClearedTransition, stageClearedTransition.transform.parent);
            newTransition.SetActive(true);
            anim = newTransition.GetComponent<Animator>();
        }

        while (anim && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return SceneManager.LoadSceneAsync("Evaluation");
        if (anim) anim.SetTrigger("End");
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
