using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OLightManager : MonoBehaviour {

	public Camera LightCamera;
	public Camera MainCamera;
	public float Intensity = 10.0F;
	public Color Overlay = new Color (0.0F, 0.0F, 0.0F);
	private Material material;
	private RenderTexture lightMap;

	// Creates a private material used to the effect
	void Awake ()
	{
		material = new Material( Shader.Find("Custom/SLightTexture") );
		LightCamera.targetTexture = new RenderTexture(LightCamera.pixelWidth, LightCamera.pixelHeight, 24);

		LightCamera.orthographicSize = MainCamera.orthographicSize;
		LightCamera.aspect = MainCamera.aspect;
	}

	void Update() {
		if (LightCamera.aspect != MainCamera.aspect || LightCamera.orthographicSize != MainCamera.orthographicSize) {
			LightCamera.orthographicSize = MainCamera.orthographicSize;
			LightCamera.aspect = MainCamera.aspect;
		}
	}

	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		lightMap = LightCamera.targetTexture;
		material.SetFloat("_Intensity", Intensity);
		material.SetColor("_Overlay", Overlay);
		material.SetTexture ("_LightTex", lightMap);
		Graphics.Blit (source, destination, material);
	}
}
