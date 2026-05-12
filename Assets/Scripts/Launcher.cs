using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class PlayerInfo
{
    public int playerIndex;
    public InputDevice device;

    public PlayerInfo(int _playerIndex, InputDevice _device)
    {
        playerIndex = _playerIndex;
        device = _device;
    }
}

public class Launcher : MonoBehaviour
{
    public static Launcher Instance { get; private set; }

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
    [SerializeField] private string nameEntryCharacterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890?!.,";
    [SerializeField] private int nameEntryMaxLength = 10;
    [SerializeField] private string defaultHighScoreName = "RAKUJIN";

    public string NameEntryCharacterSet { get => nameEntryCharacterSet; }
    public int NameEntryMaxLength { get => nameEntryMaxLength; }
    public string DefaultHighScoreName {  get => defaultHighScoreName; }

    public List<Grade> StageGrades { get => stageGrades; }
    public Grade StageFailedGrade { get => stageFailedGrade; }

    [Header("Menu Timer Properties")]
    [SerializeField] private bool enableMenuTimer = true;
    [SerializeField] private int menuTime = 20;
    [SerializeField] private GameObject menuTimerGO;
    [SerializeField] private TMP_Text menuTimerCounterLabel;
    [SerializeField] private Image menuTimerBackground;
    // [SerializeField] private Color timerNormalColor, timerDangerColor;
    [SerializeField] private int timerTickThreshold = 5;
    [SerializeField] private StudioEventEmitter timerTickEmitter;
    [Tooltip("The list of Scene names to disable the Menu Timer from. By default, it is enabled on every Scene.")]
    [SerializeField] private List<string> disableTimerFromScenes;
    private int menuTimer;
    private float clockTimer;
    private readonly int timePerTick = 1;
    // private Vector3 startCameraPos;
    public string NextSceneName { get; set; }

    public bool TimerEnabled { get; set; }
    private bool canConfirm = true;
    public Coroutine CurrentSceneChange {  get; private set; }

    public int MenuTimer
    {
        get { return menuTimer; }
        set
        {
            menuTimer = value;
            menuTimerCounterLabel.text = menuTimer.ToString().PadLeft(2, '0');
            if (menuTimer <= 0)
            {
                menuTimer = 0;
                TimerEnabled = false;
                DoMenuLogic();
            }
            else if (menuTimer <= timerTickThreshold)
            {
                // menuTimerCounterLabel.color = timerDangerColor;
                if (menuTimerGO.activeInHierarchy)
                    timerTickEmitter.Play();

                Animator timerAnimator;
                if (timerAnimator = menuTimerGO.GetComponent<Animator>())
                {
                    timerAnimator.SetTrigger("Hurry Up");
                }
                
            }
            else
            {
                // menuTimerCounterLabel.color = timerNormalColor;
            }
        }
    }


    /// <summary>
    /// The stats from all the stages played on this game so far.
    /// </summary>
    public List<StageStats> GameStageStats { get; private set; }
    public List<PlayerInfo> JoinedPlayers { get; private set; }
    public PlayerInputManager PIM { get; private set; }
    [SerializeField] private GameObject uiPlayerPrefab;

    public bool InTransition { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            // Already existing instance: renew it
            Destroy(Instance.gameObject);
        }

        // Store this instance's reference, making it persistent between scenes
        DontDestroyOnLoad((Instance = this).gameObject);

        menuTimerGO.SetActive(false);
        GameStageStats = new();
        JoinedPlayers = new();
        canConfirm = true;
        InTransition = false;
        fadeTransitionOverlay.color = new Color(0f, 0f, 0f, 0f);
        PIM = GetComponent<PlayerInputManager>();
        PIM.playerPrefab = uiPlayerPrefab;

