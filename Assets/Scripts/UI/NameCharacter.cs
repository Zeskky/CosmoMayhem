using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NameCharacter : MonoBehaviour, ISelectHandler, ISubmitHandler
{
    [SerializeField] private EventReference selectEventRef, submitEventRef;
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
        RuntimeManager.PlayOneShot(selectEventRef);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // Make sure the name does not surpass the input limit
        if (NameEntryPanel.TypeCharacter(GetComponent<TMP_Text>().text))
        {
            RuntimeManager.PlayOneShot(submitEventRef);
        }
    }
}
