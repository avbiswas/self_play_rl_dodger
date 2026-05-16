using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    
    [SerializeField] GameObject p1Lives;
    [SerializeField] GameObject p2Lives;
    
    [SerializeField] GameObject p1Hearts;
    [SerializeField] GameObject p2Hearts;

    EnvironmentManagerRayCast gameManager;
    string p1dodgedPrefix, p1killPrefix, p1deathPrefix, p1FireTimerPrefix;
    string p2dodgedPrefix, p2killPrefix, p2deathPrefix, p2FireTimerPrefix;
    int p1score, p1kill, p1deaths = 0;
    int p2score, p2kill, p2deaths = 0;
    float p1Timer, p2Timer;
    int steps = 0;

    // Start is called before the first frame update
    void Awake()
    {
 
    }
    void Start(){
        gameManager = FindObjectOfType<EnvironmentManagerRayCast>();
        UpdateUI();
    }

    public void UpdateUI(){
        if (gameManager == null){
            gameManager = FindObjectOfType<EnvironmentManagerRayCast>();
        }
        int p1_lives = gameManager.P1_Lives();
        int playerHeartsRendered = p1Lives.transform.childCount;
        // Debug.Log(playerHeartsRendered + ", " + p1_lives);
        if (p1_lives != playerHeartsRendered){
            foreach (Transform child in p1Lives.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < p1_lives; i++){
                Instantiate(p1Hearts, p1Lives.transform);
            }
        }

        int p2_lives = gameManager.P2_Lives();
        int playerHeartsRendered2 = p2Lives.transform.childCount;
        // Debug.Log(playerHeartsRendered + ", " + p1_lives);
        if (p2_lives != playerHeartsRendered2){
            foreach (Transform child in p2Lives.transform)
            {
                Destroy(child.gameObject);
            }
            for (int i = 0; i < p2_lives; i++){
                Instantiate(p2Hearts, p2Lives.transform);
            }
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
