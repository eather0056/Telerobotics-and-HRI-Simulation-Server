using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class CameraImageSubscriber : MonoBehaviour
{
    ROSConnection ros;
    public string compressedImageTopic = "/camera/image/compressed";
    public RawImage rawImage; // Drag and drop the UI RawImage object here.
    
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<CompressedImageMsg>(compressedImageTopic, UpdateCameraFeed);
    }

    void UpdateCameraFeed(CompressedImageMsg imageMessage)
    {
        // Convert the compressed image data to a Texture2D
        byte[] imageData = imageMessage.data;
        Texture2D texture = new Texture2D(2, 2);
        
        // Load the image data into the texture
        texture.LoadImage(imageData);
        
        // Apply the texture to the UI RawImage
        rawImage.texture = texture;
        
        // Scale the image to fit the RawImage object
        RectTransform rawImageRect = rawImage.GetComponent<RectTransform>();

        // Create a scaled texture size, keeping the aspect ratio
        float textureAspect = (float)texture.width / (float)texture.height;
        float rawImageAspect = rawImageRect.rect.width / rawImageRect.rect.height;

        if (textureAspect > rawImageAspect)
        {
            // If texture is wider than the RawImage, fit by width
            rawImageRect.sizeDelta = new Vector2(rawImageRect.rect.width, rawImageRect.rect.width / textureAspect);
        }
        else
        {
            // If texture is taller than the RawImage, fit by height
            rawImageRect.sizeDelta = new Vector2(rawImageRect.rect.height * textureAspect, rawImageRect.rect.height);
        }
    }
}
