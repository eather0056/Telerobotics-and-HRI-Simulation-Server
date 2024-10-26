using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RobotController : MonoBehaviour
{
    private ROSConnection ros;
    public string topicName = "/cmd_vel";
    public float linearSpeed = 0.5f;
    public float angularSpeed = 1.0f;

    void Start()
    {
        // Get or create ROS connection instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);
    }

    void Update()
    {
        // Create a new Twist message
        TwistMsg twist = new TwistMsg();

        // Check for key inputs
        bool keyPressed = false;

        if (Input.GetKey(KeyCode.W))
        {
            twist.linear.x = linearSpeed;
            keyPressed = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            twist.linear.x = -linearSpeed;
            keyPressed = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            twist.angular.z = angularSpeed;
            keyPressed = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            twist.angular.z = -angularSpeed;
            keyPressed = true;
        }

        // Publish only when a key is pressed
        if (keyPressed)
        {
            ros.Publish(topicName, twist);
        }
    }
}
