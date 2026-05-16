using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField] ShootPlayerAgent p1;
    [SerializeField] ShootPlayerAgent p2;
    UIManager2 uIManager;
    // Start is called before the first frame update
    PlayerControl p1C;
    PlayerControl p2C;
    [SerializeField] float deathReward = -1f;
    [SerializeField] float winReward = 1f;
    void Start(){
        p1C = p1.gameObject.GetComponent<PlayerControl>();
        p2C = p2.gameObject.GetComponent<PlayerControl>();
        uIManager = FindObjectOfType<UIManager2>();
    }

    void EndGameScoring(PlayerControl deadC, ShootPlayerAgent deadS,
        PlayerControl aliveC, ShootPlayerAgent aliveS){
            deadS.SetReward(deathReward);
            aliveS.SetReward(winReward);
                
            uIManager.AddDeath(deadS.team_id);            
            string deathreason = deadC.DeathReason().ToString();
            if (deathreason == "bullet"){
                uIManager.AddKill(aliveS.team_id);

            }
            Debug.Log("Total Reward for Player " + deadS.team_id + " is : "  + deadS.GetCumulativeReward());
            Debug.Log("Total Reward for Player " + aliveS.team_id + " is : "  + aliveS.GetCumulativeReward());
            deadS.EndEpisode();
            aliveS.EpisodeInterrupted();
            
        }
    // Update is called once per frame
    public void Resolve(int team_id)
    {
        if (!p1C.IsAlive() && !p2C.IsAlive()){
            p1.EndEpisode();
            p2.EndEpisode();
        }
        if (!p1C.IsAlive()){
            EndGameScoring(p1C, p1, p2C, p2);
        }
        else if (!p2C.IsAlive()){
            EndGameScoring(p2C, p2, p1C, p1);
        }
    }
}
