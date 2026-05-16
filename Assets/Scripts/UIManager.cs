using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dodgedCount;
    [SerializeField] TextMeshProUGUI stepsCount;
    [SerializeField] TextMeshProUGUI deathCount;

    string dodgedPrefix, stepsPrefix, deathPrefix;
    int score, steps, deaths = 0;


    // Start is called before the first frame update
    void Awake()
    {
        dodgedPrefix = dodgedCount.text;
        stepsPrefix = stepsCount.text;
        deathPrefix = deathCount.text;
    }
    void Start(){
        UpdateUI();
    }

    public void UpdateUI(){
        dodgedCount.text = dodgedPrefix + score;
        stepsCount.text = stepsPrefix + steps;
        deathCount.text = deathPrefix + deaths;
    }

    public void AddDeath(){
        deaths += 1;
        UpdateUI();
    }
    public void AddScore(int addScore){
        score += addScore;
        UpdateUI();
    }

    public void UpdateSteps(){
        steps += 1;
        if (steps % 10 == 0){
            UpdateUI();
        }
    }


}
