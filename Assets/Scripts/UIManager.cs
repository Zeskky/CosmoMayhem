using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private float charSpacing = 0.825f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        scoreLabel.text = $"<mspace={charSpacing.ToString().Replace(',', '.')}em>{GameManager.Instance.score.ToString().PadLeft(8, '0')}";
    }
}
