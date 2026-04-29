using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NameCharacter : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    // [SerializeField] private EventReference selectEventRef, submitEventRef;
    public NameEntryPanel NameEntryPanel { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        //RuntimeManager.PlayOneShot(selectEventRef);
        NameEntryPanel.OnSelectedCharacterChange();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        string textValue = GetComponent<TMP_Text>().text;

        switch (textValue)
        {
            case "DEL":
                NameEntryPanel.DeleteLastCharacter();
                /*
                if (NameEntryPanel.DeleteLastCharacter())
                    RuntimeManager.PlayOneShot(selectEventRef);
                */
                break;
            case "END":
                NameEntryPanel.EndInput();
                //RuntimeManager.PlayOneShot(submitEventRef);
                break;
            default:
                // Make sure the name does not surpass the input limit
                if (NameEntryPanel.TypeCharacter(textValue))
                {
                    //RuntimeManager.PlayOneShot(submitEventRef);
                }
                else
                {
                    // Jump to the 'END' button if reached the name length cap
                    NameEntryPanel.JumpToEnd();
                }
                break;
        }

    }
}