        StartCoroutine(NextAttractSceneCo());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // startCameraPos = Camera.main.transform.position;
    }

    public void OnConfirm(InputValue iv)
    {
        if (iv.isPressed && canConfirm)
        {
            canConfirm = false;
            switch (SceneManager.GetActiveScene().name)
            {
                case "CompanyLogo":
                case "GameOver":
                case "Leaderboard":
                    GoToScene(GetNextSceneName());
                    break;
                case "Title":
                    GameObject menu = GameObject.FindGameObjectWithTag("Menu");
                    if (menu ? menu.GetComponent<Animator>() : false)
                        menu.GetComponent<Animator>().SetTrigger("Confirm");
                    GoToScene(GetNextSceneName());
                    break;
                case "Menu":
                    break;
                case "Evaluation":
                    StageStats lastStageStats = GameStageStats.LastOrDefault();
                    bool highScore = false;
                    if (lastStageStats != null)
                    {
                        highScore = LocalScoresManager.Instance.IsNewRecord(lastStageStats.TotalScore);//lastStageStats.Result == StageResult.Cleared;
                    }
                    GoToScene(highScore ? "NameEntry" : "GameOver");
                    break;
                default:
                    return;
            }

            // Next scene
            MenuTimer = 0;

            if (confirmEmitter) confirmEmitter.Play();
        }
    }

    public void OnMoveCursor(InputValue iv)
    {
        Vector2 cursorDir = iv.Get<Vector2>();
        if (cursorDir.magnitude >= 0.4f)
        {
            MoveMenuCursor(cursorDir);
        }
    }

    private void MoveMenuCursor(Vector2 direction)
    {
        InteractableMenu currentMenu;
        if (currentMenu = FindFirstObjectByType<InteractableMenu>())
        {
            // currentMenu.MenuItemIndex += Mathf.CeilToInt(direction.x);
        }
        else
        {
            // Advance Attract sequence
            if (IsOnAttractSequence())
                GoToScene(GetNextAttractScene());
        }
    }
    /*
    public void OnReset(InputValue iv)
    {
        if (iv.isPressed)
        {
            SceneManager.LoadScene("Init");
        }
    }
    */

    public void OnFastForward(InputValue iv)
    {
#if UNITY_EDITOR
        Time.timeScale = iv.isPressed ? 3f : 1f;
#endif
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
                // print(asi.normalizedTime);
                if (asi.IsTag("Out") && asi.normalizedTime >= 1)
                {
                    // StartCoroutine(NextAttractSceneCo());
                    GoToScene(GetNextAttractScene(), 0f);
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

    public void SetupMenuTimer(int menuTime, bool startEnabled = true, bool visible = true)
    {
        if (enableMenuTimer)
        {
            clockTimer = 0;
            MenuTimer = menuTime;
            TimerEnabled = startEnabled;
            menuTimerGO.SetActive(visible);
        }
    }

    public string GetNextSceneName()
    {
        string nextScene = "";

        switch (SceneManager.GetActiveScene().name)
        {
            case "CompanyLogo":
                nextScene = "Title";
                break;
            case "Title":
            case "Leaderboard":
                nextScene = "Menu";
                break;
            case "Menu":
                nextScene = "Gameplay";
                break;
            case "Gameplay":
                nextScene = "Evaluation";
                break;
            case "Evaluation":
            case "NameEntry":
                nextScene = "GameOver";
                break;
            case "GameOver":
                nextScene = "CompanyLogo";
                break;
        }

        return nextScene;
    }

    public void DoMenuLogic()
    {
        InteractableMenu currentMenu = FindFirstObjectByType<InteractableMenu>();


        if (currentMenu)
        {
            /*
            if (currentMenu.CurrentMenuItem)
                currentMenu.CurrentMenuItem.OnConfirm();
            //StartCoroutine(ScreenOutCo());

            if (SceneManager.GetActiveScene().name == "Menu")
            {
                // Avoid new players to join
                pim.DisableJoining();
            }
            */
            currentMenu.DoTimeoutAction();
        }
        else
        {
            if (IsOnAttractSequence())
            {
                GoToScene(GetNextAttractScene());
            }
        }
        /*
        else
        {
            // StartCoroutine(ScreenOutCo(shouldAdvance: true));
        }
        */
    }

    public void GoToScene()
    {
        // print(NextSceneName);
        GoToScene(string.IsNullOrEmpty(NextSceneName) 
            ? GetNextSceneName() 
            : NextSceneName);
        NextSceneName = string.Empty;
    }

    public void GoToScene(string targetScene, float duration = 1f)
    {
        CurrentSceneChange ??= StartCoroutine(ScreenOutCo(targetScene, duration));
        if (targetScene == "Menu")
        {
            GameStageStats.Clear();
        }
    }

    public IEnumerator ScreenOutCo(string targetScene = "", float duration = 1f, bool shouldAdvance = false)
    {
        try
        {
            if (string.IsNullOrEmpty(targetScene)
                && (!IsOnAttractSequence() || shouldAdvance))
                targetScene = GetNextSceneName();

            // print(targetScene);
            TimerEnabled = false;
            float t = 0;

            // Do music fade-out
            SetMusicStatus(false);

            while (t < 1 && duration > 0)
            {
                fadeTransitionOverlay.color = new Color(0f, 0f, 0f, t);
                yield return new WaitForEndOfFrame();
                t += Time.unscaledDeltaTime / duration;
            }

            fadeTransitionOverlay.color = Color.black;

            if (targetScene.Contains("Gameplay"))
            {
                FindObjectsByType<PlayerInput>(0).ToList().ForEach(
                    pi => {
                        JoinedPlayers.Add(new(pi.playerIndex, pi.devices.FirstOrDefault()));
                        Destroy(pi.gameObject);
                    }
                );

            }
            else
            {
                PIM.playerPrefab = uiPlayerPrefab;
            }
            // Disable UI input in-game
            //GetComponent<PlayerInput>().enabled = !targetScene.Contains("Gameplay");
            //print(GetComponent<PlayerInput>().enabled);

            if (string.IsNullOrEmpty(targetScene))
            {
                // Next attract scene
                yield return NextAttractSceneCo();
            }
            else
            {
                // Specified scene
                yield return SceneManager.LoadSceneAsync(targetScene);

                if (!disableTimerFromScenes.Contains(targetScene)) 
                    SetupMenuTimer(
                        menuTime,
                        visible: !attractSequenceScenes.Contains(targetScene)
                    );
            }

            fadeTransitionOverlay.color = new Color(0f, 0f, 0f, 0f);
            SetMusicStatus(true);
            canConfirm = true;
        }
        finally
        {
            // Make sure to set this coroutine as finished
            CurrentSceneChange = null;
        }
    }

    public IEnumerator NextAttractSceneCo()
    {
        yield return SceneManager.LoadSceneAsync(GetNextAttractScene());
    }

    public string GetNextAttractScene()
    {
        int nextSceneIndex = attractSequenceScenes.IndexOf(SceneManager.GetActiveScene().name) + 1;
        if (nextSceneIndex >= attractSequenceScenes.Count)
        {
            nextSceneIndex = 0;
        }

        return attractSequenceScenes[nextSceneIndex];
    }

    public void SendEndStage(StageStats stats)
    {
        StartCoroutine(SendEndStageCo(stats));
    }

    public void SetMusicStatus(bool state = true)
    {
        // print($"Music status set to {state}");
        musicFadeoutCommand.gameObject.SetActive(state);
    }

    private IEnumerator SendEndStageCo(StageStats stats)
    {
        bool success = stats.Result == StageResult.Cleared;

        // Do music fade-out
        SetMusicStatus(false);

        GameObject stageEndTransition = success ? stageClearedTransition : stageFailedTransition;

        GameObject newTransition = Instantiate(stageEndTransition, stageEndTransition.transform.parent);
        newTransition.SetActive(true);
        Animator anim = newTransition.GetComponent<Animator>();

        while (anim && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            yield return new WaitForEndOfFrame();

        GameStageStats.Add(stats);
        GameManager.Instance.DestroySceneLeftovers();
        yield return SceneManager.LoadSceneAsync("Evaluation");

        // Camera.main.transform.position = startCameraPos;
        Time.timeScale = 1.0f;

        ScoreEntry newEntry = new()
        {
            Score = stats.TotalScore
        };

        SetMusicStatus(true);
        if (anim) anim.SetTrigger("End");
    }


    public int GetCurrentGameScore()
    {
        return GameStageStats.Sum(gss => gss.TotalScore);
    }

    public void CheckForPlayerJoining()
    {
        PlayerInputManager pim = GetComponent<PlayerInputManager>();
    }

    public void EnableUIInput()
    { 
        PIM.playerPrefab = uiPlayerPrefab;
        foreach (PlayerInfo pi in JoinedPlayers)
        {
            PIM.JoinPlayer(pi.playerIndex, pairWithDevice: pi.device);
        }
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
