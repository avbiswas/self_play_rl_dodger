using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MovablePlayerAgent : Agent
{
    PlayerControl control;
    [SerializeField] CapsuleCollider2D bodyCollider;
    [SerializeField] BoxCollider2D bulletDodgeTracker;
    [SerializeField] float stayAliveReward = 0.01f;
    [SerializeField] float deathReward = -1f;
    [SerializeField] Transform ground;
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
        control.SetAlive();

    }

    public override void OnActionReceived(ActionBuffers actions){
        uIManager.UpdateSteps();
        // First input is for movement (stay still, left, right)
        int moveInput = actions.DiscreteActions[0];

        // Second input is for jumping
        int jumpControl = actions.DiscreteActions[1];

        if (moveInput == 2){
            control.Move(-1); // Move Left
        }
        else if (moveInput == 1){
            control.Move(1); // Move right
        }
        else{
            control.Move(0); // Stay Still
        }
        if (jumpControl != 0){
            control.Jump(); // Jump
        }
        else{
            control.JumpSpeedCancel();  // Cancel High Jump
        }
        if (rewards_queued > 0){
            AddReward(rewards_queued);  // Total bullets dodged since last step
            uIManager.AddScore(rewards_queued);

            rewards_queued = 0;
        }
        if (control.IsAlive()){
            AddReward(stayAliveReward);   // Staying Alive
        }
        else{

            AddReward(deathReward); // death penalty!
            uIManager.AddDeath();
            Debug.Log("Total Reward: " + GetCumulativeReward());

            EndEpisode();

        }

        if (StepCount == MaxStep){
            Debug.Log("Total Reward Finished: " + GetCumulativeReward());
        }
        // Debug.Log("Reward: " + GetCumulativeReward());
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(transform.localPosition.x/5f);
        sensor.AddObservation(transform.localPosition.y/3f);

        // Agent velocity along x, y axis normalized by max speed
        sensor.AddObservation(rb.velocity.x/control.MaxSpeed());
        sensor.AddObservation(rb.velocity.y/control.MaxSpeed());


        // Send BoxCast in front of player
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.1f, 12), 0, Vector2.right, 20, LayerMask.GetMask("Bullet"));
        int hitsEncountered = 0;
        for (int i = 0; i < hits.Length; i++){
            RaycastHit2D h = hits[i];
            if (h.collider != null){

                // Fraction of the distance travelled by ray before hitting the object
                sensor.AddObservation(h.fraction);

                // Relative distance normalized
                Vector2 relative = (Vector2) transform.position - h.point;
                relative.Normalize();
                sensor.AddObservation((Vector2)relative);
                hitsEncountered += 1;
                if (hitsEncountered == maxBulletsTracked)
                    break;
            }
        }
        for (int i=hitsEncountered; i < maxBulletsTracked; i ++){

            // Padding for not hitting object, i.e. distance fraction is = 1
            sensor.AddObservation(1.0f);

            // Padding for bullet direction when nothing was hit
            sensor.AddObservation(new Vector2(-1, 0));
        }
        
        /*
        float[] array = new float[12];
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
        int movementControl = (int) Input.GetAxisRaw("Horizontal");
        if (movementControl == -1){
            movementControl = 2;
        }
        actions[0] = movementControl;
        actions[1] = (int) Input.GetAxisRaw("Jump");
        
    }
    
}
