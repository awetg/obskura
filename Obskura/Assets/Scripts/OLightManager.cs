using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OLightManager : MonoBehaviour {

	public Camera LightCamera;
	public Camera MainCamera;
	public Camera UICamera;
	public float Intensity = 10.0F;
	public Color Overlay = new Color (0.0F, 0.0F, 0.0F);
	private Material material;
	private RenderTexture lightMap;
	private RenderTexture uiTexture;

	// Creates a private material used to the effect
	void Awake ()
	{
		material = new Material( Shader.Find("Custom/SLightTexture") );
		LightCamera.targetTexture = new RenderTexture(LightCamera.pixelWidth, LightCamera.pixelHeight, 24);

		LightCamera.orthographicSize = MainCamera.orthographicSize;
		LightCamera.aspect = MainCamera.aspect;

		UICamera.targetTexture = new RenderTexture(UICamera.pixelWidth, UICamera.pixelHeight, 24);

		UICamera.orthographicSize = MainCamera.orthographicSize;
		UICamera.aspect = MainCamera.aspect;

		RefreshVertices ();
	}

	void Update() {
		if (LightCamera.aspect != MainCamera.aspect || LightCamera.orthographicSize != MainCamera.orthographicSize) {
			LightCamera.orthographicSize = MainCamera.orthographicSize;
			LightCamera.aspect = MainCamera.aspect;
		}
		if (UICamera.aspect != MainCamera.aspect || UICamera.orthographicSize != MainCamera.orthographicSize) {
			UICamera.orthographicSize = MainCamera.orthographicSize;
			UICamera.aspect = MainCamera.aspect;
		}
	}

	// Postprocess the image
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		lightMap = LightCamera.targetTexture;
		uiTexture = UICamera.targetTexture;
		material.SetFloat("_Intensity", Intensity);
		material.SetColor("_Overlay", Overlay);
		material.SetTexture ("_LightTex", lightMap);
		material.SetTexture ("_UITex", uiTexture);
		Graphics.Blit (source, destination, material);
	}

	/// <summary>
	/// Refreshs the vertices.
	/// Call when the shadown casting objects in the map move or change.
	/// </summary>
	public void RefreshVertices(){
		Geometry.CollectVertices ();
	}
}
