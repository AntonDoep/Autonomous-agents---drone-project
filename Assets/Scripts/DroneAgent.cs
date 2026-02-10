using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections; // npt used
public class DroneAgent : Agent
{
    Rigidbody rb;
    public Transform targetPlatform;
    public float UpThrust = 11.5f;
    public float DownThrust = 9.1f;
    public float TurnAngle = 45f;

    private Vector3 startPos;
    private float prevDistance;

    private float backForth = 0f;
    private float leftRight = 0f;
    private float Thrust = 0f;

    // Variables for rewarding based on time spent on platform
    //private bool onPlatform = false;
    //private float platformLandingTime = 0f;
    //private float maxLandingTime = 5f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startPos + new Vector3(0, 1f, 0);
        transform.rotation = Quaternion.identity; //necessary?
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        prevDistance = Vector3.Distance(transform.localPosition, targetPlatform.localPosition);

        // Randomize target platform position
        if (targetPlatform != null)
        {
            float x = Random.Range(-3f, 3f);
            float z = Random.Range(-3f, 3f);
            float y = Random.Range(0.5f, 5f);
            targetPlatform.localPosition = new Vector3(x, y, z);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var act = actions.DiscreteActions;

        // Reset orientation each step
        rb.angularVelocity = Vector3.zero;

        // Thrust control
        switch (act[0])
        {
            case 1: Thrust = UpThrust; break;
            case 2: Thrust = DownThrust; break;
            default: Thrust = 9.82f; break; 
        }

        // Direction control
        backForth = 0f;
        leftRight = 0f;

        switch (act[1])
        {
            case 1: leftRight = TurnAngle; break;   // left
            case 2: leftRight = -TurnAngle; break;  // right
            case 3: backForth = TurnAngle; break;   // forward
            case 4: backForth = -TurnAngle; break;  // backward
            default: break;
        }

        // Distance reward shaping
        float distance = Vector3.Distance(transform.localPosition, targetPlatform.localPosition);
        float delta = prevDistance - distance;
        
        if (prevDistance != 0)
            AddReward(delta * 0.2f); // reward for moving closer

        prevDistance = distance;
       
        //TRODDE DETTA ORSAKA ATT DEN INT LANDA O RESETTA POSITION MEN DE VA ÖR ATT ISTRIGGER TUSEN BULLAR!
        //Reward for being above platform
        float heightDiff = transform.localPosition.y - targetPlatform.localPosition.y;
        if (heightDiff < 0)
            AddReward(-0.003f); 
        else
            AddReward(0.002f);         


        
        // Out of bounds
        if (transform.localPosition.y < -1f || transform.localPosition.y > 30f)
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Relative position to target
        sensor.AddObservation(targetPlatform.localPosition - transform.localPosition);

        // Drone velocity
        sensor.AddObservation(rb.linearVelocity);

        // Orientation
        sensor.AddObservation(transform.localRotation.normalized);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.DiscreteActions;
        a[0] = 0;
        a[1] = 0;

        if (Input.GetKey(KeyCode.W)) a[0] = 1; 
        if (Input.GetKey(KeyCode.S)) a[0] = 2; 
        if (Input.GetKey(KeyCode.LeftArrow)) a[1] = 1;
        if (Input.GetKey(KeyCode.RightArrow)) a[1] = 2;
        if (Input.GetKey(KeyCode.UpArrow)) a[1] = 3;
        if (Input.GetKey(KeyCode.DownArrow)) a[1] = 4;
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(0f, Thrust, 0f);
        var targetRot = Quaternion.Euler(backForth, transform.eulerAngles.y, leftRight);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 8f));

    }/*
    private IEnumerator LandAndFinish() // AI suggestion for controlled landing. Unsuccessful in my case.
    {
        // Stop any active forces
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Give a strong landing reward immediately
        AddReward(1.0f);

        // Stay on the platform for a bit while gravity holds it down
        yield return new WaitForSeconds(1.0f);

        AddReward(2.0f); // bonus for staying still
        EndEpisode();
    }*/
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Finish"))
        {
            AddReward(1f);
            //AddReward(0.1f * Time.fixedDeltaTime); // reward for staying on platform
            EndEpisode();

        }/*
        if (col.gameObject.CompareTag("Finish"))
        {
            
            StartCoroutine(LandAndFinish());
        }*/
        if (col.gameObject.CompareTag("Ground"))
        {
            AddReward(-1f);  // penalize hitting ground
            EndEpisode();
        }

        if (col.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }


    //This solution didnt work for me
/*    void OnCollisionStay(Collision col) // This solution ends with the drone just bouncing around on platform and never ends episode...
    {
        if (col.gameObject.CompareTag("Finish"))
        {
            AddReward(0.002f);
            platformLandingTime += Time.fixedDeltaTime;

            if (platformLandingTime >= 6f)
            {
                AddReward(2f);
                EndEpisode();
            }
        }
    }
    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Finish"))
            platformLandingTime = 0f;
    }*/
    
}