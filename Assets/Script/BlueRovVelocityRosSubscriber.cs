using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class BlueRovVelocityRosSubscriber : MonoBehaviour
{
    public BlueRovVelocityControl blueRovVelocityControl;

    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        
        ros.Subscribe<TwistMsg>("/bluerov1/cmd_vel", MoveBlueRov);
    }

    void MoveBlueRov(TwistMsg velocityMessage)
    {
        // Call MoveVelocity from BlueRovVelocityControl
        blueRovVelocityControl.MoveVelocity(velocityMessage);
    }
}
