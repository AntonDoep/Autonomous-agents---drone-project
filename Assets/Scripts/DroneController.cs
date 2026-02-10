using Unity.VisualScripting;
using UnityEngine;


public class DroneController : MonoBehaviour
{
    Rigidbody rb;
    public float UpThrust = 10.5f;
    public float DownThrust = 9.1f;
    public float TurnAngle = 3;

    float backForth = 0;
    float leftRight = 0;
    float Thrust = 0;

     public bool ManualControl = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        if (!ManualControl) return;
        transform.rotation = Quaternion.Euler(backForth, 0, leftRight);

        Thrust = 9.82f;
        backForth = 0;
        leftRight = 0;


        //Move drone up
        if (Input.GetKey(KeyCode.W))
        {
            Thrust = UpThrust;
        }

        //Move drone down
        if (Input.GetKey(KeyCode.S))
        {
            Thrust = DownThrust;
        }

        //Move drown left
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            leftRight = TurnAngle;
        }

        //Move drone right
        if (Input.GetKey(KeyCode.RightArrow))
        {
            leftRight = -TurnAngle;
        }

        //Move drone forward
        if (Input.GetKey(KeyCode.UpArrow))
        {
            backForth = TurnAngle;
        }

        //Move drone backwards
        if (Input.GetKey(KeyCode.DownArrow))
        {
            backForth = -TurnAngle;
        }
    }

    private void FixedUpdate()
    {   
        if (!ManualControl) return;
        rb.AddRelativeForce(0, Thrust, 0);
    }
}
