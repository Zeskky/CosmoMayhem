using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NameEntryPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameInputLabel;
    [SerializeField] private GameObject characterGO;
    [SerializeField] private Transform characterSetContainer;
    private string enteredName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupCharacterSet();
        print(EventSystem.current.currentSelectedGameObject);
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

        characterSetContainer.GetChild(1).GetComponent<Selectable>().Select();
    }

    public bool TypeCharacter(string c)
    {
        if (enteredName.Length >= Launcher.Instance.NameEntryMaxLength) 
            return false;

        enteredName += c;
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        nameInputLabel.text = enteredName;
    }
}
