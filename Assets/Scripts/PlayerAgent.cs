using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    PlayerControl control;
    [SerializeField] BoxCollider2D bodyCollider;
    [SerializeField] BoxCollider2D bulletDodgeTracker;
    [SerializeField] float stayAliveReward = 0.01f;
    [SerializeField] float deathReward = -1f;
    
    UIManager uIManager;

    int rewards_queued = 0;
    Vector2 minBounds;
    Vector2 maxBounds;
    float startY, startX;
    BulletSpawner[] spawners;
    Rigidbody2D rb;
    [SerializeField] int maxBulletsTracked = 4;
    
    void Start(){
        EnvBounds envBounds = GetComponentInParent<EnvBounds>();
        startX = transform.position.x;
        startY = transform.position.y;
        Camera mainCamera = Camera.main;
        minBounds = mainCamera.ViewportToWorldPoint(new Vector2(0.1f, 0));
        maxBounds = mainCamera.ViewportToWorldPoint(new Vector2(0.5f, 1));
        rb = GetComponent<Rigidbody2D>();
        spawners = envBounds.GetSpawner();
        control = GetComponent<PlayerControl>();
        uIManager = FindObjectOfType<UIManager>();
        
    }
    public override void OnEpisodeBegin()
    {
        foreach (BulletSpawner spawner in spawners){
            spawner.Reset();
        }
        transform.position = new Vector2(startX, startY);
        // new Vector2(Random.Range(minBounds.x, maxBounds.x), startY);
        control.SetAlive();

    }

    public override void OnActionReceived(ActionBuffers actions){
        /*
        Debug.Log(actions.DiscreteActions[0] + ", " + actions.DiscreteActions[1]);
        int moveInput = actions.DiscreteActions[0];
        int jumpControl = actions.DiscreteActions[1];
        if (moveInput == 2){
            control.Move(-1);
        }
        else if (moveInput == 1){
            control.Move(1);
        }
        else{
            control.Move(0);
        }
        */
        int jumpControl = actions.DiscreteActions[0];
        if (jumpControl != 0){
            control.Jump();
        }
        else{
            control.JumpSpeedCancel();
        }
        if (rewards_queued > 0){
            uIManager.AddScore(rewards_queued);
            AddReward(rewards_queued);
            rewards_queued = 0;
        }
        if (control.IsAlive()){
            AddReward(stayAliveReward);
        }
        else{
            AddReward(deathReward);
            EndEpisode();

        }
        uIManager.UpdateSteps();
        if (StepCount == MaxStep){
            Debug.Log("Total Reward Finished: " + GetCumulativeReward());
        }
        // Debug.Log("Total Steps: " + StepCount);
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        // sensor.AddObservation(transform.localPosition.x/5f);
        sensor.AddObservation(transform.localPosition.y/3f);
        // sensor.AddObservation(rb.velocity.x/control.MaxSpeed());
        sensor.AddObservation(rb.velocity.y/control.MaxSpeed());

        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.1f, 12), 0, Vector2.right, 20, LayerMask.GetMask("Bullet"));
        int hitsEncountered = 0;
        for (int i = 0; i < hits.Length; i++){
            RaycastHit2D h = hits[i];
            if (h.collider != null){
                sensor.AddObservation(h.fraction);
                Vector2 relative = (Vector2) transform.position - h.point;
                relative.Normalize();
                sensor.AddObservation((Vector2)relative);
                hitsEncountered += 1;
                if (hitsEncountered == maxBulletsTracked)
                    break;
            }
        }
        for (int i=hitsEncountered; i < maxBulletsTracked; i ++){
            sensor.AddObservation(1.0f);
            sensor.AddObservation(new Vector2(-1, 0));
        }
        
        /*
        float[] array = new float[8];
        GetObservations().CopyTo(array, 0);
        Debug.Log(string.Join(", ", array));
        */
        
    }

    public void OnTriggerExit2D(Collider2D other){
        if (other.tag == "Bullet"){
            rewards_queued += 1;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut){
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        /*int movementControl = (int) Input.GetAxisRaw("Horizontal");
        if (movementControl == -1){
            movementControl = 2;
        }
        actions[0] = movementControl;
        actions[1] = (int) Input.GetAxisRaw("Jump");
        */
        actions[0] = (int) Input.GetAxisRaw("Jump");
    }
    
}
