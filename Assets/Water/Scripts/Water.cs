using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Water : MonoBehaviour
{
	[SerializeField]
	bool disablePixelLights = true;
	
	[SerializeField][Range(1, 5)]
	int reflectionDownsample = 2;
	
	[SerializeField]
	float clipPlaneOffset = 0.2f; // this is actually a maximum offset
 
	[SerializeField]
	LayerMask m_ReflectLayers = -1;

	[SerializeField]
	WaterSettings settings = WaterSettings.Default;
	public WaterSettings Settings => settings;

	[SerializeField]
	Material surfaceMaterial;

	[SerializeField]
	List<Material> groundMaterials;

	[SerializeField]
	Light sunLight;

	Camera reflectionCamera;
	Dictionary<Camera, RenderTexture> reflTextures = new Dictionary<Camera, RenderTexture>();
	Vector2Int currentTextureDimensions;

	static bool insideRendering;
 
	void OnWillRenderObject()
	{
		if (!surfaceMaterial || !enabled)
			return;

		Camera cam = Camera.current;
		if (!cam)
			return;

		// Safeguard from recursive reflections.
		if (insideRendering)
			return;

		// Check if the object is within the camera's view frustum
		Vector3 screenPos = cam.WorldToViewportPoint(transform.position);
		if (screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1 || screenPos.z < 0)
		{
			return; // Skip rendering if the object is out of view
		}

		// Depth check for near and far planes
		float distanceToCamera = Vector3.Distance(cam.transform.position, transform.position);
		if (distanceToCamera < cam.nearClipPlane || distanceToCamera > cam.farClipPlane)
		{
			return; // Skip rendering if outside the depth range
		}

		insideRendering = true;

		try
		{
			// Reflection texture setup
			var reflTexture = GetRenderTexture(cam);
			surfaceMaterial.SetTexture("_ReflectionTex", reflTexture);

			// Find out the reflection plane: position and normal in world space
			Vector3 pos = transform.position;
			Vector3 normal = cam.transform.position.y > pos.y ? Vector3.up : Vector3.down;

			// Optionally disable pixel lights for reflection
			int oldPixelLightCount = QualitySettings.pixelLightCount;
			if (disablePixelLights)
				QualitySettings.pixelLightCount = 0;

			UpdateCameraModes(cam);

			// Calculate reflection matrix and setup reflection camera
			float d = -Vector3.Dot(normal, pos);
			Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

			Matrix4x4 reflection = Matrix4x4.zero;
			CalculateReflectionMatrix(ref reflection, reflectionPlane);
			Vector3 oldPos = cam.transform.position;
			Vector3 newPos = reflection.MultiplyPoint(oldPos);

			// Clamp the reflection position to avoid extreme values
			newPos.x = Mathf.Clamp(newPos.x, -1000f, 1000f);
			newPos.y = Mathf.Clamp(newPos.y, -1000f, 1000f);
			newPos.z = Mathf.Clamp(newPos.z, -1000f, 1000f);

			reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

			// Set up oblique projection matrix for the reflection camera
			Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal);
			Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
			reflectionCamera.projectionMatrix = projection;

			reflectionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
			reflectionCamera.cullingMask = ~(1 << 4) & m_ReflectLayers.value; // never render water layer

			var oldCulling = GL.invertCulling;
			GL.invertCulling = !oldCulling;
			reflectionCamera.transform.position = newPos;
			Vector3 euler = cam.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

			// Render to texture and reset
			reflectionCamera.targetTexture = reflTexture;
			reflectionCamera.Render();
			reflectionCamera.targetTexture = null;

			// Restore the camera's original position and culling state
			reflectionCamera.transform.position = oldPos;
			GL.invertCulling = oldCulling;

			// Restore pixel light count
			if (disablePixelLights)
				QualitySettings.pixelLightCount = oldPixelLightCount;
		}
		finally
		{
			insideRendering = false;
		}
	}



	void UpdateMaterial()
	{
		if(!sunLight)
			sunLight = FindObjectOfType<Light>();

		// make up our own light direction if no light could be found
		// (just avoids tedious null checks later on)
		var lightDir = sunLight ? sunLight.transform.forward : -Vector3.one.normalized;
		var transmittance = WaterUtility.GetTransmittance(lightDir, settings.sunExtinction.ToVector3());
		
		float sunFade = Mathf.Clamp01((.1f - lightDir.y) * 10);
		float scatterFade = Mathf.Clamp01((.15f - lightDir.y) * 4);

		// uncomment if want to adjust visibility through editor, else initial visibility set in line 38 of WaterSettings.cs 
//		   WaterSettings.controlVisibility = Settings.visibility;

		if (surfaceMaterial)
		{
			surfaceMaterial.SetTexture("_NormalTex", settings.normalTexture);
			surfaceMaterial.SetVector("_WindDirection", settings.windDirection);
			surfaceMaterial.SetFloat("_WindSpeed", settings.windSpeed);
			//surfaceMaterial.SetFloat("_Visibility", settings.visibility);
			surfaceMaterial.SetFloat("_Visibility", WaterSettings.controlVisibility);

			surfaceMaterial.SetFloat("_WaveScale", settings.waveScale);
			surfaceMaterial.SetFloat("_ScatterAmount", settings.scatterAmount);
			surfaceMaterial.SetColor("_ScatterColor", settings.scatterColor);
			surfaceMaterial.SetFloat("_ReflDistortionAmount", settings.reflectionDistortionAmount);
			surfaceMaterial.SetFloat("_RefrDistortionAmount", settings.refractionDistortionAmount);
			surfaceMaterial.SetFloat("_AberrationAmount", settings.aberrationAmount);
			surfaceMaterial.SetColor("_WaterExtinction", settings.waterExtinction);
			surfaceMaterial.SetVector("_SunTransmittance", transmittance);
			surfaceMaterial.SetFloat("_SunFade", sunFade);
			surfaceMaterial.SetFloat("_ScatterFade", scatterFade);
		}

		foreach (Material element in groundMaterials)
		{
			element.SetTexture("_NormalTex", settings.normalTexture);
			element.SetVector("_WindDirection", settings.windDirection);
			element.SetFloat("_WindSpeed", settings.windSpeed);

			//element.SetFloat("_Visibility", settings.visibility);
			element.SetFloat("_Visibility", WaterSettings.controlVisibility);

			element.SetFloat("_WaveScale", settings.waveScale);
			element.SetColor("_MudExtinction", settings.mudExtinction);
			element.SetColor("_WaterExtinction", settings.waterExtinction);
			element.SetVector("_SunTransmittance", transmittance);
			element.SetFloat("_SunFade", sunFade);
			element.SetFloat("_ScatterFade", scatterFade);
		}
	}

	void OnEnable()
	{
		Camera.onPreRender += PreRender;
	}

	void LateUpdate()
	{
		UpdateMaterial();
	}

	void PreRender(Camera c)
	{
		// ignore reflection camera
		if(c != reflectionCamera)
		{
			// cache camera position so reflected shader has access to it
			foreach (Material element in groundMaterials) 
				element.SetVector("_WorldSpaceCameraPos2", c.transform.position);
		}
	}
 
	void OnDisable()
	{
		foreach(var kvp in reflTextures)
			if(kvp.Value)
				DestroyImmediate(kvp.Value);

		reflTextures.Clear();

		if(reflectionCamera)
		{
			DestroyImmediate(reflectionCamera.gameObject);
			reflectionCamera = null;
		}

		Camera.onPreRender -= PreRender;
	}

	void UpdateCameraModes(Camera src)
	{
		if(reflectionCamera == null)
		{
			reflectionCamera = new GameObject("Reflection Camera", typeof(Camera)).GetComponent<Camera>();
			reflectionCamera.enabled = false;
			reflectionCamera.gameObject.hideFlags = HideFlags.DontSave;
		}
		
		reflectionCamera.clearFlags = src.clearFlags;
		reflectionCamera.backgroundColor = src.backgroundColor;        
		
		if(src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox sky = src.GetComponent<Skybox>();
			Skybox reflSky = reflectionCamera.GetComponent<Skybox>();
			
			if(!sky || !sky.material)
			{
				if(reflSky)
					reflSky.enabled = false;
			}
			else
			{
				if(!reflSky)
					reflSky = reflectionCamera.gameObject.AddComponent<Skybox>();
				
				reflSky.enabled = true;
				reflSky.material = sky.material;
			}
		}
		
		reflectionCamera.farClipPlane = src.farClipPlane;
		reflectionCamera.nearClipPlane = src.nearClipPlane;
		reflectionCamera.orthographic = src.orthographic;
		reflectionCamera.fieldOfView = src.fieldOfView;
		reflectionCamera.aspect = src.aspect;
		reflectionCamera.orthographicSize = src.orthographicSize;
	}
 
	RenderTexture GetRenderTexture(Camera currentCamera)
	{
		Vector2Int newTextureDimensions = new Vector2Int(currentCamera.scaledPixelWidth / reflectionDownsample, currentCamera.scaledPixelHeight / reflectionDownsample);
		RenderTexture rt;
		
		if(reflTextures.TryGetValue(currentCamera, out rt))
		{
			if(rt.width != newTextureDimensions.x || rt.height != newTextureDimensions.y)
			{
				DestroyImmediate(rt);
				reflTextures.Remove(currentCamera);
			}

			goto Skip;
		}
		
		rt = new RenderTexture(newTextureDimensions.x, newTextureDimensions.y, 16, RenderTextureFormat.DefaultHDR)
		{
			name = $"PlanarReflection {GetInstanceID()}",
			hideFlags = HideFlags.DontSave
		};
		
		reflTextures.Add(currentCamera, rt);
		
		Skip:
		
		return rt;
	}
 
	Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal)
	{
		// lerp between small clip plane offset and maximum offset based on dist from water plane
		// we want small offset when near surface and large offset when high above surface
		float dist = Mathf.Abs(cam.transform.position.y - pos.y);
		float clipOffset = Mathf.Lerp(.01f, clipPlaneOffset, Mathf.Clamp01(dist));
		
		Matrix4x4 m = cam.worldToCameraMatrix;
		pos = m.MultiplyPoint(pos - normal * clipOffset);
		normal = m.MultiplyVector(normal).normalized;
		
		return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(pos, normal));
	}
 
	static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1F - 2F * plane[0] * plane[0];
		reflectionMat.m01 = -2F * plane[0] * plane[1];
		reflectionMat.m02 = -2F * plane[0] * plane[2];
		reflectionMat.m03 = -2F * plane[3] * plane[0];

		reflectionMat.m10 = -2F * plane[1] * plane[0];
		reflectionMat.m11 = 1F - 2F * plane[1] * plane[1];
		reflectionMat.m12 = -2F * plane[1] * plane[2];
		reflectionMat.m13 = -2F * plane[3] * plane[1];

		reflectionMat.m20 = -2F * plane[2] * plane[0];
		reflectionMat.m21 = -2F * plane[2] * plane[1];
		reflectionMat.m22 = 1F - 2F * plane[2] * plane[2];
		reflectionMat.m23 = -2F * plane[3] * plane[2];
 
		reflectionMat.m30 = 0F;
		reflectionMat.m31 = 0F;
		reflectionMat.m32 = 0F;
		reflectionMat.m33 = 1F;
	}
}