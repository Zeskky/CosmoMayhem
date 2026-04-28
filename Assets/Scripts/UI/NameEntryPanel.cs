using TMPro;
using UnityEngine;

public class NameEntryPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text nameInputLabel;
    [SerializeField] private GameObject characterGO;
    [SerializeField] private Transform characterSetContainer;
    private string enteredName = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void SetupCharacterSet()
    {
        foreach (char c in Launcher.Instance.NameEntryCharacterSet)
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        nameInputLabel.text = enteredName;
    }
}
