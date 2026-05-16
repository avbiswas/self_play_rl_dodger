using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System;

public class ShootPlayerAgent : Agent
{
    PlayerControl control;
    [SerializeField] BoxCollider2D bulletDodgeTracker;
    float stayAliveReward = 0f; // -0.0001f;
    float dodgeReward = 0f; // 0.0002f;
    
    UIManager2 uIManager;
    public int team_id;
    int rewards_queued = 0;
    float startY, startX;
    float localStartX;
    Rigidbody2D rb;
    [SerializeField] int maxBulletsTracked = 4;
    [SerializeField] int maxBackBulletsTracked = 2;
    [SerializeField] int opponentBulletsTracked = 2;
    
    
    Vector2 facingCorrection; 
    [SerializeField] float dashPenalty;
    CapsuleCollider2D bodyCollider;
    int numSteps = 0;
    [SerializeField] Transform opponentTransform;
    Rigidbody2D opponentRb;
    PlayerControl opponentControl;
    ShootPlayerAgent opponentShooterAgent;
    Dictionary<string, (int, int)> obs_string;

    EnvironmentManager environmentManager;
    void Start(){
        startX = transform.position.x;
        localStartX = transform.localPosition.x;
        startY = transform.position.y;
        rb = GetComponent<Rigidbody2D>();
        control = GetComponent<PlayerControl>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        uIManager = FindObjectOfType<UIManager2>();
        environmentManager = FindObjectOfType<EnvironmentManager>();
        facingCorrection = new Vector2(
            control.GetFacingDirection(), 1);
        opponentRb = opponentTransform.gameObject.GetComponent<Rigidbody2D>();
        opponentControl = opponentTransform.gameObject.GetComponent<PlayerControl>();
        opponentShooterAgent = opponentTransform.gameObject.GetComponent<ShootPlayerAgent>();
        team_id = GetComponent<BehaviorParameters>().TeamId;
        
        // Dictionary containing each entity in vector observation array
        // And the (start, end) tuple denoting where they occur
        // in the observation array
        obs_string = new Dictionary<string, (int, int)>();

        obs_string.Add("LocalPos", (0, 2));
        obs_string.Add("LocalVel", (2, 4));
        obs_string.Add("crouch?", (4, 5));
        obs_string.Add("canDash?", (5, 6));
        obs_string.Add("dashing?", (6, 7));
        obs_string.Add("time2nextDash?", (7, 8));
        obs_string.Add("distFrmEdge?", (8, 10));
        obs_string.Add("Bullet 1", (10, 16));
        obs_string.Add("Bullet 2", (16, 22));
        obs_string.Add("Bullet 3", (22, 28));
        obs_string.Add("Back Bullet", (28, 32));
    }
    public override void OnEpisodeBegin()
    {
        numSteps = 0;
        transform.position = new Vector2(startX, startY);
        rb.velocity = new Vector2(0, 0);
        control.SetAlive();
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(4, 1, !control.IsCrouching());
    }


    public override void OnActionReceived(ActionBuffers actions){
        int moveInput = actions.DiscreteActions[0];
        int jumpControl = actions.DiscreteActions[1];
        int crouchControl = actions.DiscreteActions[2];
        int dashControl = actions.DiscreteActions[3];
        int shootControl = actions.DiscreteActions[4];

        if (shootControl == 1){
            control.FireBullet();
        }
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
        
        /*
        float energy = Vector2.Dot(rb.velocity, rb.velocity)/100;
        float energy_reward = 0;
        if (energy > 0.4){
            energy_reward = energyPenalty * Mathf.Exp(-energy);
            energy_reward = Mathf.Min(energyPenalty, Mathf.Max(energy_reward, 0));
        }
        AddReward(energy_reward);

        */
        if (jumpControl != 0){
            control.Jump();
        }
        else{
            control.JumpSpeedCancel();
        }
        
        control.Crouch(crouchControl);
        int dashDir = (moveInput == 1)? 1 : (moveInput == 0)? 0: -1;
        control.Dash(dashControl, dashDir);

        if (rewards_queued > 0){
            //AddReward(rewards_queued * dodgeReward);
            //opponentShooterAgent.AddReward(-rewards_queued * dodgeReward);
            uIManager.AddDodge(team_id, rewards_queued);
            rewards_queued = 0;
        }
        if (!control.IsAlive()){
            environmentManager.Resolve(team_id);
        }
        //else{
        //    AddReward(stayAliveReward);
        //}

        if (StepCount == MaxStep){
            Debug.Log("Total Reward Finished for Player " + team_id + " is : " + GetCumulativeReward());
            EpisodeInterrupted();
        }
        
    }
    
