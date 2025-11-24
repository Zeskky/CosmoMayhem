using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Grade
{
    [SerializeField] private float targetScoreRatio;
    public float TargetScoreRatio { get { return targetScoreRatio; } }

    [SerializeField] private Sprite gradeSprite;
    public Sprite GradeSprite { get { return gradeSprite; } }
}

public class StageGrade : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StageStats ss = Launcher.Instance.GameStageStats.LastOrDefault();
        Grade grade = ss?.GetStageGrade();
        if (grade != null)
        {
            Image gradeImage = gameObject.GetComponent<Image>();
            if (gradeImage)
            {
                gradeImage.sprite = grade.GradeSprite;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
