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
    [SerializeField] private StudioEventEmitter confirmEmitter, selectionChangeEmitter;
    [SerializeField] private StudioEventEmitter musicFadeoutCommand;
    // [SerializeField] private Animator systemAnimator;
    [SerializeField] private Image fadeTransitionOverlay;
    
    [SerializeField] private GameObject stageClearedTransition, stageFailedTransition;
    // [SerializeField] private float transitionStayTime = 5f;

    [SerializeField] private List<string> attractSequenceScenes;
    [SerializeField] private List<Grade> stageGrades;
    [SerializeField] private Grade stageFailedGrade;

    public List<Grade> StageGrades {  get { return stageGrades; } }
    public Grade StageFailedGrade { get { return stageFailedGrade; } }

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
                DoMenuLogic();
                //StartCoroutine(ScreenOutCo());
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

    public void OnMoveCursor(InputValue value)
    {
        Vector2 cursorDir = value.Get<Vector2>();
        if (cursorDir.magnitude >= 0.4f)
        {
            MoveMenuCursor(cursorDir);
        }
    }

    private void MoveMenuCursor(Vector2 direction)
    {
        InteractableMenu currentMenu;
        if (currentMenu = FindFirstObjectByType<InteractableMenu>())
            currentMenu.MenuItemIndex += Mathf.CeilToInt(direction.x);
    }

    public void PlaySelectionChangeSound()
    {
        if (selectionChangeEmitter) selectionChangeEmitter.Play();
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
            menuTimerGO.SetActive(MenuTimer > 0 && !IsOnAttractSequence());
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

    public bool IsOnAttractSequence()
    {
        return attractSequenceScenes.Contains(SceneManager.GetActiveScene().name);
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
                /*
            case "Evaluation":
                nextScene = "GameOver";
                break;
                */
        }

        return nextScene;
    }

    public void DoMenuLogic()
    {
        InteractableMenu currentMenu = FindFirstObjectByType<InteractableMenu>();
        if (currentMenu)
        {
            currentMenu.CurrentMenuItem.OnConfirm();
            //StartCoroutine(ScreenOutCo());
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "CompanyLogo")
            {
                StartCoroutine(ScreenOutCo());
            }
        }
    }

    public void GoToScene(string targetScene)
    {
        StartCoroutine(ScreenOutCo(targetScene));
    }

    public IEnumerator ScreenOutCo(string targetScene = "", float duration = 1f)
    {
        if (string.IsNullOrEmpty(targetScene))
            targetScene = GetNextSceneName();

        TimerEnabled = false;
        float t = 0;

        // Do music fade-out
        musicFadeoutCommand.gameObject.SetActive(true);
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

        SetMusicStatus(false);
        canConfirm = true;
    }

    public void NextAttractScene()
    {
        int nextSceneIndex = attractSequenceScenes.IndexOf(SceneManager.GetActiveScene().name) + 1;
        if (nextSceneIndex >= attractSequenceScenes.Count)
        {
            nextSceneIndex = 0;
        }

        string nextScene = attractSequenceScenes[nextSceneIndex];
        SceneManager.LoadScene(nextScene);
    }

    public void SendEndStage(StageStats stats)
    {
        StartCoroutine(SendEndStageCo(stats));
    }

    public void SetMusicStatus(bool state = true)
    {
        musicFadeoutCommand.gameObject.SetActive(state);
    }

    private IEnumerator SendEndStageCo(StageStats stats)
    {
        bool success = stats.Result == StageResult.Cleared;
        
        // Do music fade-out
        musicFadeoutCommand.Play();

        GameStageStats.Add(stats);

        GameObject stageEndTransition = success ? stageClearedTransition : stageFailedTransition;

        GameObject newTransition = Instantiate(stageEndTransition, stageEndTransition.transform.parent);
        newTransition.SetActive(true);
        Animator anim = newTransition.GetComponent<Animator>();

        while (anim && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            yield return new WaitForEndOfFrame();

        yield return SceneManager.LoadSceneAsync("Evaluation");

        Time.timeScale = 1.0f;

        ScoreEntry newEntry = new()
        {
            score = stats.TotalScore
        };
        LocalScoresManager.Instance.SubmitScoreEntry(newEntry);
        SetMusicStatus(true);
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