    public void RequestObservationForOpponent(VectorSensor opponents_sensor){
        // Opponent local position
        opponents_sensor.AddObservation(transform.localPosition.x/(control.maxX - control.minX) * facingCorrection.x);
        opponents_sensor.AddObservation(transform.localPosition.y/3f);

        // Opponent local velocity
        opponents_sensor.AddObservation(rb.velocity.x/control.MaxSpeed()  * facingCorrection.x);
        opponents_sensor.AddObservation(rb.velocity.y/control.MaxSpeed());

        // Opponent relative position
        opponents_sensor.AddObservation(Mathf.Abs(transform.localPosition.x - opponentTransform.localPosition.x)/(control.maxX - control.minX));
        opponents_sensor.AddObservation((transform.localPosition.y - opponentTransform.localPosition.y)/3f);

        // Opponent relative velocity
        opponents_sensor.AddObservation((rb.velocity.x - opponentRb.velocity.x)/control.MaxSpeed() * facingCorrection.x);
        opponents_sensor.AddObservation((rb.velocity.y - opponentRb.velocity.y)/control.MaxSpeed());

        // Opponent state: canDash, isDashing, isJumping, isCrouching
        opponents_sensor.AddObservation(control.CanDash());
        opponents_sensor.AddObservation(control.IsDashing());
        opponents_sensor.AddObservation(control.IsJumping());
        opponents_sensor.AddObservation(control.IsCrouching());

        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(0.1f, 12), 0, Vector2.right * facingCorrection, 40, 
            LayerMask.GetMask(control.adversarialBulletTag));
        int hitsEncountered = 0;
        for (int i = 0; i < hits.Length; i++){
            RaycastHit2D h = hits[i];
            if (h.collider != null){
                Vector2 relative = (Vector2) transform.position - h.point;
                relative.Normalize();
                float ttc = control.ttc_to_ttd(control.ttc(h.collider.gameObject));
                
                opponents_sensor.AddObservation(h.fraction);
                opponents_sensor.AddObservation((Vector2)relative * facingCorrection);
                opponents_sensor.AddObservation(ttc);

                hitsEncountered += 1;
                if (hitsEncountered == opponentBulletsTracked)
                    break;
            }
        }
        for (int i=hitsEncountered; i < opponentBulletsTracked; i ++){
            opponents_sensor.AddObservation(1.0f);
            opponents_sensor.AddObservation(new Vector2(-1, 0));
            opponents_sensor.AddObservation(0);
            
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
                
                //Vector2 closestPoint = (Vector2) bodyCollider.ClosestPoint(h.point) - h.point;
                //float closestDistance = closestPoint.magnitude/40;
                //closestPoint.Normalize();

                float ttc = control.ttc_to_ttd(control.ttc(h.collider.gameObject));
                float duckDistance = (control.GetCrouchableHeight() - (h.point.y - 1))/6f;

                sensor.AddObservation(h.fraction);
                sensor.AddObservation((Vector2)relative * facingCorrection);
                sensor.AddObservation(Mathf.Abs(bulletVelocity.x/20));
                //sensor.AddObservation(closestDistance);
                //sensor.AddObservation((Vector2)closestPoint * facingCorrection);
                sensor.AddObservation(ttc);
                sensor.AddObservation(duckDistance);
                // Debug.Log("Player " + team_id + ": " + h.fraction + ", " + (Vector2)relative * facingCorrection + ", " + Mathf.Abs(bulletVelocity.x/20) + ", " + ttc);
                hitsEncountered += 1;
                if (hitsEncountered == maxBulletsTracked)
                    break;
            }
        }
        for (int i=hitsEncountered; i < maxBulletsTracked; i ++){
            sensor.AddObservation(1.0f);
            sensor.AddObservation(new Vector2(-1, 0));
            sensor.AddObservation(0);
            //sensor.AddObservation(1.0f);
            //sensor.AddObservation(new Vector2(-1, 0));
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

                float ttc = control.ttc_to_ttd(control.ttc(h.collider.gameObject));
                sensor.AddObservation(h.fraction);
                sensor.AddObservation((Vector2)relative * facingCorrection);
                //sensor.AddObservation(Mathf.Abs(bulletVelocity.x/20));
                sensor.AddObservation(ttc);

                backHitsEncountered += 1;
                if (backHitsEncountered == maxBackBulletsTracked)
                    break;
            }
        }

        for (int i=backHitsEncountered; i < maxBackBulletsTracked; i ++){
            sensor.AddObservation(1.0f);
            sensor.AddObservation(new Vector2(-1, 0));
            //sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        
        opponentShooterAgent.RequestObservationForOpponent(sensor);
        debug_observations();
    }

    void debug_observations(){
        if (team_id == 1){
            float[] arr = new float[68];
            GetObservations().CopyTo(arr, 0);
            string statement = "Obs for " + team_id + ", Step: " + StepCount + "\n";
            foreach (KeyValuePair<string, (int, int)> entry in obs_string)
            {
                string key = entry.Key;
                int start = entry.Value.Item1;
                int end = entry.Value.Item2;
                var segment = new ArraySegment<float>(arr, start, (end - start));
                statement += key + ": " + string.Join(", ", segment) + "\n";
            }
            Debug.Log(statement);
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
        actions[4] = (int)  Input.GetAxisRaw("Fire1"); 
    }
}
