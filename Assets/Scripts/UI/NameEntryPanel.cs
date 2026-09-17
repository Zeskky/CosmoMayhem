using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NameEntryPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameInputLabel, finalScoreLabel;
    [SerializeField] private GameObject characterGO;
    [SerializeField] private Transform characterSetContainer;
    [SerializeField] private List<Transform> lastCharacters;
    [SerializeField] private StudioEventEmitter selectEventEmitter, submitEventEmitter;
    [SerializeField] private float charSpacing = 0.825f;
    private string enteredName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectEventEmitter.enabled = false; // Avoiding the initial selection sound

        string monospaceTag = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>";
        finalScoreLabel.text = $"{monospaceTag}{Launcher.Instance.GetCurrentGameScore()}";
        Launcher.Instance.SetupMenuTimer(30);
        SetupCharacterSet();
        print(EventSystem.current.currentSelectedGameObject);

        selectEventEmitter.enabled = true;
    }

    public void OnSelectedCharacterChange()
    {
        selectEventEmitter.Play();
    }

    private void SetupCharacterSet()
    {
        foreach (char c in Launcher.Instance.NameEntryCharacterSet)
        {
            GameObject newChar = Instantiate(characterGO, characterGO.transform.parent);
            newChar.GetComponent<TMP_Text>().text = c.ToString();
            newChar.GetComponent<NameCharacter>().NameEntryPanel = this;
            newChar.SetActive(true);
        }

        foreach (Transform t in lastCharacters)
        {
            t.SetAsLastSibling();
            t.GetComponent<NameCharacter>().NameEntryPanel = this;
        }

        characterSetContainer.GetChild(1).GetComponent<Selectable>().Select();
    }

    public bool DeleteLastCharacter()
    {
        if (enteredName.Length == 0)
            return false;

        selectEventEmitter.Play();

        // Substring the name before the last character
        enteredName = enteredName[..^1];
        return true;
    }

    /// <summary>
    /// Inserts the specified character on the current name input.
    /// </summary>
    /// <param name="c">The character to insert.</param>
    /// <returns>Returns TRUE if the character has been entered successfully. <br></br>Otherwise (i.e. has reached the name character limit), returns FALSE.</returns>
    public bool TypeCharacter(string c)
    {
        // Pre-type check
        if (enteredName.Length >= Launcher.Instance.NameEntryMaxLength) 
            return false;

        enteredName += c;
        submitEventEmitter.Play();

        // Post-type check
        if (enteredName.Length >= Launcher.Instance.NameEntryMaxLength)
            JumpToEnd();

        return true;
    }

    public void JumpToEnd()
    {
        characterSetContainer
            .GetChild(characterSetContainer.childCount - 1)
            .GetComponent<Selectable>().Select();
    }

    public void EndInput()
    {
        submitEventEmitter.Play();
        characterSetContainer.gameObject.SetActive(false);
        Launcher.Instance.MenuTimer = 0;
    }

    public void FinishInput()
    {
        // Establish a default name if empty
        if (string.IsNullOrEmpty(enteredName))
        {
            enteredName = Launcher.Instance.DefaultHighScoreName;
        }

        submitEventEmitter.Play();

        // StageStats latestStageStats = Launcher.Instance.GameStageStats.LastOrDefault();
        LocalScoresManager.Instance.SubmitScoreEntry(new ScoreEntry()
        {
            PlayerName = enteredName,
            Score = Launcher.Instance.GetCurrentGameScore(),
        });

        // Update current leaderboard
        LeaderboardRenderer lr = FindAnyObjectByType<LeaderboardRenderer>();
        if (lr) lr.RenderLeaderboard();

        StartCoroutine(PostInputCo());
    }

    private IEnumerator PostInputCo()
    {
        yield return new WaitForSecondsRealtime(3f);
        Launcher.Instance.GoToScene();
    }

    // Update is called once per frame
    void Update()
    {
        nameInputLabel.text = enteredName;
    }
}
