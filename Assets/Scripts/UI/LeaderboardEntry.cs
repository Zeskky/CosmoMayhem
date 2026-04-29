using TMPro;
using UnityEngine;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text placeLabel, nameLabel, scoreLabel;
    [SerializeField] private Color firstColor, secondColor, thirdColor;

    public void UpdateEntryData(string name, int score)
    {
        int place = transform.GetSiblingIndex() + 1;
        placeLabel.text = place.ToString();
        // Set the place color label depending on the rank
        placeLabel.color = place switch
        {
            1 => firstColor,
            2 => secondColor,
            3 => thirdColor,
            _ => Color.white,
        };

        nameLabel.text = name;
        scoreLabel.text = $"{score,8}";
    }
}
