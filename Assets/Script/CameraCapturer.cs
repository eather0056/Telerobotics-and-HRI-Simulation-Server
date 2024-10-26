using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
using System.Collections;

public class CameraRosPublisher : MonoBehaviour
{
    public string topicName = "/camera/image/compressed";
    public int resolutionWidth = 640;
    public int resolutionHeight = 480;
    public int qualityLevel = 75; // JPEG quality level (0-100)
    
    // Use 'new' keyword to hide inherited member 'Component.camera'
    public new Camera camera;
    public float publishRate = 0.1f; // Publish every 0.1 seconds (10 Hz)

    private ROSConnection ros;
    private Texture2D texture2D;
    private CompressedImageMsg compressedImageMessage;
    private WaitForSeconds publishWait;

    void Start()
    {
        if (camera == null)
        {
            camera = GetComponent<Camera>();
        }

        // Initialize ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(topicName);

        // Initialize Texture2D
        texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);

        // Initialize WaitForSeconds
        publishWait = new WaitForSeconds(publishRate);

        // Start the publishing coroutine
        StartCoroutine(PublishImage());
    }

    IEnumerator PublishImage()
    {
        while (true)
        {
            CaptureAndPublishImage();
            yield return publishWait;
        }
    }

    void CaptureAndPublishImage()
    {
        // Set the target texture of the camera
        RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        camera.targetTexture = rt;

        // Render the camera's view
        camera.Render();

        // Activate the RenderTexture and read the pixels into the Texture2D
        RenderTexture.active = rt;
        texture2D.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture2D.Apply();

        // Clean up
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Encode the Texture2D to JPEG
        byte[] imageBytes = texture2D.EncodeToJPG(qualityLevel);

        // Create the CompressedImage message
        compressedImageMessage = new CompressedImageMsg();
        compressedImageMessage.header = new HeaderMsg();
        compressedImageMessage.header.stamp = new TimeMsg();

        // Set the timestamp (optional but recommended)
        compressedImageMessage.header.stamp.sec = (int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        compressedImageMessage.header.stamp.nanosec = (uint)((System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000) * 1000000);

        // Set the frame ID (optional)
        compressedImageMessage.header.frame_id = "camera_frame";

        // Set the format and data fields
        compressedImageMessage.format = "jpeg";
        compressedImageMessage.data = imageBytes;

        // Publish the message
        ros.Publish(topicName, compressedImageMessage);
    }
}
