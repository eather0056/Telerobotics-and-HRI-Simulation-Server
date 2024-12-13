using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using TMPro;

public class RobotController : MonoBehaviour
{
    private ROSConnection ros;
    public string topicName = "/bluerov1/cmd_vel";
    
    public float linearSpeed = 0.5f;
    public float angularSpeed = 0.0f;

    // UI Button references
    public Button buttonW;
    public Button buttonA;
    public Button buttonS;
    public Button buttonD;
    public Button buttonX;
    public Button buttonUp;
    public Button buttonDown;
    public Button resetOrientationButton; // New button for resetting orientation

    // UI Slider references
    public Slider linearSpeedSlider;
    public Slider angularSpeedSlider;

    // UI Text references
    public TMP_Text linearSpeedText;
    public TMP_Text angularSpeedText;

    private Quaternion initialRotation; // Store the initial rotation

    void Start()
    {
        // Store the initial rotation of the robot
        initialRotation = transform.rotation;

        // Get or create ROS connection instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);

        // Assign button click listeners
        buttonW.onClick.AddListener(MoveForward);
        buttonA.onClick.AddListener(TurnLeft);
        buttonS.onClick.AddListener(StopMovement);
        buttonD.onClick.AddListener(TurnRight);
        buttonX.onClick.AddListener(MoveBackward);
        buttonUp.onClick.AddListener(MoveUp);
        buttonDown.onClick.AddListener(MoveDown);
        resetOrientationButton.onClick.AddListener(ResetOrientation); // Assign the reset button

        // Set slider min and max values
        linearSpeedSlider.minValue = 0.0f;
        linearSpeedSlider.maxValue = 2.0f;
        angularSpeedSlider.minValue = 0.0f;
        angularSpeedSlider.maxValue = 5.0f;

        // Assign slider listeners
        linearSpeedSlider.onValueChanged.AddListener(UpdateLinearSpeed);
        angularSpeedSlider.onValueChanged.AddListener(UpdateAngularSpeed);

        // Set default slider values to match initial speeds
        linearSpeedSlider.value = linearSpeed;
        angularSpeedSlider.value = angularSpeed;

        // Initialize text values
        linearSpeedText.text = "Linear Speed: " + linearSpeed.ToString("F1");
        angularSpeedText.text = "Angular Speed: " + angularSpeed.ToString("F1");
    }

    void Update()
    {
        // Check for key inputs to control movement
        if (Input.GetKey(KeyCode.W)) MoveForward();
        if (Input.GetKey(KeyCode.A)) TurnLeft();
        if (Input.GetKey(KeyCode.S)) StopMovement();
        if (Input.GetKey(KeyCode.D)) TurnRight();
        if (Input.GetKey(KeyCode.X)) MoveBackward();
        if (Input.GetKey(KeyCode.UpArrow)) MoveUp();
        if (Input.GetKey(KeyCode.DownArrow)) MoveDown();
    }

    // Button functions
    void MoveForward()
    {
        TwistMsg twist = new TwistMsg();
        Vector3 forwardDirection = transform.forward * linearSpeed;
        twist.linear.x = forwardDirection.x;
        twist.linear.y = forwardDirection.y;
        twist.linear.z = forwardDirection.z;
        ros.Publish(topicName, twist);
    }

    void TurnLeft()
    {
        TwistMsg twist = new TwistMsg();
        twist.angular.z = angularSpeed;
        ros.Publish(topicName, twist);
    }

    void TurnRight()
    {
        TwistMsg twist = new TwistMsg();
        twist.angular.z = -angularSpeed;
        ros.Publish(topicName, twist);
    }

    void StopMovement()
    {
        TwistMsg twist = new TwistMsg();
        ros.Publish(topicName, twist);  // Publish stop command with zeroed twist
    }

    void MoveBackward()
    {
        TwistMsg twist = new TwistMsg();
        Vector3 backwardDirection = -transform.forward * linearSpeed;
        twist.linear.x = backwardDirection.x;
        twist.linear.y = backwardDirection.y;
        twist.linear.z = backwardDirection.z;
        ros.Publish(topicName, twist);
    }

    void MoveUp()
    {
        TwistMsg twist = new TwistMsg();
        twist.linear.y = linearSpeed;
        ros.Publish(topicName, twist);
    }

    void MoveDown()
    {
        TwistMsg twist = new TwistMsg();
        twist.linear.y = -linearSpeed;
        ros.Publish(topicName, twist);
    }

    // Reset orientation function
    void ResetOrientation()
    {
        transform.rotation = initialRotation;
    }

    // Slider update functions
    public void UpdateLinearSpeed(float newSpeed)
    {
        linearSpeed = newSpeed;
        linearSpeedText.text = "Linear Speed: " + linearSpeed.ToString("F1");
    }

    public void UpdateAngularSpeed(float newSpeed)
    {
        angularSpeed = newSpeed;
        angularSpeedText.text = "Angular Speed: " + angularSpeed.ToString("F1");
    }
}
