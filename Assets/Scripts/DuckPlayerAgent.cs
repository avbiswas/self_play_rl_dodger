using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DuckPlayerAgent : Agent
{
    PlayerControl control;
    [SerializeField] BoxCollider2D bulletDodgeTracker;
    [SerializeField] float stayAliveReward = 0.01f;
    [SerializeField] float energyPenalty = -0.05f;
    [SerializeField] float strayingPenalty = -0.05f;
    [SerializeField] float deathReward = -1f;
    UIManager uIManager;

    int rewards_queued = 0;
    float startY, startX;
    float localStartX;
    Rigidbody2D rb;
    [SerializeField] int maxBulletsTracked = 4;
    [SerializeField] int maxBackBulletsTracked = 2;
    [SerializeField] GameObject shootPoint;
    Vector2 facingCorrection; 
    [SerializeField] float dashPenalty;
    [SerializeField] SmartBulletSpawner bulletSpawner;
    CapsuleCollider2D bodyCollider;
    int numSteps = 0;
    void Start(){
        startX = transform.position.x;
        localStartX = transform.localPosition.x;
        startY = transform.position.y;
        rb = GetComponent<Rigidbody2D>();
        control = GetComponent<PlayerControl>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        uIManager = FindObjectOfType<UIManager>();
        facingCorrection = new Vector2(
            control.GetFacingDirection(), 0);
        
        
    }
    public override void OnEpisodeBegin()
    {
        numSteps = 0;
        transform.position = new Vector2(startX, startY);
        rb.velocity = new Vector2(0, 0);
        control.SetAlive();
        if (shootPoint != null){
            shootPoint.GetComponent<SmartBulletSpawner>().Reset();
        }
        bulletSpawner.Reset();
    }

    public override void OnActionReceived(ActionBuffers actions){
        uIManager.UpdateSteps();
        int moveInput = actions.DiscreteActions[0];
        int jumpControl = actions.DiscreteActions[1];
        int crouchControl = actions.DiscreteActions[2];
        int dashControl = actions.DiscreteActions[3];
        if (moveInput == 2){
            control.Move(-1 * (int) facingCorrection.x);
        }
        else if (moveInput == 1){
            control.Move(1  * (int) facingCorrection.x);
        }
        else{
            control.Move(0);
        }

        float distance_from_center = Mathf.Abs(localStartX - transform.localPosition.x);
        if (distance_from_center > 5){
            AddReward(-strayingPenalty);
        }

        float energy = Vector2.Dot(rb.velocity, rb.velocity)/100;
        float energy_reward = 0;
        if (energy > 0.4){
            energy_reward = energyPenalty * Mathf.Exp(-energy); // energyPenalty = -0.05
            energy_reward = Mathf.Min(energyPenalty, Mathf.Max(energy_reward, 0));
        }
        AddReward(energy_reward);


        if (jumpControl != 0){
            control.Jump();
        }
        else{
            control.JumpSpeedCancel();
        }
        
        control.Crouch(crouchControl);
        control.Dash(dashControl);

        if (dashControl == 1){
            AddReward(dashPenalty);
        }

        if (rewards_queued > 0){
            // rewards_queued = num bullets dodged since last update
            AddReward(rewards_queued);
            uIManager.AddScore(rewards_queued);

            rewards_queued = 0;
        }
        if (control.IsAlive()){
            AddReward(stayAliveReward); // Fixed reward for survival (0.01)
        }
        else{
            // if agent gets hit by bullet or falls down from platform
            // deathReward = -1
            AddReward(deathReward);
            uIManager.AddDeath();
            Debug.Log("Total Reward: " + GetCumulativeReward());

            EndEpisode();

        }

        if (StepCount == MaxStep){
            Debug.Log("Total Reward Finished: " + GetCumulativeReward());
        }
        
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation((transform.localPosition.x)/(control.maxX - control.minX) * facingCorrection.x);
        
        sensor.AddObservation(transform.localPosition.y/3f);
        sensor.AddObservation(rb.velocity.x/control.MaxSpeed() * facingCorrection.x);

        sensor.AddObservation(rb.velocity.y/control.MaxSpeed());
        sensor.AddObservation(control.IsCrouching());
        sensor.AddObservation(control.CanDash());
        sensor.AddObservation(control.IsDashing());
        sensor.AddObservation(control.TimeToNextDash());

        Vector2 distanceFromPlatformEdges = control.GetDistanceFromPlatformEdges();
        float distanceFromMinX = Mathf.Clamp(distanceFromPlatformEdges.x/(control.maxX - control.minX), -1.1f, 1.1f);
        float distanceFromMaxX = Mathf.Clamp(distanceFromPlatformEdges.y/(control.maxX - control.minX), -1.1f, 1.1f);
        sensor.AddObservation(distanceFromMaxX);
        sensor.AddObservation(distanceFromMinX);
        
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.1f, 12), 0, Vector2.right * facingCorrection, 40, 
            LayerMask.GetMask(control.adversarialBulletTag));
        int hitsEncountered = 0;
        for (int i = 0; i < hits.Length; i++){
            RaycastHit2D h = hits[i];
            if (h.collider != null){
                Vector2 relative = (Vector2) transform.position - h.point;
                relative.Normalize();
                
                
                Vector2 bulletVelocity = h.collider.gameObject.GetComponent<Rigidbody2D>().velocity;
                
                Vector2 closestPoint = (Vector2) bodyCollider.ClosestPoint(h.point) - h.point;
                float closestDistance = closestPoint.magnitude/40;
                closestPoint.Normalize();

                float ttc = control.ttc_to_ttd(control.ttc(h.collider.gameObject));
                float duckDistance = (control.GetCrouchableHeight() - (h.point.y - 1))/6f;

                sensor.AddObservation(h.fraction);
                sensor.AddObservation((Vector2)relative * facingCorrection);
                sensor.AddObservation(Mathf.Abs(bulletVelocity.x/20));
                sensor.AddObservation(closestDistance);
                sensor.AddObservation((Vector2)closestPoint * facingCorrection);
                sensor.AddObservation(ttc);
                sensor.AddObservation(duckDistance);
                //Debug.Log("Player " + team_id + ": " + h.fraction + ", " + (Vector2)relative * facingCorrection + ", " + Mathf.Abs(bulletVelocity.x/20) + ", " + closestDistance + ", " + (Vector2)closestPoint * facingCorrection + ", " + ttc);
                hitsEncountered += 1;
                if (hitsEncountered == maxBulletsTracked)
                    break;
            }
        }
        for (int i=hitsEncountered; i < maxBulletsTracked; i ++){
            sensor.AddObservation(1.0f);
            sensor.AddObservation(new Vector2(-1, 0));
            sensor.AddObservation(0);
            sensor.AddObservation(1.0f);
            sensor.AddObservation(new Vector2(-1, 0));
            sensor.AddObservation(0);
            sensor.AddObservation(-0.2f);
            
        }

        RaycastHit2D[] back_hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.1f, 12), 0, Vector2.left  * facingCorrection, 15, LayerMask.GetMask(control.adversarialBulletTag));
        int backHitsEncountered = 0;
        for (int i = 0; i < back_hits.Length; i++){
            RaycastHit2D h = back_hits[i];
            if (h.collider != null){
                Vector2 relative = (Vector2) transform.position - h.point;
                relative.Normalize();
                
                
                Vector2 bulletVelocity = h.collider.gameObject.GetComponent<Rigidbody2D>().velocity;

                float ttc = control.ttc(h.collider.gameObject);
                sensor.AddObservation(h.fraction);
                sensor.AddObservation((Vector2)relative * facingCorrection);
                sensor.AddObservation(Mathf.Abs(bulletVelocity.x/20));
                sensor.AddObservation(ttc);

                backHitsEncountered += 1;
                if (backHitsEncountered == maxBackBulletsTracked)
                    break;
            }
        }

        for (int i=backHitsEncountered; i < maxBackBulletsTracked; i ++){
            sensor.AddObservation(1.0f);
            sensor.AddObservation(new Vector2(-1, 0));
            sensor.AddObservation(0);
            sensor.AddObservation(1);
        }

    }


    public void OnTriggerExit2D(Collider2D other){
        if (other.tag == control.adversarialBulletTag){
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
        numSteps += 1 ;

        actions[1] = (int) Input.GetAxisRaw("Jump");        
        float crouchInput = Input.GetAxisRaw("Vertical");
        float dashInput = Input.GetAxisRaw("Dash");
        
        if (crouchInput == 1){
            crouchInput = 0;
        }
        else if (crouchInput == -1){
            crouchInput = 1;
        }

        actions[2] = (int) crouchInput;
        actions[3] = (int) dashInput;
    }
}
