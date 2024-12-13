using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class BlueRovVelocityControl : MonoBehaviour
{
    public float lvx = 0.0f;
    public float lvy = 0.0f;
    public float lvz = 0.0f;
    public float avz = 0.0f;
    public bool movementActive = false;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing on BlueRov2. Please add a Rigidbody.");
        }
    }

    private void MoveVelocityRigidbody()
    {
        if (rb == null) return;

        // Invert the lvz value to correct forward/backward direction
        Vector3 movement = new Vector3(lvx * Time.deltaTime, lvy * Time.deltaTime, lvz * Time.deltaTime);
        rb.MovePosition(rb.position + movement);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, avz * Time.deltaTime, 0));
    }

    public void MoveVelocity(TwistMsg velocityMessage)
    {
        lvx = (float)velocityMessage.linear.x;
        lvy = (float)velocityMessage.linear.y;
        lvz = (float)velocityMessage.linear.z;
        avz = (float)velocityMessage.angular.z;
        movementActive = true;
    }

    void FixedUpdate()
    {
        if (movementActive)
        {
            MoveVelocityRigidbody();
        }
    }
}
