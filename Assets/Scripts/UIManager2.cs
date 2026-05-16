using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager2 : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI p1dodgedCount;
    [SerializeField] TextMeshProUGUI p1deathCount;
    [SerializeField] TextMeshProUGUI p1killCount;

    [SerializeField] TextMeshProUGUI p2dodgedCount;
    [SerializeField] TextMeshProUGUI p2deathCount;
    [SerializeField] TextMeshProUGUI p2killCount;

    [SerializeField] TextMeshProUGUI p1FireTimer;
    [SerializeField] TextMeshProUGUI p2FireTimer;

    

    string p1dodgedPrefix, p1killPrefix, p1deathPrefix, p1FireTimerPrefix;
    string p2dodgedPrefix, p2killPrefix, p2deathPrefix, p2FireTimerPrefix;
    int p1score, p1kill, p1deaths = 0;
    int p2score, p2kill, p2deaths = 0;
    float p1Timer, p2Timer;
    int steps = 0;

    // Start is called before the first frame update
    void Awake()
    {
        p1dodgedPrefix = p1dodgedCount.text;
        p1killPrefix = p1killCount.text;
        p1deathPrefix = p1deathCount.text;
        p2dodgedPrefix = p2dodgedCount.text;
        p2killPrefix = p2killCount.text;
        p2deathPrefix = p2deathCount.text;
        if (p2FireTimer != null){
            p1FireTimerPrefix = p1FireTimer.text;
            p2FireTimerPrefix = p2FireTimer.text;

        }

    }
    void Start(){
        UpdateUI();
    }

    public void UpdateUI(){
        p1dodgedCount.text = p1dodgedPrefix + p1score;
        p1deathCount.text = p1deathPrefix + p1deaths;
        p1killCount.text = p1killPrefix + p1kill;
        p2dodgedCount.text = p2dodgedPrefix + p2score;
        p2deathCount.text = p2deathPrefix + p2deaths;
        p2killCount.text = p2killPrefix + p2kill;
        if (p1FireTimer != null){
            string p1TimerText, p2TimerText;
            p1TimerText = p1Timer.ToString("F2");
            p2TimerText = p2Timer.ToString("F2");
            if (p1Timer < 2){
                p1TimerText = "<color=\"red\">"+p1TimerText+"</color>";
            }
            if (p2Timer < 2){
                p2TimerText = "<color=\"red\">"+p2TimerText+"</color>";
            }
            p1FireTimer.text = p1FireTimerPrefix + p1TimerText;
            p2FireTimer.text = p2FireTimerPrefix + p2TimerText;
        }
    }

    public void UpdateTimer(int team_id, float timer){
        if (team_id == 1)
            p1Timer = timer;
        else
            p2Timer = timer;
        UpdateUI();

    }
    public void AddDeath(int team_id){
        if (team_id == 1)
            p1deaths += 1;
        else
            p2deaths += 1;
        UpdateUI();
    }
    public void AddDodge(int team_id, int addScore){
        if (team_id == 1)
            p1score += addScore;
        else
            p2score += addScore;
        UpdateUI();
    }

    public void AddKill(int team_id){
        if (team_id == 1){
            p1kill += 1;
        }
        else{
            p2kill += 1;
        }
        UpdateUI();
    }

    public void FixedUpdate(){
        steps += 1;
        if (steps % 10 == 0){
            UpdateUI();
        }
    }


}
