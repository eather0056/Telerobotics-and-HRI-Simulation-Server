using TMPro;  // Add this at the top if you're using TextMeshPro
using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;

public class VelocityDisplay : MonoBehaviour
{
    public TextMeshProUGUI velocityText;  // Use TextMeshProUGUI instead of Text
    ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>("/bluerov1/cmd_vel", UpdateVelocityDisplay);
    }

    void UpdateVelocityDisplay(TwistMsg twistMsg)
    {
        string velocityInfo = $"Linear Velocity: {twistMsg.linear.x:F2}\nAngular Velocity: {twistMsg.angular.z:F2}";
        velocityText.text = velocityInfo;
    }
}
