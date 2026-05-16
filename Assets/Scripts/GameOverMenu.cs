using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.MLAgents;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] TextMeshProUGUI dodged;
    [SerializeField] TextMeshProUGUI kills;
    [SerializeField] TextMeshProUGUI deaths;
    [SerializeField] TextMeshProUGUI levels;

    string dodgedText, killsText, deathsText, levelsText;
    EnvironmentManagerRayCast env;

    void Start(){
        dodgedText = dodged.text;
        killsText = kills.text;
        deathsText = deaths.text;
        levelsText = levels.text;
        env = GetComponent<EnvironmentManagerRayCast>();
    }

    void FixedUpdate(){
        if (Academy.Instance.IsCommunicatorOn){
            return;
        }
        if (env.P1_Lives() <= 0){
            GameOver();
        }
    }
    // Update is called once per frame
    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverMenu.SetActive(true);
        dodged.text = dodgedText + env.GetDodgeScore();
        kills.text = killsText + env.GetKills();
        deaths.text = deathsText + env.GetDeaths();
        levels.text = levelsText + env.GetLevel();
    }
}
