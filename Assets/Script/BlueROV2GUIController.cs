// This Unity script creates a similar GUI setup using Unity's UI system
// Unity version 2020.3 or newer is recommended

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;

public class BlueROV2GUIController : MonoBehaviour
{
    public Slider depthSlider;
    public Slider pwmGainSlider;

    public TextMeshProUGUI depthLabel;
    public TextMeshProUGUI pwmGainLabel;
    public TextMeshProUGUI arucoDistanceLabel;
    public TextMeshProUGUI voltageLabel;
    public TextMeshProUGUI pitchLabel;
    public TextMeshProUGUI rollLabel;
    public TextMeshProUGUI yawLabel;

    public RawImage normalImage;
    public RawImage detectedImage;

    public Button forwardButton;
    public Button backwardButton;
    public Button leftButton;
    public Button rightButton;
    public Button yawLeftButton;
    public Button yawRightButton;
    public Button stopButton;
    public Button lightsUpButton;
    public Button lightsDownButton;
    public Button gripperOpenButton;
    public Button gripperCloseButton;
    public Button armButton;
    public Button disarmButton;
    public Button manualModeButton;
    public Button arucoModeButton;

    private float depth;
    private float pwmGain;
    private float arucoDistance;
    private float voltage;
    private float pitch;
    private float roll;
    private float yaw;

    private ROSConnection ros;

    void Start()
    {
        // Initialize ROS connection
        ros = ROSConnection.GetOrCreateInstance();

        // Define publishers
        ros.RegisterPublisher<UInt16Msg>("/settings/pwm_gain");
        ros.RegisterPublisher<Float64Msg>("/settings/depth/set_depth");
        ros.RegisterPublisher<StringMsg>("/settings/control_mode");
        ros.RegisterPublisher<BoolMsg>("/bluerov2/arm");
        ros.RegisterPublisher<UInt16Msg>("/bluerov2/rc/lights");
        ros.RegisterPublisher<UInt16Msg>("/bluerov2/rc/gripper");
        ros.RegisterPublisher<UInt16Msg>("/bluerov2/rc/forward");
        ros.RegisterPublisher<UInt16Msg>("/bluerov2/rc/lateral");
        ros.RegisterPublisher<UInt16Msg>("/bluerov2/rc/yaw");

        // Initialize UI components and assign event listeners
        depthSlider.onValueChanged.AddListener(OnDepthChanged);
        pwmGainSlider.onValueChanged.AddListener(OnPWMGainChanged);

        forwardButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/forward", 1600));
        backwardButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/forward", 1400));
        leftButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/lateral", 1400));
        rightButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/lateral", 1600));
        yawLeftButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/yaw", 1400));
        yawRightButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/yaw", 1600));
        
        stopButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/forward", 1500));
        stopButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/lateral", 1500));
        stopButton.onClick.AddListener(() => SendMovementCommand("/bluerov2/rc/yaw", 1500));

        lightsUpButton.onClick.AddListener(() => AdjustLights(true));
        lightsDownButton.onClick.AddListener(() => AdjustLights(false));
        gripperOpenButton.onClick.AddListener(() => AdjustGripper(true));
        gripperCloseButton.onClick.AddListener(() => AdjustGripper(false));

        armButton.onClick.AddListener(Arm);
        disarmButton.onClick.AddListener(Disarm);
        manualModeButton.onClick.AddListener(() => SwitchMode("manual"));
        arucoModeButton.onClick.AddListener(() => SwitchMode("aruco"));
    }

    void Update()
    {
        // Update UI labels dynamically
        depthLabel.text = $"Depth: {depth:F2} m";
        pwmGainLabel.text = $"PWM Gain: {pwmGain:F0}";
        arucoDistanceLabel.text = $"Distance: {arucoDistance:F2} m";
        voltageLabel.text = $"Voltage: {voltage:F2} V";
        pitchLabel.text = $"Pitch: {pitch:F2}";
        rollLabel.text = $"Roll: {roll:F2}";
        yawLabel.text = $"Yaw: {yaw:F2}";
    }

    void OnDepthChanged(float value)
    {
        depth = value;
        var msg = new Float64Msg(depth);
        ros.Publish("/settings/depth/set_depth", msg);
    }

    void OnPWMGainChanged(float value)
    {
        pwmGain = value;
        var msg = new UInt16Msg((ushort)pwmGain);
        ros.Publish("/settings/pwm_gain", msg);
    }

    void SendMovementCommand(string topic, ushort value)
    {
        var msg = new UInt16Msg(value);
        ros.Publish(topic, msg);
        Debug.Log($"Published {value} to {topic}");
    }

    void AdjustLights(bool increase)
    {
        var value = (ushort)(increase ? 1900 : 1100);
        var msg = new UInt16Msg(value);
        ros.Publish("/bluerov2/rc/lights", msg);
    }

    void AdjustGripper(bool open)
    {
        var value = (ushort)(open ? 1900 : 1100);
        var msg = new UInt16Msg(value);
        ros.Publish("/bluerov2/rc/gripper", msg);
    }

    void Arm()
    {
        var msg = new BoolMsg(true);
        ros.Publish("/bluerov2/arm", msg);
        Debug.Log("Armed");
    }

    void Disarm()
    {
        var msg = new BoolMsg(false);
        ros.Publish("/bluerov2/arm", msg);
        Debug.Log("Disarmed");
    }

    void SwitchMode(string mode)
    {
        var msg = new StringMsg(mode);
        ros.Publish("/settings/control_mode", msg);
        Debug.Log($"Mode switched to: {mode}");
    }
}
