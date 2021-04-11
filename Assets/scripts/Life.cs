using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
	public ComputeShader MainShader;
	private RenderTexture Result;
	private RenderTexture Intermediate;
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

		Application.targetFrameRate = 10000;
	}
	private void SetShaderParameters()
	{
		MainShader.SetInt("width", WIDTH);
		MainShader.SetInt("height", HEIGHT);
		MainShader.SetInt("time", (int)Time.time);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		WIDTH = Screen.width * 2;
		HEIGHT = Screen.height * 2;

		SetShaderParameters();
		Render(destination);

		// Debug.Log(Mathf.Pow(Time.deltaTime,-1));
	}

	private void Render(RenderTexture destination)
	{
		InitRenderTexture();
		// Make sure we have a current render target
		Graphics.Blit(Result, Intermediate);
		// Set the target and dispatch the compute shader
		MainShader.SetTexture(0, "Result", Result);
		MainShader.SetTexture(0, "Intermediate", Intermediate);

		int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
		int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);
		MainShader.Dispatch(KERNEL_ID_CSMain, threadGroupsX, threadGroupsY, 1);
		// Blit the result texture to the screen

		Graphics.Blit(Result, destination);
	}
	private bool InitTexture(ref RenderTexture tex, int width, int height)
	{
		if (tex == null || tex.width != WIDTH || tex.height != HEIGHT)
		{
			// Release render texture if we already have one
			if (tex != null)
				tex.Release();
			// Get a render target for Ray Tracing

			tex = new RenderTexture(WIDTH, HEIGHT, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
			tex.enableRandomWrite = true;
			tex.Create();
			return true;
		}
		return false;
	}

	private void InitRenderTexture()
	{
		if (InitTexture(ref Result, WIDTH, HEIGHT))
		{
			MainShader.SetTexture(1, "Result", Result);

			int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
			int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);

			MainShader.Dispatch(KERNEL_ID_InitShader, threadGroupsX, threadGroupsY, 1);
		}
		InitTexture(ref Intermediate, WIDTH, HEIGHT);
	}
}
