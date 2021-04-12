using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ComputeShaderRenderer : MonoBehaviour
{
	public ComputeShader MainShader;
	private RenderTexture _target;
	private Camera _camera;

	private int KERNEL_ID_InitShader, KERNEL_ID_CSMain,
				WIDTH, HEIGHT;

	private void Awake()
	{
		_camera = GetComponent<Camera>();

		// WIDTH = Screen.width;
		// HEIGHT = Screen.height;

		KERNEL_ID_CSMain = MainShader.FindKernel("CSMain");
		KERNEL_ID_InitShader = MainShader.FindKernel("Init");

		Application.targetFrameRate = 20;
	}
	private void SetShaderParameters()
	{
		MainShader.SetInt("width", WIDTH);
		MainShader.SetInt("height", HEIGHT);
		MainShader.SetInt("time",(int)Time.time);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		WIDTH = Screen.width /4;
		HEIGHT = Screen.height /4;

		SetShaderParameters();
		Render(destination);

		// Debug.Log(Mathf.Pow(Time.deltaTime,-1));
	}

	private void Render(RenderTexture destination)
	{
		// Make sure we have a current render target
		InitRenderTexture();
		// Set the target and dispatch the compute shader
		MainShader.SetTexture(0, "Result", _target);

		int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);
		MainShader.Dispatch(KERNEL_ID_CSMain, threadGroupsX, threadGroupsY, 1);
		// Blit the result texture to the screen

		Graphics.Blit(_target, destination);
	}
	private void InitTexture(ref RenderTexture tex, RenderTextureFormat format)
	{
		if (tex == null || tex.width != WIDTH || tex.height != HEIGHT)
		{
			// Release render texture if we already have one
			if (tex != null)
				tex.Release();
			// Get a render target for Ray Tracing

			tex = new RenderTexture(WIDTH, HEIGHT, 0, format, RenderTextureReadWrite.Linear);
			tex.enableRandomWrite = true;
			tex.Create();

			MainShader.SetTexture(1, "Result", _target);

			int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
			int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);

			MainShader.Dispatch(KERNEL_ID_InitShader, threadGroupsX, threadGroupsY, 1);
		}
	}

	private void InitRenderTexture()
	{
		InitTexture(ref _target, RenderTextureFormat.ARGBFloat);
	}
}