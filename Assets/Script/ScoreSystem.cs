using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreSystem : MonoBehaviour
{

    public TMP_Text ScoreText;

    private int score = 0;

    public void upScore()
    {
        score += 1;
        ScoreText.SetText(score.ToString());
    }

}
