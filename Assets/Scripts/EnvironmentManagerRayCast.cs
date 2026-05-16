using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class EnvironmentManagerRayCast : MonoBehaviour
{
   
    ShootRayCastAgent p1;
    ShootRayCastAgent p2;
    HUDManager uIManager;
    // Start is called before the first frame update
    PlayerControl p1C;
    PlayerControl p2C;
    [SerializeField] float deathReward = -1f;
    [SerializeField] float winReward = 1f;
    [SerializeField] float dodgeReward = 0;
    int bullets_dodged = 0;
    int deaths = 0;
    int kills = 0;

    void Start(){
        ShootRayCastAgent[] ps = GetComponentsInChildren<ShootRayCastAgent>();
        foreach (ShootRayCastAgent p in ps){
            if (p.gameObject.name == "AI"){
                p2 = p;
            }
            else{
                p1 = p;
            }
        }    
        p1C = p1.gameObject.GetComponent<PlayerControl>();
        p2C = p2.gameObject.GetComponent<PlayerControl>();
        uIManager = FindObjectOfType<HUDManager>();
        Debug.Log("P1 is " + p1C.gameObject.name);
        Debug.Log("P2 is " + p2C.gameObject.name);
        bullets_dodged = 0;

    }

    public int P1_Lives(){
        if (p1C == null){
            Start();
        }
        return p1C.NumLives();
    }

    public int P2_Lives(){
        if (p2C == null){
            Start();
        }
        return p2C.NumLives();
    }
    void EndGameScoring(PlayerControl deadC, ShootRayCastAgent deadS,
        PlayerControl aliveC, ShootRayCastAgent aliveS){
            deadS.SetReward(deathReward);
            aliveS.SetReward(winReward);
            string deathreason = deadC.DeathReason().ToString();

            if (uIManager != null){
                uIManager.AddDeath(deadS.team_id); 
                if (deathreason == "bullet"){
                    uIManager.AddKill(aliveS.team_id);

                }           
            }
            
            Debug.Log("Total Reward for Player " + deadS.team_id + " is : "  + deadS.GetCumulativeReward());
            Debug.Log("Total Reward for Player " + aliveS.team_id + " is : "  + aliveS.GetCumulativeReward());
            deadS.EndEpisode();
            aliveS.EpisodeInterrupted();
            
        }
    // Update is called once per frame

    public void register_bullet_dodge(int team_id){
        if (p1.team_id == team_id){
            bullets_dodged += 1;
            p1.AddReward(dodgeReward);
            if (uIManager != null){
                uIManager.AddDodge(p1.team_id, 1);
            }
        }
        else{
            p2.AddReward(dodgeReward);
            if (uIManager != null){
                uIManager.AddDodge(p2.team_id, 1);
            }
        }
    }
    public void Resolve(int team_id)
    {
        if (!p1C.IsAlive() && !p2C.IsAlive()){
            p1.EndEpisode();
            p2.EndEpisode();
        }
        if (!p1C.IsAlive()){
            deaths += 1;
            EndGameScoring(p1C, p1, p2C, p2);
        }
        else if (!p2C.IsAlive()){
            kills += 1;
            EndGameScoring(p2C, p2, p1C, p1);
            GetComponent<Options>().ShowMenu();
        }
        
    }
    void Update(){
        if (Input.GetKey(KeyCode.M)){
            GetComponent<Options>().ShowMenu();
        }
    }

    public int GetDodgeScore(){
        return bullets_dodged;
    }

    public int GetKills(){
        return kills;
    }

    public int GetDeaths(){
        return deaths;
    }

    public int GetLevel(){
        return 0;
    }

}
