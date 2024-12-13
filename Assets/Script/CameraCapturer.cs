using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;
using System.Collections;

public class CameraRosPublisher : MonoBehaviour
{
    public string topicName = "/camera/image/compressed";
    public int resolutionWidth = 640; // Desired resolution width
    public int resolutionHeight = 480; // Desired resolution height
    public int qualityLevel = 75; // JPEG quality level (0-100)

    public new Camera camera; // Ensure this is your Unity camera
    public float publishRate = 0.1f; // Publish every 0.1 seconds (10 Hz)

    private ROSConnection ros;
    private Texture2D texture2D;
    private CompressedImageMsg compressedImageMessage;
    private WaitForSeconds publishWait;

    void Start()
    {
        // Ensure a Camera component is assigned
        if (camera == null)
        {
            camera = GetComponent<Camera>();
        }

        // Ensure camera aspect ratio matches the resolution
        camera.aspect = (float)resolutionWidth / resolutionHeight;

        // Ensure the camera viewport covers the full screen
        camera.rect = new Rect(0, 0, 1, 1);

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
        // Create a RenderTexture matching the desired resolution
        RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        camera.targetTexture = rt;

        // Render the camera's view
        camera.Render();

        // Activate the RenderTexture and read the pixels into the Texture2D
        RenderTexture.active = rt;
        texture2D.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture2D.Apply();

        // Clean up RenderTexture
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Encode the Texture2D to JPEG
        byte[] imageBytes = texture2D.EncodeToJPG(qualityLevel);

        // Create the CompressedImage message
        compressedImageMessage = new CompressedImageMsg
        {
            header = new HeaderMsg
            {
                stamp = new TimeMsg
                {
                    sec = (int)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    nanosec = (uint)((System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000) * 1000000)
                },
                frame_id = "camera_frame"
            },
            format = "jpeg",
            data = imageBytes
        };

        // Publish the message
        ros.Publish(topicName, compressedImageMessage);
    }

    void OnGUI()
    {
        // Optional: Display the captured image in Unity for debugging
        //GUI.DrawTexture(new Rect(0, 0, resolutionWidth, resolutionHeight), texture2D, ScaleMode.ScaleToFit);
    }
}
